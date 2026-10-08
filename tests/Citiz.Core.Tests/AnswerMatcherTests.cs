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
    [InlineData("twenty-seven", "Twenty-seven (27)", AnswerMatchKind.Exact)]
    [InlineData("Senate and House", "Senate and House (of Representatives)", AnswerMatchKind.Exact)]
    [InlineData("the Senate and the House of Representatives", "Senate and House (of Representatives)", AnswerMatchKind.Exact)]
    [InlineData("I think it is the constitution", "(U.S.) Constitution", AnswerMatchKind.Contains)]
    [InlineData("my answer is the Constitution", "(U.S.) Constitution", AnswerMatchKind.Contains)]
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

    // The defect class the 2026-10-04 release audit reproduced against the real banks: responses that
    // contain the right words and are wrong. None of these may ever be accepted again.
    [Theory]
    [InlineData("the Vice President", "The President (of the United States)")]
    [InlineData("not the President", "The President (of the United States)")]
    [InlineData("non-citizens", "Citizens")]
    [InlineData("permanent residents and citizens", "Citizens")]
    [InlineData("Theodore Roosevelt", "(Franklin) Roosevelt")]
    [InlineData("2 or 6 years", "Six (6) years")]
    [InlineData("twenty-six years", "Six (6) years")]
    [InlineData("twenty-five", "Five (5)")]
    [InlineData("four hundred thirty-five", "Five (5)")]
    [InlineData("one hundred and two", "One hundred (100)")]
    [InlineData("not George Washington", "(George) Washington")]
    [InlineData("Washington or Lincoln", "(George) Washington")]
    [InlineData("Senate, House of Representatives, and more", "Senate and House (of Representatives)")]
    [InlineData("constitution-based federal republic", "(U.S.) Constitution")]
    [InlineData("it says not all people are created equal", "All people are created equal")]
    [InlineData("powers not given to the states belong to the federal government or to the people", "(It states that the) powers not given to the federal government belong to the states or to the people.")]
    [InlineData("a state supreme court", "the Supreme Court")]
    [InlineData("Andrew Johnson", "Johnson")]
    public void Never_accepts_a_wrong_answer_that_contains_the_right_words(string response, string accepted)
    {
        var match = AnswerMatcher.Evaluate(response, [accepted]);

        Assert.False(match.IsAccepted, $"'{response}' was accepted as '{match.MatchedAnswer}' ({match.Kind}).");
    }

    [Theory]
    [InlineData("the President is commander in chief", "Who is Commander in Chief of the U.S. military?", "The President (of the United States)")]
    [InlineData("there are nine justices", "How many justices are on the Supreme Court?", "Nine (9)")]
    [InlineData("citizens can vote", "Who can vote in federal elections, run for federal office, and serve on a jury?", "Citizens")]
    [InlineData("Vice President Vance", "What is the name of the Vice President of the United States now?", "Vance")]
    public void Accepts_an_answer_that_echoes_the_question(string response, string prompt, string accepted)
    {
        var match = AnswerMatcher.Evaluate(response, [accepted], prompt);

        Assert.True(match.IsAccepted, $"'{response}' was {match.Kind}.");
        Assert.Equal(AnswerMatchKind.Contains, match.Kind);
    }

    [Fact]
    public void Echoing_the_question_does_not_make_a_wrong_answer_right()
    {
        var match = AnswerMatcher.Evaluate("the Vice President is commander in chief", ["The President (of the United States)"], "Who is Commander in Chief of the U.S. military?");

        Assert.False(match.IsAccepted);
    }

    [Theory]
    [InlineData("6", "Six (6) years", true)]
    [InlineData("6 years", "Six (6) years", true)]
    [InlineData("six years", "Six (6) years", true)]
    [InlineData("435", "Four hundred thirty-five (435)", true)]
    [InlineData("100", "One hundred (100)", true)]
    [InlineData("July 4th", "July 4", true)]
    [InlineData("eighteen and older", "eighteen (18) and older", true)]
    [InlineData("18 and older", "eighteen (18) and older", true)]
    [InlineData("18", "Citizens eighteen (18) and older (can vote).", false)]
    [InlineData("citizens under 18 can vote", "Citizens eighteen (18) and older (can vote).", false)]
    [InlineData("26", "between eighteen (18) and twenty-six (26)", false)]
    [InlineData("18", "at age eighteen (18)", false)]
    public void Numbers_are_read_in_place_and_stand_alone_only_when_the_answer_is_that_number(string response, string accepted, bool acceptedByMatcher)
    {
        Assert.Equal(acceptedByMatcher, AnswerMatcher.Evaluate(response, [accepted]).IsAccepted);
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
    public void A_longer_answer_is_not_broken_up_by_a_shorter_one()
    {
        string[] accepted = ["speech", "freedom of speech"];

        var match = AnswerMatcher.Evaluate("freedom of speech", accepted);

        Assert.True(match.IsAccepted);
        Assert.Equal("freedom of speech", match.MatchedAnswer);
    }

    [Fact]
    public void Accepted_matches_are_exact_or_contains_only()
    {
        Assert.True(AnswerMatcher.Evaluate("nine", ["nine (9)"]).IsAccepted);
        Assert.True(AnswerMatcher.Evaluate("I think nine", ["nine (9)"]).IsAccepted);
        Assert.False(AnswerMatcher.Evaluate("nien", ["nine (9)"]).IsAccepted);
        Assert.False(AnswerMatcher.Evaluate("ten", ["nine (9)"]).IsAccepted);
        Assert.False(AnswerMatcher.Evaluate("nine or ten", ["nine (9)"]).IsAccepted);
    }

    [Fact]
    public void A_wrong_answer_built_on_the_right_words_is_reported_as_close_to_it()
    {
        var match = AnswerMatcher.Evaluate("the Vice President", ["The President (of the United States)"]);

        Assert.Equal(AnswerMatchKind.Close, match.Kind);
        Assert.Equal("The President (of the United States)", match.MatchedAnswer);
    }

    [Theory]
    [InlineData("Christmas, Thanksgiving and Labor Day", 3, AnswerMatchKind.Exact)]
    [InlineData("Christmas and Thanksgiving", 2, AnswerMatchKind.Exact)]
    [InlineData("I'd say Christmas, Thanksgiving, New Year's Day", 3, AnswerMatchKind.Contains)]
    [InlineData("Christmas", 3, AnswerMatchKind.Partial)]
    [InlineData("Christmas Day", 3, AnswerMatchKind.Close)]
    [InlineData("Christmas and Thanksgiving", 3, AnswerMatchKind.Partial)]
    [InlineData("Christmas, Thanksgiving and my birthday", 3, AnswerMatchKind.Close)]
    [InlineData("Christmas, Christmas and Christmas", 3, AnswerMatchKind.Partial)]
    public void Counts_distinct_items_when_the_question_asks_for_several(string response, int required, AnswerMatchKind expected)
    {
        string[] holidays = ["New Year's Day", "Martin Luther King, Jr. Day", "Presidents' Day", "Memorial Day", "Juneteenth", "Independence Day", "Labor Day", "Columbus Day", "Veterans Day", "Thanksgiving", "Christmas"];

        var match = AnswerMatcher.Evaluate(response, holidays, "Name three national U.S. holidays.", required);

        Assert.Equal(expected, match.Kind);
        Assert.Equal(expected is AnswerMatchKind.Exact or AnswerMatchKind.Contains, match.IsAccepted);
    }

    [Fact]
    public void An_answer_containing_and_still_counts_as_one_item()
    {
        string[] positions = ["Secretary of State", "Secretary of Health and Human Services", "Attorney General"];

        var match = AnswerMatcher.Evaluate("Secretary of Health and Human Services and the Attorney General", positions, "What are two Cabinet-level positions?", 2);

        Assert.Equal(AnswerMatchKind.Exact, match.Kind);
        Assert.Equal("Secretary of Health and Human Services; Attorney General", match.MatchedAnswer);
    }

    [Fact]
    public void Parenthesised_parts_are_optional_and_numbers_stand_alone_only_for_number_answers()
    {
        Assert.Equal(["Twenty-seven (27)", "Twenty-seven 27", "Twenty-seven", "27"], AnswerMatcher.Variants("Twenty-seven (27)"));
        Assert.Equal(["Six (6) years", "Six 6 years", "Six years", "6"], AnswerMatcher.Variants("Six (6) years"));
        Assert.Equal(["(U.S.) Constitution", "U.S. Constitution", "Constitution"], AnswerMatcher.Variants("(U.S.) Constitution"));
        Assert.Equal(["Republic"], AnswerMatcher.Variants("Republic"));
        Assert.DoesNotContain("18", AnswerMatcher.Variants("Citizens eighteen (18) and older (can vote)."));
        Assert.DoesNotContain("26", AnswerMatcher.Variants("between eighteen (18) and twenty-six (26)"));
    }

    [Fact]
    public void Normalization_is_what_the_interface_can_explain()
    {
        Assert.Equal("the us constitution", AnswerMatcher.Normalize("The U.S. Constitution!"));
        Assert.Equal("twenty seven", AnswerMatcher.Normalize("Twenty-seven"));
        Assert.Equal("we the people", AnswerMatcher.Normalize("  We  the   People  "));
        Assert.Equal(string.Empty, AnswerMatcher.Normalize(null));
    }

    [Theory]
    [InlineData("twenty seven", "27")]
    [InlineData("four hundred thirty five members", "435 members")]
    [InlineData("one hundred and two", "102")]
    [InlineData("one two", "1 2")]
    [InlineData("five twenty", "5 20")]
    [InlineData("july 4th", "july 4")]
    [InlineData("the 16th president", "16 president")]
    public void Content_tokens_read_numbers(string normalized, string expectedTokens)
    {
        Assert.Equal(expectedTokens, string.Join(' ', AnswerMatcher.ContentTokens(normalized)));
    }
}
