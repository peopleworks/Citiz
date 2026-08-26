using Citiz.Core.Content;

namespace Citiz.Core.Exams;

/// <summary>Who an answer depends on: a federal officeholder, or something that varies per state or congressional district.</summary>
public enum DynamicAnswerScope
{
    /// <summary>One answer for everyone, e.g. the President.</summary>
    Federal,

    /// <summary>Depends on the learner's state, e.g. the governor or a senator.</summary>
    State,

    /// <summary>Depends on the learner's congressional district, e.g. their representative.</summary>
    District,
}

/// <summary>
/// The current answer to a question whose official answer changes with elections and appointments.
/// Kept apart from the question bank so it can be re-verified on its own cadence, and so the bank
/// itself never has to change when an office changes hands.
/// </summary>
/// <param name="Key">Identifier referenced by <see cref="CivicsQuestion.DynamicAnswerKey"/>, e.g. <c>president</c>.</param>
/// <param name="Office">The office, as the learner would name it.</param>
/// <param name="Scope">Whether the answer is federal, per state or per district.</param>
/// <param name="Holder">Current officeholder for federal scope; <c>null</c> when the answer varies by learner.</param>
/// <param name="AcceptedAnswers">Forms of the name that would be accepted, most complete first.</param>
/// <param name="Since">When the current holder took office.</param>
/// <param name="VerifiedOn">When a maintainer last confirmed the holder against the source.</param>
/// <param name="LookupHint">For state and district scope: where the learner can find their own answer.</param>
/// <param name="ReviewStatus">Editorial state of this entry.</param>
/// <param name="Sources">Official sources for the current holder.</param>
public sealed record DynamicAnswer(
    string Key,
    string Office,
    DynamicAnswerScope Scope,
    string? Holder,
    IReadOnlyList<string> AcceptedAnswers,
    DateOnly? Since,
    DateOnly? VerifiedOn,
    string? LookupHint,
    ReviewStatus ReviewStatus,
    IReadOnlyList<SourceReference> Sources)
{
    /// <summary>Whether Citiz can state the answer itself rather than pointing the learner to a lookup.</summary>
    public bool IsResolved => Holder is not null && AcceptedAnswers.Count > 0;
}
