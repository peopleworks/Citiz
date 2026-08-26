using Citiz.Core.Exams;

namespace Citiz.Games;

/// <summary>One answered item in a <see cref="CivicsChallenge"/>.</summary>
/// <param name="Item">The item presented.</param>
/// <param name="ChosenIndex">The option the learner chose.</param>
public sealed record ChallengeAnswer(MultipleChoiceItem Item, int ChosenIndex)
{
    /// <summary>Whether the learner chose the official answer.</summary>
    public bool Correct => Item.IsCorrect(ChosenIndex);
}

/// <summary>
/// "Reto cívico": a short round of multiple-choice questions from the applicable bank. A practice
/// result, never an official one; it feeds the learning ledger and stays out of the exam simulation.
/// </summary>
public sealed class CivicsChallenge
{
    private readonly List<ChallengeAnswer> _answers = [];

    private CivicsChallenge(IReadOnlyList<MultipleChoiceItem> items)
    {
        Items = items;
    }

    /// <summary>Default number of items in a round.</summary>
    public const int DefaultLength = 10;

    /// <summary>The items in this round.</summary>
    public IReadOnlyList<MultipleChoiceItem> Items { get; }

    /// <summary>Answers so far.</summary>
    public IReadOnlyList<ChallengeAnswer> Answers => _answers;

    /// <summary>Zero-based index of the next item.</summary>
    public int Position => _answers.Count;

    /// <summary>The item to present now, or <c>null</c> when the round is over.</summary>
    public MultipleChoiceItem? Current => IsComplete ? null : Items[Position];

    /// <summary>Whether every item has been answered.</summary>
    public bool IsComplete => Position >= Items.Count;

    /// <summary>Correct answers so far.</summary>
    public int Score => _answers.Count(a => a.Correct);

    /// <summary>
    /// Starts a round of up to <paramref name="length"/> items drawn from <paramref name="bank"/>,
    /// skipping questions that cannot be presented as multiple choice. Optionally restricted to the
    /// question ids in <paramref name="focus"/> (for example, the learner's weakest items).
    /// </summary>
    public static CivicsChallenge Start(
        QuestionBank bank,
        IReadOnlyDictionary<string, DynamicAnswer>? dynamicAnswers,
        Random random,
        int length = DefaultLength,
        IReadOnlySet<string>? focus = null)
    {
        ArgumentNullException.ThrowIfNull(bank);
        ArgumentNullException.ThrowIfNull(random);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(length);

        var pool = bank.Questions.Where(q => focus is null || focus.Contains(q.Id)).OrderBy(_ => random.Next());
        var items = new List<MultipleChoiceItem>();
        foreach (var question in pool)
        {
            var item = MultipleChoiceBuilder.Build(question, bank, dynamicAnswers, random);
            if (item is not null)
            {
                items.Add(item);
            }

            if (items.Count == length)
            {
                break;
            }
        }

        return new CivicsChallenge(items);
    }

    /// <summary>Answers the current item with the option at <paramref name="chosenIndex"/>.</summary>
    /// <exception cref="InvalidOperationException">The round is over.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The index is not an option.</exception>
    public ChallengeAnswer Answer(int chosenIndex)
    {
        var item = Current ?? throw new InvalidOperationException("The challenge is complete.");
        ArgumentOutOfRangeException.ThrowIfNegative(chosenIndex);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(chosenIndex, item.Options.Count);

        var answer = new ChallengeAnswer(item, chosenIndex);
        _answers.Add(answer);
        return answer;
    }
}
