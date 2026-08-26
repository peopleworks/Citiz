namespace Citiz.Core.Exams;

/// <summary>
/// How one sitting of the civics test is administered: how many questions the officer asks and when
/// the test stops. USCIS stops as soon as the outcome is decided, so a well-formed rule set satisfies
/// <c>PassingAnswers + FailingAnswers == QuestionsAsked + 1</c>; the constructor enforces it.
/// </summary>
public sealed record ExamAdministrationRules
{
    /// <summary>Creates a rule set, validating the stop-rule invariant.</summary>
    /// <param name="questionsAsked">Maximum number of questions asked in one sitting.</param>
    /// <param name="passingAnswers">Correct answers needed to pass; the test stops here.</param>
    /// <param name="failingAnswers">Incorrect answers that end the test as a fail.</param>
    /// <exception cref="ArgumentOutOfRangeException">A value is not positive.</exception>
    /// <exception cref="ArgumentException">The values cannot describe a test that always stops with a decision.</exception>
    public ExamAdministrationRules(int questionsAsked, int passingAnswers, int failingAnswers)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(questionsAsked);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(passingAnswers);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(failingAnswers);

        if (passingAnswers + failingAnswers != questionsAsked + 1)
        {
            throw new ArgumentException(
                $"Passing ({passingAnswers}) + failing ({failingAnswers}) answers must equal questions asked ({questionsAsked}) + 1, " +
                "so that every sitting ends with a decision.");
        }

        QuestionsAsked = questionsAsked;
        PassingAnswers = passingAnswers;
        FailingAnswers = failingAnswers;
    }

    /// <summary>Maximum number of questions asked in one sitting.</summary>
    public int QuestionsAsked { get; }

    /// <summary>Correct answers needed to pass. The officer stops asking once this is reached.</summary>
    public int PassingAnswers { get; }

    /// <summary>Incorrect answers that end the sitting as a fail.</summary>
    public int FailingAnswers { get; }
}
