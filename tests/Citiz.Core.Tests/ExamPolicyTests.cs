using Citiz.Core.Exams;

namespace Citiz.Core.Tests;

public sealed class ExamPolicyTests
{
    [Theory]
    [InlineData("2025-10-19", "2008")]
    [InlineData("2025-10-20", "2025")]
    [InlineData("2020-01-01", "2008")]
    [InlineData("2026-08-25", "2025")]
    public void Resolves_version_by_filing_date(string filingDate, string expected)
    {
        var version = ExamPolicy.Resolve(DateOnly.Parse(filingDate, System.Globalization.CultureInfo.InvariantCulture), TestData.Versions);

        Assert.NotNull(version);
        Assert.Equal(expected, version.Id);
    }

    [Fact]
    public void Returns_null_when_no_version_covers_the_date()
    {
        var onlyNew = new[] { TestData.V2025 };

        Assert.Null(ExamPolicy.Resolve(new DateOnly(2024, 1, 1), onlyNew));
    }

    [Fact]
    public void Throws_when_versions_overlap_because_that_is_a_content_error()
    {
        var overlapping = new[] { TestData.V2025, TestData.V2025 with { Id = "2030" } };

        Assert.Throws<InvalidOperationException>(() => ExamPolicy.Resolve(new DateOnly(2026, 1, 1), overlapping));
    }

    [Theory]
    [InlineData(6, 0, ExamOutcome.Passed)]
    [InlineData(6, 4, ExamOutcome.Passed)]
    [InlineData(5, 5, ExamOutcome.Failed)]
    [InlineData(0, 5, ExamOutcome.Failed)]
    [InlineData(5, 4, ExamOutcome.InProgress)]
    [InlineData(0, 0, ExamOutcome.InProgress)]
    public void Applies_2008_thresholds(int correct, int incorrect, ExamOutcome expected) =>
        Assert.Equal(expected, ExamPolicy.Evaluate(TestData.V2008.Standard, correct, incorrect));

    [Theory]
    [InlineData(12, 0, ExamOutcome.Passed)]
    [InlineData(12, 8, ExamOutcome.Passed)]
    [InlineData(11, 9, ExamOutcome.Failed)]
    [InlineData(11, 8, ExamOutcome.InProgress)]
    public void Applies_2025_thresholds(int correct, int incorrect, ExamOutcome expected) =>
        Assert.Equal(expected, ExamPolicy.Evaluate(TestData.V2025.Standard, correct, incorrect));

    [Fact]
    public void Rules_must_always_end_with_a_decision()
    {
        // 20 asked, 12 to pass: the fail threshold has to be 9, or a 11/8 sitting would run out of questions undecided.
        Assert.Throws<ArgumentException>(() => new ExamAdministrationRules(20, 12, 8));
        Assert.Throws<ArgumentException>(() => new ExamAdministrationRules(20, 12, 10));
        Assert.Throws<ArgumentOutOfRangeException>(() => new ExamAdministrationRules(0, 1, 1));
    }

    [Fact]
    public void Version_applies_inclusively_at_both_ends()
    {
        var bounded = TestData.V2008 with { FilingFrom = new DateOnly(2008, 10, 1) };

        Assert.True(bounded.AppliesTo(new DateOnly(2008, 10, 1)));
        Assert.True(bounded.AppliesTo(new DateOnly(2025, 10, 19)));
        Assert.False(bounded.AppliesTo(new DateOnly(2008, 9, 30)));
        Assert.False(bounded.AppliesTo(new DateOnly(2025, 10, 20)));
    }
}
