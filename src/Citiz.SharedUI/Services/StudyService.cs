using Citiz.Content;
using Citiz.Core.Exams;

namespace Citiz.SharedUI.Services;

/// <summary>The exam the learner is preparing for, fully loaded.</summary>
/// <param name="Version">The version.</param>
/// <param name="Bank">Its question bank.</param>
/// <param name="DynamicAnswers">Current dynamic answers.</param>
/// <param name="Versions">Every version, for the picker.</param>
/// <param name="ResolvedFromFilingDate">Whether the version came from the learner's filing date rather than an explicit choice or the default.</param>
public sealed record StudyContext(
    ExamVersion Version,
    QuestionBank Bank,
    IReadOnlyDictionary<string, DynamicAnswer> DynamicAnswers,
    IReadOnlyList<ExamVersion> Versions,
    bool ResolvedFromFilingDate)
{
    /// <summary>The answers to accept for a question right now.</summary>
    public IReadOnlyList<string> AnswersFor(CivicsQuestion question) => question.ResolveAnswers(DynamicAnswers);

    /// <summary>The dynamic entry behind a question, if any.</summary>
    public DynamicAnswer? DynamicFor(CivicsQuestion question) =>
        question.DynamicAnswerKey is { } key && DynamicAnswers.TryGetValue(key, out var answer) ? answer : null;
}

/// <summary>Resolves which exam the learner studies: their explicit choice, else their filing date, else the current version.</summary>
public sealed class StudyService(ContentRepository content, LearnerState learner)
{
    /// <summary>Loads the study context for the learner's settings.</summary>
    public async Task<StudyContext> GetContextAsync(CancellationToken cancellationToken = default)
    {
        var versions = await content.GetExamVersionsAsync(cancellationToken);
        var settings = learner.Exam;

        ExamVersion? version = null;
        var fromFilingDate = false;

        if (settings.VersionId is { } chosen)
        {
            version = versions.FirstOrDefault(v => string.Equals(v.Id, chosen, StringComparison.OrdinalIgnoreCase));
        }

        if (version is null && settings.FilingDate is { } filingDate)
        {
            version = ExamPolicy.Resolve(filingDate, versions);
            fromFilingDate = version is not null;
        }

        version ??= versions.FirstOrDefault(v => v.IsCurrent) ?? versions[0];

        var bank = await content.GetQuestionBankAsync(version.Id, cancellationToken);
        var dynamicAnswers = await content.GetDynamicAnswersAsync(cancellationToken);
        return new StudyContext(version, bank, dynamicAnswers, versions, fromFilingDate);
    }
}
