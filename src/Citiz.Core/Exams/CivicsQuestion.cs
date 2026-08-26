using Citiz.Core.Content;

namespace Citiz.Core.Exams;

/// <summary>
/// One official civics question with its accepted answers, exactly as USCIS publishes them. This is
/// the content the guiding principle protects: the interface, the games and any AI feature may
/// explain it, translate it or quiz on it, but only a content maintainer edits it.
/// </summary>
/// <param name="Id">Stable identifier: version id, a dash, and the official number zero-padded to three digits (<c>2025-001</c>).</param>
/// <param name="VersionId">The <see cref="ExamVersion.Id"/> this question belongs to.</param>
/// <param name="Number">Official question number within the version.</param>
/// <param name="Category">Top-level official section, e.g. <c>American Government</c>.</param>
/// <param name="Subcategory">Official subsection, e.g. <c>Principles of American Government</c>.</param>
/// <param name="Prompt">The question in its official English wording.</param>
/// <param name="AcceptedAnswers">Official accepted answers. Empty for questions whose answer varies by officeholder or state; see <see cref="DynamicAnswerKey"/>.</param>
/// <param name="DynamicAnswerKey">Key into the dynamic answers file when the answer depends on who holds an office now; <c>null</c> for stable answers.</param>
/// <param name="Note">Editorial note shown with the answers, e.g. a USCIS instruction to check for updates.</param>
/// <param name="ReviewStatus">Editorial state of this question.</param>
public sealed record CivicsQuestion(
    string Id,
    string VersionId,
    int Number,
    string Category,
    string Subcategory,
    string Prompt,
    IReadOnlyList<string> AcceptedAnswers,
    string? DynamicAnswerKey = null,
    string? Note = null,
    ReviewStatus ReviewStatus = ReviewStatus.NeedsReview)
{
    /// <summary>Whether the answer depends on a current officeholder or the learner's state.</summary>
    public bool IsDynamic => DynamicAnswerKey is not null;

    /// <summary>
    /// The answers to accept right now: the official list, or, for a dynamic question, the current
    /// officeholder when <paramref name="dynamicAnswers"/> resolves it. Returns an empty list when
    /// the answer varies and cannot be resolved, so callers must say "answers vary" rather than guess.
    /// </summary>
    public IReadOnlyList<string> ResolveAnswers(IReadOnlyDictionary<string, DynamicAnswer>? dynamicAnswers)
    {
        if (DynamicAnswerKey is null)
        {
            return AcceptedAnswers;
        }

        if (dynamicAnswers is not null &&
            dynamicAnswers.TryGetValue(DynamicAnswerKey, out var dynamic) &&
            dynamic.IsResolved)
        {
            return dynamic.AcceptedAnswers;
        }

        return AcceptedAnswers;
    }
}
