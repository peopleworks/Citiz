namespace Citiz.Learning;

/// <summary>
/// What a learner has done with one item: a civics question, a vocabulary word or a capsule. Items
/// are identified by their content id, so progress survives content updates that keep ids stable.
/// </summary>
/// <param name="ItemId">Content id, e.g. <c>2025-002</c>.</param>
/// <param name="Attempts">Times the item was practised.</param>
/// <param name="Correct">Times the answer was accepted.</param>
/// <param name="Streak">Consecutive correct answers; reset to zero by a miss.</param>
/// <param name="LastPracticedAt">When the item was last practised.</param>
/// <param name="NextReviewAt">When the scheduler wants to see it again.</param>
public sealed record ItemProgress(
    string ItemId,
    int Attempts,
    int Correct,
    int Streak,
    DateTimeOffset LastPracticedAt,
    DateTimeOffset NextReviewAt)
{
    /// <summary>Consecutive correct answers needed before an item counts as mastered.</summary>
    public const int MasteryStreak = 3;

    /// <summary>Share of attempts that were correct, 0 to 1.</summary>
    public double Accuracy => Attempts == 0 ? 0 : (double)Correct / Attempts;

    /// <summary>Whether the item has been answered correctly <see cref="MasteryStreak"/> times in a row.</summary>
    public bool IsMastered => Streak >= MasteryStreak;

    /// <summary>Whether the item is due for review at <paramref name="now"/>.</summary>
    public bool IsDue(DateTimeOffset now) => NextReviewAt <= now;
}
