using Citiz.Core.Exams;

namespace Citiz.Core.Tests;

public sealed class ExamSessionTests
{
    [Fact]
    public void Draws_the_configured_number_of_distinct_questions()
    {
        var bank = TestData.Bank(TestData.V2025, 128);

        var session = ExamSession.Start(TestData.V2025, bank, random: new Random(1));

        Assert.Equal(20, session.Questions.Count);
        Assert.Equal(20, session.Questions.Select(q => q.Id).Distinct().Count());
    }

    [Fact]
    public void Same_seed_draws_the_same_questions()
    {
        var bank = TestData.Bank(TestData.V2025, 128);

        var first = ExamSession.Start(TestData.V2025, bank, random: new Random(42));
        var second = ExamSession.Start(TestData.V2025, bank, random: new Random(42));

        Assert.Equal(first.Questions.Select(q => q.Id), second.Questions.Select(q => q.Id));
    }

    [Fact]
    public void Stops_as_soon_as_the_passing_score_is_reached()
    {
        var session = ExamSession.Start(TestData.V2025, TestData.Bank(TestData.V2025, 128), random: new Random(1));

        for (var i = 0; i < 12; i++)
        {
            Assert.False(session.IsComplete);
            session.Record(correct: true);
        }

        Assert.True(session.IsComplete);
        Assert.Equal(ExamOutcome.Passed, session.Outcome);
        Assert.Equal(12, session.Position);
        Assert.Null(session.CurrentQuestion);
    }

    [Fact]
    public void Stops_as_soon_as_the_failing_count_is_reached()
    {
        var session = ExamSession.Start(TestData.V2008, TestData.Bank(TestData.V2008, 100), random: new Random(1));

        for (var i = 0; i < 5; i++)
        {
            session.Record(correct: false);
        }

        Assert.Equal(ExamOutcome.Failed, session.Outcome);
        Assert.True(session.IsComplete);
        Assert.Throws<InvalidOperationException>(() => session.Record(correct: true));
    }

    [Fact]
    public void Every_sitting_ends_decided()
    {
        // Alternate right and wrong: 11 right, 8 wrong is the closest an applicant can get to running
        // out of questions, and the 20th question still decides it.
        var session = ExamSession.Start(TestData.V2025, TestData.Bank(TestData.V2025, 128), random: new Random(7));

        var correct = true;
        while (!session.IsComplete)
        {
            session.Record(correct);
            correct = !correct;
        }

        Assert.NotEqual(ExamOutcome.InProgress, session.Outcome);
        Assert.True(session.Position <= 20);
    }

    [Fact]
    public void Senior_sitting_uses_only_designated_questions_and_its_own_rules()
    {
        var bank = TestData.Bank(TestData.V2008, 100);

        var session = ExamSession.Start(TestData.V2008, bank, seniorConsideration: true, random: new Random(3));

        Assert.Equal(10, session.Questions.Count);
        Assert.All(session.Questions, q => Assert.Contains(q.Number, TestData.V2008.SeniorQuestionNumbers));
        Assert.Equal(6, session.Rules.PassingAnswers);
    }

    [Fact]
    public void Senior_sitting_is_refused_until_the_designation_is_recorded()
    {
        var bank = TestData.Bank(TestData.V2025, 128);

        var ex = Assert.Throws<InvalidOperationException>(() => ExamSession.Start(TestData.V2025, bank, seniorConsideration: true));

        Assert.Contains("65/20", ex.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Refuses_a_bank_from_another_version()
    {
        Assert.Throws<ArgumentException>(() => ExamSession.Start(TestData.V2025, TestData.Bank(TestData.V2008, 100)));
    }

    [Fact]
    public void Records_the_response_with_each_answer()
    {
        var session = ExamSession.Start(TestData.V2025, TestData.Bank(TestData.V2025, 128), random: new Random(1));

        session.Record(correct: true, response: "the Constitution");

        var record = Assert.Single(session.History);
        Assert.Equal("the Constitution", record.Response);
        Assert.True(record.Correct);
        Assert.Equal(session.Questions[0], record.Question);
    }
}
