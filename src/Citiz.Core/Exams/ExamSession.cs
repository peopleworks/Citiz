namespace Citiz.Core.Exams;

/// <summary>One answered question in an <see cref="ExamSession"/>.</summary>
/// <param name="Question">The question that was asked.</param>
/// <param name="Response">What the learner said or typed, if captured.</param>
/// <param name="Correct">Whether the response was accepted.</param>
public sealed record ExamAnswerRecord(CivicsQuestion Question, string? Response, bool Correct);

/// <summary>
/// A simulated sitting of the civics test, run exactly as an officer would: questions drawn from the
/// applicable bank, asked one at a time, stopping the moment the outcome is decided. Deterministic:
/// give it a seeded <see cref="Random"/> and it draws the same questions every time, which is what
/// the tests do.
/// </summary>
public sealed class ExamSession
{
    private readonly List<ExamAnswerRecord> _history = [];

    private ExamSession(ExamVersion version, ExamAdministrationRules rules, bool seniorConsideration, IReadOnlyList<CivicsQuestion> questions)
    {
        Version = version;
        Rules = rules;
        SeniorConsideration = seniorConsideration;
        Questions = questions;
    }

    /// <summary>The exam version being simulated.</summary>
    public ExamVersion Version { get; }

    /// <summary>The rules in force for this sitting (standard or 65/20).</summary>
    public ExamAdministrationRules Rules { get; }

    /// <summary>Whether the sitting uses the 65/20 special consideration.</summary>
    public bool SeniorConsideration { get; }

    /// <summary>The questions drawn for this sitting, in the order they will be asked.</summary>
    public IReadOnlyList<CivicsQuestion> Questions { get; }

    /// <summary>Answers so far, in order.</summary>
    public IReadOnlyList<ExamAnswerRecord> History => _history;

    /// <summary>Correct answers so far.</summary>
    public int Correct { get; private set; }

    /// <summary>Incorrect answers so far.</summary>
    public int Incorrect { get; private set; }

    /// <summary>Zero-based index of the next question to ask.</summary>
    public int Position => _history.Count;

    /// <summary>Outcome so far.</summary>
    public ExamOutcome Outcome => ExamPolicy.Evaluate(Rules, Correct, Incorrect);

    /// <summary>Whether the sitting has ended, because the outcome is decided or the questions ran out.</summary>
    public bool IsComplete => Outcome != ExamOutcome.InProgress || Position >= Questions.Count;

    /// <summary>The question to ask now, or <c>null</c> once the sitting is complete.</summary>
    public CivicsQuestion? CurrentQuestion => IsComplete ? null : Questions[Position];

    /// <summary>
    /// Starts a sitting, drawing <see cref="ExamAdministrationRules.QuestionsAsked"/> questions at
    /// random from <paramref name="bank"/> (or from the 65/20 subset when
    /// <paramref name="seniorConsideration"/> is set).
    /// </summary>
    /// <param name="version">The version to simulate.</param>
    /// <param name="bank">The question bank for that version.</param>
    /// <param name="seniorConsideration">Use the 65/20 rules and question subset.</param>
    /// <param name="random">Source of randomness; pass a seeded instance for a reproducible draw.</param>
    /// <exception cref="ArgumentException">The bank belongs to another version.</exception>
    /// <exception cref="InvalidOperationException">The 65/20 subset was requested but has not been recorded for this version, or the pool is smaller than the number of questions to ask.</exception>
    public static ExamSession Start(ExamVersion version, QuestionBank bank, bool seniorConsideration = false, Random? random = null)
    {
        ArgumentNullException.ThrowIfNull(version);
        ArgumentNullException.ThrowIfNull(bank);

        if (!string.Equals(bank.VersionId, version.Id, StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException($"Bank '{bank.VersionId}' does not belong to exam version '{version.Id}'.", nameof(bank));
        }

        var rules = seniorConsideration ? version.SeniorConsideration : version.Standard;
        IReadOnlyList<CivicsQuestion> pool = seniorConsideration ? bank.SeniorQuestions(version) : bank.Questions;

        if (seniorConsideration && pool.Count == 0)
        {
            throw new InvalidOperationException(
                $"The 65/20 question list for exam version '{version.Id}' has not been recorded yet, so a 65/20 sitting cannot be simulated.");
        }

        if (pool.Count < rules.QuestionsAsked)
        {
            throw new InvalidOperationException(
                $"Exam version '{version.Id}' asks {rules.QuestionsAsked} questions but only {pool.Count} are available.");
        }

        var drawn = Shuffle(pool, random ?? Random.Shared).Take(rules.QuestionsAsked).ToList();
        return new ExamSession(version, rules, seniorConsideration, drawn);
    }

    /// <summary>Records the answer to <see cref="CurrentQuestion"/> and advances.</summary>
    /// <exception cref="InvalidOperationException">The sitting is already complete.</exception>
    public void Record(bool correct, string? response = null)
    {
        var question = CurrentQuestion ?? throw new InvalidOperationException("The exam session is complete; no question is pending.");

        _history.Add(new ExamAnswerRecord(question, response, correct));
        if (correct)
        {
            Correct++;
        }
        else
        {
            Incorrect++;
        }
    }

    private static List<CivicsQuestion> Shuffle(IReadOnlyList<CivicsQuestion> source, Random random)
    {
        var items = source.ToList();
        for (var i = items.Count - 1; i > 0; i--)
        {
            var j = random.Next(i + 1);
            (items[i], items[j]) = (items[j], items[i]);
        }

        return items;
    }
}
