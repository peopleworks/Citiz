using Citiz.Core.Exams;

namespace Citiz.Core.Tests;

public sealed class AnswerMatcherTests
{
    [Theory]
    [InlineData("the Constitution", "(U.S.) Constitution", AnswerMatchKind.Exact)]
    [InlineData("U.S. Constitution", "(U.S.) Constitution", AnswerMatchKind.Exact)]
    [InlineData("us constitution", "(U.S.) Constitution", AnswerMatchKind.Exact)]
    [InlineData("United States Constitution", "(U.S.) Constitution", AnswerMatchKind.Exact)]
    [InlineData("CONSTITUTION!", "(U.S.) Constitution", AnswerMatchKind.Exact)]
    [InlineData("27", "Twenty-seven (27)", AnswerMatchKind.Exact)]
    [InlineData("twenty seven", "Twenty-seven (27)", AnswerMatchKind.Exact)]
    [InlineData("Senate and House", "Senate and House (of Representatives)", AnswerMatchKind.Exact)]
    [InlineData("the Senate and the House of Representatives", "Senate and House (of Representatives)", AnswerMatchKind.Exact)]
    [InlineData("Senate, House of Representatives, and more", "Senate and House (of Representatives)", AnswerMatchKind.Contains)]
    [InlineData("I think it is the constitution", "(U.S.) Constitution", AnswerMatchKind.Contains)]
    [InlineData("constitutoin", "(U.S.) Constitution", AnswerMatchKind.Close)]
    [InlineData("the Bill of Rights", "(U.S.) Constitution", AnswerMatchKind.None)]
    [InlineData("", "(U.S.) Constitution", AnswerMatchKind.None)]
    [InlineData("   ", "(U.S.) Constitution", AnswerMatchKind.None)]
    public void Classifies_responses(string response, string accepted, AnswerMatchKind expected)
    {
        var match = AnswerMatcher.Evaluate(response, [accepted]);

        Assert.Equal(expected, match.Kind);
        if (expected != AnswerMatchKind.None)
        {
            Assert.Equal(accepted, match.MatchedAnswer);
        }
    }

    [Fact]
    public void Picks_the_best_match_across_several_accepted_answers()
    {
        string[] accepted = ["freedom of speech", "freedom of religion", "the right to bear arms"];

        var match = AnswerMatcher.Evaluate("freedom of religion", accepted);

        Assert.Equal(AnswerMatchKind.Exact, match.Kind);
        Assert.Equal("freedom of religion", match.MatchedAnswer);
    }

    [Fact]
    public void Accepted_matches_are_exact_or_contains_only()
    {
        Assert.True(AnswerMatcher.Evaluate("nine", ["nine (9)"]).IsAccepted);
        Assert.True(AnswerMatcher.Evaluate("there are nine justices", ["nine (9)"]).IsAccepted);
        Assert.False(AnswerMatcher.Evaluate("nien", ["nine (9)"]).IsAccepted);
        Assert.False(AnswerMatcher.Evaluate("ten", ["nine (9)"]).IsAccepted);
    }

    [Fact]
    public void Parenthesised_parts_are_optional_and_numbers_stand_alone()
    {
        Assert.Equal(["Twenty-seven (27)", "Twenty-seven 27", "Twenty-seven", "27"], AnswerMatcher.Variants("Twenty-seven (27)"));
        Assert.Equal(["(U.S.) Constitution", "U.S. Constitution", "Constitution"], AnswerMatcher.Variants("(U.S.) Constitution"));
        Assert.Equal(["Republic"], AnswerMatcher.Variants("Republic"));
    }

    [Fact]
    public void Normalization_is_what_the_interface_can_explain()
    {
        Assert.Equal("the us constitution", AnswerMatcher.Normalize("The U.S. Constitution!"));
        Assert.Equal("twenty seven", AnswerMatcher.Normalize("Twenty-seven"));
        Assert.Equal("we the people", AnswerMatcher.Normalize("  We  the   People  "));
        Assert.Equal(string.Empty, AnswerMatcher.Normalize(null));
    }
}
