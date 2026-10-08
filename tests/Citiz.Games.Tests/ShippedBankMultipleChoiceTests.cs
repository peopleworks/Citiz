using Citiz.Content;
using Citiz.Core.Exams;
using Citiz.Testing;

namespace Citiz.Games.Tests;

/// <summary>
/// Sweeps every question of both shipped banks through the multiple-choice builder many times: no
/// item may ever offer, as a wrong option, something the checker would accept for that question.
/// The 2026-10-04 release audit found "the Civil War" next to "Civil War" and "The President" marked
/// wrong for "Who vetoes bills?"; this is the regression test for that class.
/// </summary>
public sealed class ShippedBankMultipleChoiceTests
{
    private const int SeedsPerQuestion = 40;

    private static ContentRepository Repository() => new(new FileContentStore(RepositoryPaths.Content));

    [Theory]
    [InlineData("2008")]
    [InlineData("2025")]
    public async Task No_distractor_is_an_accepted_answer_to_its_own_question(string versionId)
    {
        var repository = Repository();
        var bank = await repository.GetQuestionBankAsync(versionId);
        var dynamicAnswers = await repository.GetDynamicAnswersAsync();
        var collisions = new List<string>();
        var built = 0;

        foreach (var question in bank.Questions)
        {
            var answers = question.ResolveAnswers(dynamicAnswers);
            for (var seed = 0; seed < SeedsPerQuestion; seed++)
            {
                var item = MultipleChoiceBuilder.Build(question, bank, dynamicAnswers, new Random(seed));
                if (item is null)
                {
                    continue;
                }

                built++;
                foreach (var (option, index) in item.Options.Select((o, i) => (o, i)))
                {
                    if (index == item.CorrectIndex)
                    {
                        continue;
                    }

                    var match = AnswerMatcher.Evaluate(option, answers, question.Prompt);
                    if (match.Kind != AnswerMatchKind.None)
                    {
                        collisions.Add($"{question.Id} offers '{option}' as wrong, but the checker says {match.Kind} ({match.MatchedAnswer}).");
                    }
                }
            }
        }

        Assert.True(built > 0);
        Assert.Empty(collisions.Distinct());
    }

    [Theory]
    [InlineData("2008")]
    [InlineData("2025")]
    public async Task Questions_that_ask_for_several_items_are_not_offered_as_multiple_choice(string versionId)
    {
        var repository = Repository();
        var bank = await repository.GetQuestionBankAsync(versionId);
        var dynamicAnswers = await repository.GetDynamicAnswersAsync();

        var several = bank.Questions.Where(q => q.AsksForSeveral).ToList();

        Assert.NotEmpty(several);
        Assert.All(several, q => Assert.Null(MultipleChoiceBuilder.Build(q, bank, dynamicAnswers, new Random(1))));
    }

    [Theory]
    [InlineData("2008")]
    [InlineData("2025")]
    public async Task Officeholder_names_are_never_offered_as_wrong_options(string versionId)
    {
        var repository = Repository();
        var bank = await repository.GetQuestionBankAsync(versionId);
        var dynamicAnswers = await repository.GetDynamicAnswersAsync();
        // People, not counts: "nine (9)" is both the dynamic number of justices and a static answer elsewhere.
        var officeholders = dynamicAnswers.Values
            .Where(d => d.Holder is not null && !AnswerMatcher.ContentTokens(AnswerMatcher.Normalize(d.Holder)).All(t => t.All(char.IsDigit)))
            .SelectMany(d => d.AcceptedAnswers)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var question in bank.Questions.Where(q => !q.IsDynamic))
        {
            for (var seed = 0; seed < SeedsPerQuestion; seed++)
            {
                var item = MultipleChoiceBuilder.Build(question, bank, dynamicAnswers, new Random(seed));
                if (item is null)
                {
                    continue;
                }

                Assert.DoesNotContain(item.Options.Where((_, i) => i != item.CorrectIndex), officeholders.Contains);
            }
        }
    }
}
