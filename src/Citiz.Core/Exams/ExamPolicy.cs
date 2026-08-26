namespace Citiz.Core.Exams;

/// <summary>Result of a civics-test sitting so far.</summary>
public enum ExamOutcome
{
    /// <summary>Neither threshold reached yet.</summary>
    InProgress,

    /// <summary>The passing number of correct answers was reached.</summary>
    Passed,

    /// <summary>The failing number of incorrect answers was reached.</summary>
    Failed,
}

/// <summary>The two rules USCIS applies that Citiz must never get wrong: which version, and whether you passed.</summary>
public static class ExamPolicy
{
    /// <summary>
    /// The version an applicant takes given the date their Form N-400 was filed, or <c>null</c> when
    /// no version in <paramref name="versions"/> covers that date. Citiz does not guess: a null here
    /// becomes a question to the learner, not a default.
    /// </summary>
    /// <exception cref="InvalidOperationException">More than one version claims the date, which is a content error.</exception>
    public static ExamVersion? Resolve(DateOnly filingDate, IEnumerable<ExamVersion> versions)
    {
        ArgumentNullException.ThrowIfNull(versions);

        var applicable = versions.Where(v => v.AppliesTo(filingDate)).ToList();
        return applicable.Count switch
        {
            0 => null,
            1 => applicable[0],
            _ => throw new InvalidOperationException(
                $"Exam versions {string.Join(", ", applicable.Select(v => v.Id))} all apply to filing date {filingDate:yyyy-MM-dd}. Fix content/exams/versions.json."),
        };
    }

    /// <summary>Outcome after <paramref name="correct"/> correct and <paramref name="incorrect"/> incorrect answers under <paramref name="rules"/>.</summary>
    public static ExamOutcome Evaluate(ExamAdministrationRules rules, int correct, int incorrect)
    {
        ArgumentNullException.ThrowIfNull(rules);
        ArgumentOutOfRangeException.ThrowIfNegative(correct);
        ArgumentOutOfRangeException.ThrowIfNegative(incorrect);

        if (correct >= rules.PassingAnswers)
        {
            return ExamOutcome.Passed;
        }

        if (incorrect >= rules.FailingAnswers)
        {
            return ExamOutcome.Failed;
        }

        return ExamOutcome.InProgress;
    }
}
