using Citiz.Core.Content;
using Citiz.Core.Exams;

namespace Citiz.Core.Tests;

public sealed class QuestionBankTests
{
    [Fact]
    public void Rejects_duplicate_ids_and_numbers()
    {
        var question = new CivicsQuestion("2025-001", "2025", 1, "c", "s", "p?", ["a"]);

        Assert.Throws<ArgumentException>(() => new QuestionBank("2025", [question, question], ReviewStatus.Approved, [TestData.Source]));
        Assert.Throws<ArgumentException>(() => new QuestionBank("2025", [question, question with { Id = "2025-002" }], ReviewStatus.Approved, [TestData.Source]));
    }

    [Fact]
    public void Finds_by_id_number_and_category()
    {
        var bank = TestData.Bank(TestData.V2025, 10);

        Assert.Equal(3, bank.Find("2025-003")!.Number);
        Assert.Equal("2025-007", bank.FindByNumber(7)!.Id);
        Assert.Null(bank.Find("2025-999"));
        Assert.Equal(5, bank.InCategory("American Government").Count);
        Assert.Equal(5, bank.InCategory("1800s").Count);
        Assert.Equal(["American Government", "American History"], bank.Categories);
    }

    [Fact]
    public void Senior_questions_follow_the_version_designation()
    {
        var bank = TestData.Bank(TestData.V2008, 100);

        Assert.Equal(20, bank.SeniorQuestions(TestData.V2008).Count);
        Assert.Empty(bank.SeniorQuestions(TestData.V2025 with { Id = "2008" }));
    }

    [Fact]
    public void Dynamic_questions_resolve_through_the_dynamic_answers()
    {
        var question = new CivicsQuestion("2025-038", "2025", 38, "c", "s", "Who is the President now?", [], DynamicAnswerKey: "president");
        var resolved = new Dictionary<string, DynamicAnswer>
        {
            ["president"] = new("president", "President", DynamicAnswerScope.Federal, "Jane Doe", ["Jane Doe", "Doe"], null, null, null, ReviewStatus.Approved, [TestData.Source]),
        };
        var unresolved = new Dictionary<string, DynamicAnswer>
        {
            ["president"] = new("president", "President", DynamicAnswerScope.Federal, null, [], null, null, null, ReviewStatus.NeedsReview, [TestData.Source]),
        };

        Assert.True(question.IsDynamic);
        Assert.Equal(["Jane Doe", "Doe"], question.ResolveAnswers(resolved));
        Assert.Empty(question.ResolveAnswers(unresolved));
        Assert.Empty(question.ResolveAnswers(null));
    }

    [Theory]
    [InlineData("draft", ReviewStatus.Draft)]
    [InlineData("needs-review", ReviewStatus.NeedsReview)]
    [InlineData("Approved", ReviewStatus.Approved)]
    [InlineData(" outdated ", ReviewStatus.Outdated)]
    public void Review_status_round_trips_through_kebab_case(string text, ReviewStatus expected)
    {
        Assert.Equal(expected, ReviewStatuses.Parse(text));
        Assert.Equal(text.Trim().ToLowerInvariant(), expected.ToKebabCase());
    }

    [Fact]
    public void Unknown_review_status_is_rejected()
    {
        Assert.Throws<FormatException>(() => ReviewStatuses.Parse("published"));
        Assert.False(ReviewStatuses.TryParse(null, out _));
    }
}
