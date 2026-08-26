using Citiz.Core.Content;
using Citiz.Core.Discovery;

namespace Citiz.Discovery;

/// <summary>
/// Chooses what to discover. The daily pick is a function of the date alone, so every learner sees
/// the same capsule on the same day and no profile is needed to compute it; the design's privacy
/// rule that recommendations never infer sensitive traits is satisfied by not inferring anything.
/// </summary>
public sealed class DiscoveryEngine
{
    /// <summary>
    /// The capsule for <paramref name="date"/>: eligible topics in file order, cycled by day number.
    /// Returns <c>null</c> when nothing is eligible.
    /// </summary>
    /// <param name="topics">All capsules.</param>
    /// <param name="date">The day.</param>
    /// <param name="minimumStatus">The lowest review state to show; <see cref="ReviewStatus.Outdated"/> is never shown.</param>
    public DiscoveryTopic? SelectDaily(IReadOnlyList<DiscoveryTopic> topics, DateOnly date, ReviewStatus minimumStatus = ReviewStatus.Draft)
    {
        ArgumentNullException.ThrowIfNull(topics);

        var eligible = Eligible(topics, minimumStatus).ToList();
        return eligible.Count == 0 ? null : eligible[date.DayNumber % eligible.Count];
    }

    /// <summary>Capsules that give context for a civics question.</summary>
    public IReadOnlyList<DiscoveryTopic> ForQuestion(string questionId, IReadOnlyList<DiscoveryTopic> topics, ReviewStatus minimumStatus = ReviewStatus.Draft)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(questionId);
        ArgumentNullException.ThrowIfNull(topics);

        return Eligible(topics, minimumStatus)
            .Where(t => t.RelatedQuestionIds.Contains(questionId, StringComparer.OrdinalIgnoreCase))
            .ToList();
    }

    /// <summary>Other capsules that share a category or a place with <paramref name="topic"/>, closest first.</summary>
    public IReadOnlyList<DiscoveryTopic> Related(DiscoveryTopic topic, IReadOnlyList<DiscoveryTopic> topics, ReviewStatus minimumStatus = ReviewStatus.Draft)
    {
        ArgumentNullException.ThrowIfNull(topic);
        ArgumentNullException.ThrowIfNull(topics);

        return Eligible(topics, minimumStatus)
            .Where(t => !string.Equals(t.Id, topic.Id, StringComparison.Ordinal))
            .Select(t => (Topic: t, Score: Affinity(topic, t)))
            .Where(x => x.Score > 0)
            .OrderByDescending(x => x.Score)
            .ThenBy(x => x.Topic.Title, StringComparer.Ordinal)
            .Select(x => x.Topic)
            .ToList();
    }

    /// <summary>Capsules whose review state is at least <paramref name="minimumStatus"/> and not outdated.</summary>
    public static IEnumerable<DiscoveryTopic> Eligible(IEnumerable<DiscoveryTopic> topics, ReviewStatus minimumStatus)
    {
        ArgumentNullException.ThrowIfNull(topics);
        return topics.Where(t => t.ReviewStatus != ReviewStatus.Outdated && t.ReviewStatus >= minimumStatus);
    }

    private static int Affinity(DiscoveryTopic a, DiscoveryTopic b)
    {
        var score = 0;
        if (string.Equals(a.Category, b.Category, StringComparison.Ordinal))
        {
            score += 1;
        }

        score += a.RelatedPlaces.Intersect(b.RelatedPlaces, StringComparer.OrdinalIgnoreCase).Count() * 2;
        score += a.RelatedQuestionIds.Intersect(b.RelatedQuestionIds, StringComparer.OrdinalIgnoreCase).Count() * 2;
        return score;
    }
}
