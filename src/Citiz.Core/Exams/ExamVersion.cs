using Citiz.Core.Content;

namespace Citiz.Core.Exams;

/// <summary>
/// One version of the USCIS civics test. Which version an applicant takes depends on the date their
/// Form N-400 was filed, so the version is data (content/exams/versions.json), never a condition in
/// code. A future version is a new entry in that file.
/// </summary>
/// <param name="Id">Stable identifier, e.g. <c>2008</c> or <c>2025</c>. Question ids are prefixed with it.</param>
/// <param name="DisplayName">Name as USCIS refers to it, e.g. <c>2025 Civics Test</c>.</param>
/// <param name="FilingFrom">First N-400 filing date this version applies to (inclusive); <c>null</c> for no lower bound.</param>
/// <param name="FilingTo">Last N-400 filing date this version applies to (inclusive); <c>null</c> while the version is current.</param>
/// <param name="BankSize">Number of questions in the official bank.</param>
/// <param name="Standard">Rules for a regular sitting.</param>
/// <param name="SeniorConsideration">Rules for applicants who qualify for the 65/20 special consideration.</param>
/// <param name="SeniorQuestionNumbers">Official question numbers designated for the 65/20 consideration (marked with an asterisk by USCIS). Empty while not yet verified.</param>
/// <param name="ReviewStatus">Editorial state of this rule set.</param>
/// <param name="Sources">Official documents these rules were taken from.</param>
public sealed record ExamVersion(
    string Id,
    string DisplayName,
    DateOnly? FilingFrom,
    DateOnly? FilingTo,
    int BankSize,
    ExamAdministrationRules Standard,
    ExamAdministrationRules SeniorConsideration,
    IReadOnlyList<int> SeniorQuestionNumbers,
    ReviewStatus ReviewStatus,
    IReadOnlyList<SourceReference> Sources)
{
    /// <summary>Whether an applicant who filed Form N-400 on <paramref name="filingDate"/> takes this version.</summary>
    public bool AppliesTo(DateOnly filingDate) =>
        (FilingFrom is null || filingDate >= FilingFrom) &&
        (FilingTo is null || filingDate <= FilingTo);

    /// <summary>Whether the 65/20 question list has been recorded for this version.</summary>
    public bool HasSeniorDesignation => SeniorQuestionNumbers.Count > 0;

    /// <summary>Whether this is the version currently given to new applicants.</summary>
    public bool IsCurrent => FilingTo is null;
}
