using Citiz.Core.Exams;
using Citiz.Testing;

namespace Citiz.Content.Tests;

/// <summary>
/// The checker against the banks that ship: every official answer, typed as printed, is accepted for
/// its own question, and the wrong answers the 2026-10-04 release audit found graded "Correct" are
/// not. A study aid for a federal exam must get both directions right.
/// </summary>
public sealed class ShippedBankGradingTests
{
    private static ContentRepository Repository() => new(new FileContentStore(RepositoryPaths.Content));

    [Theory]
    [InlineData("2008")]
    [InlineData("2025")]
    public async Task Every_official_answer_is_accepted_for_its_own_question(string versionId)
    {
        var repository = Repository();
        var bank = await repository.GetQuestionBankAsync(versionId);
        var dynamicAnswers = await repository.GetDynamicAnswersAsync();
        var rejected = new List<string>();

        foreach (var question in bank.Questions)
        {
            var answers = question.ResolveAnswers(dynamicAnswers);
            if (answers.Count == 0)
            {
                continue;
            }

            if (question.AsksForSeveral)
            {
                // Any N of the official items, in either list order.
                var forward = string.Join(", ", answers.Take(question.RequiredCount));
                var backward = string.Join(" and ", answers.Reverse().Take(question.RequiredCount));
                foreach (var response in new[] { forward, backward })
                {
                    if (!AnswerMatcher.Evaluate(response, answers, question.Prompt, question.RequiredCount).IsAccepted)
                    {
                        rejected.Add($"{question.Id}: '{response}'");
                    }
                }

                continue;
            }

            foreach (var answer in answers)
            {
                foreach (var variant in AnswerMatcher.Variants(answer))
                {
                    var match = AnswerMatcher.Evaluate(variant, answers, question.Prompt);
                    if (!match.IsAccepted)
                    {
                        rejected.Add($"{question.Id}: '{variant}' -> {match.Kind}");
                    }
                }
            }
        }

        Assert.Empty(rejected);
    }

    [Theory]
    [InlineData("2025", 42, "the Vice President")]
    [InlineData("2025", 42, "not the President")]
    [InlineData("2025", 44, "the Vice President")]
    [InlineData("2025", 64, "non-citizens")]
    [InlineData("2025", 64, "permanent residents and citizens")]
    [InlineData("2025", 105, "Theodore Roosevelt")]
    [InlineData("2025", 126, "Christmas Day")]
    [InlineData("2025", 81, "Georgia")]
    [InlineData("2025", 22, "2 or 6 years")]
    [InlineData("2025", 22, "twenty-six years")]
    [InlineData("2025", 36, "4 or 8 years")]
    [InlineData("2025", 54, "twenty-five")]
    [InlineData("2025", 63, "18")]
    [InlineData("2025", 63, "citizens under 18 can vote")]
    [InlineData("2025", 60, "powers not given to the states belong to the federal government or to the people")]
    [InlineData("2025", 1, "not a republic, a monarchy")]
    [InlineData("2008", 15, "Vice President")]
    [InlineData("2008", 32, "the Vice President")]
    [InlineData("2008", 57, "26")]
    [InlineData("2008", 19, "twenty-six")]
    [InlineData("2008", 80, "Theodore Roosevelt")]
    [InlineData("2008", 47, "Andrew Johnson")]
    [InlineData("2008", 28, "Donald Trump Jr.")]
    [InlineData("2008", 38, "a state supreme court")]
    [InlineData("2008", 1, "not the Constitution")]
    [InlineData("2008", 100, "Christmas")]
    public async Task Wrong_answers_from_the_release_audit_are_not_accepted(string versionId, int number, string response)
    {
        var repository = Repository();
        var bank = await repository.GetQuestionBankAsync(versionId);
        var dynamicAnswers = await repository.GetDynamicAnswersAsync();
        var question = bank.FindByNumber(number)!;

        var match = AnswerMatcher.Evaluate(response, question.ResolveAnswers(dynamicAnswers), question.Prompt, question.RequiredCount);

        Assert.False(match.IsAccepted, $"{question.Id} '{question.Prompt}' accepted '{response}' as '{match.MatchedAnswer}' ({match.Kind}).");
    }

    [Theory]
    [InlineData("2025", 126, "Christmas, Thanksgiving and Labor Day")]
    [InlineData("2025", 81, "Georgia, Virginia, Delaware, New York and New Jersey")]
    [InlineData("2025", 42, "the President")]
    [InlineData("2025", 42, "I think it is the President")]
    [InlineData("2025", 64, "citizens")]
    [InlineData("2025", 105, "Franklin Roosevelt")]
    [InlineData("2025", 105, "Roosevelt")]
    [InlineData("2025", 22, "six years")]
    [InlineData("2025", 22, "6")]
    [InlineData("2025", 7, "27")]
    [InlineData("2025", 7, "twenty-seven")]
    [InlineData("2025", 24, "435")]
    [InlineData("2008", 57, "between 18 and 26")]
    [InlineData("2008", 99, "July 4th")]
    [InlineData("2008", 100, "Christmas and Thanksgiving")]
    [InlineData("2008", 36, "Secretary of Health and Human Services and the Attorney General")]
    public async Task Natural_correct_answers_are_accepted(string versionId, int number, string response)
    {
        var repository = Repository();
        var bank = await repository.GetQuestionBankAsync(versionId);
        var dynamicAnswers = await repository.GetDynamicAnswersAsync();
        var question = bank.FindByNumber(number)!;

        var match = AnswerMatcher.Evaluate(response, question.ResolveAnswers(dynamicAnswers), question.Prompt, question.RequiredCount);

        Assert.True(match.IsAccepted, $"{question.Id} '{question.Prompt}' graded '{response}' as {match.Kind} ({match.MatchedAnswer}).");
    }

    [Theory]
    [InlineData("2008", new[] { 9, 36, 51, 55, 64, 100 })]
    [InlineData("2025", new[] { 10, 48, 65, 67, 69, 81, 126 })]
    public async Task Questions_that_ask_to_name_several_items_require_them(string versionId, int[] numbers)
    {
        var bank = await Repository().GetQuestionBankAsync(versionId);

        Assert.Equal(numbers, bank.Questions.Where(q => q.AsksForSeveral).Select(q => q.Number));
        Assert.All(numbers.Select(n => bank.FindByNumber(n)!), q => Assert.Equal(AnswerMatchKind.Partial, AnswerMatcher.Evaluate(q.AcceptedAnswers[0], q.AcceptedAnswers, q.Prompt, q.RequiredCount).Kind));
    }
}
