using Citiz.Core.Exams;

namespace Citiz.Games;

/// <summary>A question presented with options, exactly one of which is an official accepted answer.</summary>
/// <param name="Question">The official question.</param>
/// <param name="Options">The options, shuffled.</param>
/// <param name="CorrectIndex">Index of the official answer in <see cref="Options"/>.</param>
public sealed record MultipleChoiceItem(CivicsQuestion Question, IReadOnlyList<string> Options, int CorrectIndex)
{
    /// <summary>The official answer among the options.</summary>
    public string CorrectOption => Options[CorrectIndex];

    /// <summary>Whether the option at <paramref name="index"/> is the official answer.</summary>
    public bool IsCorrect(int index) => index == CorrectIndex;
}

/// <summary>
/// Builds multiple-choice items for beginners. Distractors are other official answers from the same
/// bank, preferring the same subcategory, so every option a learner sees is a real answer to a real
/// question and nothing is invented.
/// </summary>
public static class MultipleChoiceBuilder
{
    /// <summary>Default number of options.</summary>
    public const int DefaultOptionCount = 4;

    /// <summary>
    /// Builds an item for <paramref name="question"/>, or <c>null</c> when it has no resolvable
    /// answer or the bank cannot supply enough distractors.
    /// </summary>
    /// <param name="question">The question to present.</param>
    /// <param name="bank">The bank to draw distractors from.</param>
    /// <param name="dynamicAnswers">Current dynamic answers, to resolve officeholder questions.</param>
    /// <param name="random">Randomness; seed it for a reproducible item.</param>
    /// <param name="optionCount">Total options including the correct one.</param>
    public static MultipleChoiceItem? Build(
        CivicsQuestion question,
        QuestionBank bank,
        IReadOnlyDictionary<string, DynamicAnswer>? dynamicAnswers,
        Random random,
        int optionCount = DefaultOptionCount)
    {
        ArgumentNullException.ThrowIfNull(question);
        ArgumentNullException.ThrowIfNull(bank);
        ArgumentNullException.ThrowIfNull(random);
        ArgumentOutOfRangeException.ThrowIfLessThan(optionCount, 2);

        var answers = question.ResolveAnswers(dynamicAnswers);
        if (answers.Count == 0)
        {
            return null;
        }

        var correct = answers[random.Next(answers.Count)];
        var ownAnswers = answers.Select(AnswerMatcher.Normalize).ToHashSet(StringComparer.Ordinal);

        var candidates = bank.Questions
            .Where(q => !string.Equals(q.Id, question.Id, StringComparison.Ordinal))
            .OrderBy(q => string.Equals(q.Subcategory, question.Subcategory, StringComparison.Ordinal) ? 0 : 1)
            .ThenBy(_ => random.Next())
            .SelectMany(q => q.ResolveAnswers(dynamicAnswers))
            .Where(a => !ownAnswers.Contains(AnswerMatcher.Normalize(a)))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Take(optionCount - 1)
            .ToList();

        if (candidates.Count < optionCount - 1)
        {
            return null;
        }

        var options = candidates.Append(correct).OrderBy(_ => random.Next()).ToList();
        return new MultipleChoiceItem(question, options, options.IndexOf(correct));
    }
}
