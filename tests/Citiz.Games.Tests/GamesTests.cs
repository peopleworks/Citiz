using Citiz.Core.Content;
using Citiz.Core.Exams;

namespace Citiz.Games.Tests;

public sealed class GamesTests
{
    private static readonly SourceReference Source = new("USCIS", "T", new Uri("https://www.uscis.gov/"), null, "Public domain");

    private static QuestionBank Bank(int size) => new(
        "2025",
        Enumerable.Range(1, size).Select(n => new CivicsQuestion($"2025-{n:000}", "2025", n, "Gov", n % 2 == 0 ? "Even" : "Odd", $"Q{n}?", [$"A{n}"])).ToList(),
        ReviewStatus.Approved,
        [Source]);

    [Theory]
    [InlineData(0.0, 1)]
    [InlineData(0.34, 1)]
    [InlineData(0.35, 2)]
    [InlineData(0.64, 2)]
    [InlineData(0.65, 3)]
    [InlineData(0.85, 4)]
    [InlineData(1.0, 4)]
    [InlineData(double.NaN, 1)]
    public void Difficulty_follows_mastery(double mastery, int level) => Assert.Equal(level, DifficultyAdapter.LevelFor(mastery));

    [Fact]
    public void Multiple_choice_has_one_official_answer_and_real_distractors()
    {
        var bank = Bank(12);
        var question = bank.FindByNumber(4)!;

        var item = MultipleChoiceBuilder.Build(question, bank, null, new Random(5));

        Assert.NotNull(item);
        Assert.Equal(4, item.Options.Count);
        Assert.Equal(4, item.Options.Distinct().Count());
        Assert.Equal("A4", item.CorrectOption);
        Assert.True(item.IsCorrect(item.CorrectIndex));
        Assert.All(item.Options.Where((_, i) => i != item.CorrectIndex), o => Assert.Contains(bank.Questions, q => q.AcceptedAnswers.Contains(o)));
    }

    [Fact]
    public void Multiple_choice_prefers_distractors_from_the_same_subcategory()
    {
        var bank = Bank(12);
        var question = bank.FindByNumber(4)!; // "Even"

        var item = MultipleChoiceBuilder.Build(question, bank, null, new Random(1))!;

        var distractors = item.Options.Where((_, i) => i != item.CorrectIndex);
        Assert.All(distractors, o => Assert.Equal("Even", bank.Questions.Single(q => q.AcceptedAnswers.Contains(o)).Subcategory));
    }

    [Fact]
    public void Same_seed_builds_the_same_item()
    {
        var bank = Bank(12);

        var a = MultipleChoiceBuilder.Build(bank.FindByNumber(1)!, bank, null, new Random(9))!;
        var b = MultipleChoiceBuilder.Build(bank.FindByNumber(1)!, bank, null, new Random(9))!;

        Assert.Equal(a.Options, b.Options);
        Assert.Equal(a.CorrectIndex, b.CorrectIndex);
    }

    [Fact]
    public void Unresolved_dynamic_questions_are_skipped()
    {
        var bank = Bank(6);
        var dynamic = new CivicsQuestion("2025-099", "2025", 99, "Gov", "Odd", "Who?", [], DynamicAnswerKey: "president");

        Assert.Null(MultipleChoiceBuilder.Build(dynamic, bank, null, new Random(1)));
    }

    [Fact]
    public void Challenge_scores_a_round_and_stops_at_the_end()
    {
        var bank = Bank(12);

        var challenge = CivicsChallenge.Start(bank, null, new Random(2), length: 5);

        Assert.Equal(5, challenge.Items.Count);
        while (challenge.Current is { } item)
        {
            challenge.Answer(item.CorrectIndex);
        }

        Assert.True(challenge.IsComplete);
        Assert.Equal(5, challenge.Score);
        Assert.Throws<InvalidOperationException>(() => challenge.Answer(0));
    }

    [Fact]
    public void Challenge_can_focus_on_chosen_questions()
    {
        var bank = Bank(12);
        var focus = new HashSet<string> { "2025-001", "2025-002" };

        var challenge = CivicsChallenge.Start(bank, null, new Random(2), length: 10, focus: focus);

        Assert.Equal(2, challenge.Items.Count);
        Assert.All(challenge.Items, i => Assert.Contains(i.Question.Id, focus));
    }

    [Fact]
    public void Catalog_lists_the_playable_game_first_and_is_honest_about_the_rest()
    {
        Assert.Equal(GameStatus.Playable, GameCatalog.All[0].Status);
        Assert.Equal("civics-challenge", GameCatalog.All[0].Id);
        Assert.Contains(GameCatalog.All, g => g.Status == GameStatus.InDesign);
        Assert.Null(GameCatalog.Find("nope"));
    }
}
