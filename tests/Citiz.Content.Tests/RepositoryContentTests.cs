using Citiz.Content.Validation;
using Citiz.Core.Audio;
using Citiz.Core.English;
using Citiz.Core.Exams;
using Citiz.Testing;

namespace Citiz.Content.Tests;

/// <summary>
/// The content that ships in this repository must load and validate. These tests are the reason a
/// broken content pull request cannot merge.
/// </summary>
public sealed class RepositoryContentTests
{
    private static ContentRepository Repository() => new(new FileContentStore(RepositoryPaths.Content));

    [Fact]
    public async Task Shipped_content_has_no_validation_errors()
    {
        var report = await new ContentValidator(new FileContentStore(RepositoryPaths.Content)).ValidateAsync();

        var errors = report.Issues.Where(i => i.Severity == ContentIssueSeverity.Error).Select(i => $"{i.File}: {i.Message}");
        Assert.True(report.IsValid, string.Join(Environment.NewLine, errors));
    }

    [Fact]
    public async Task Both_official_versions_are_present_with_their_rules()
    {
        var versions = await Repository().GetExamVersionsAsync();

        var v2008 = Assert.Single(versions, v => v.Id == "2008");
        var v2025 = Assert.Single(versions, v => v.Id == "2025");

        Assert.Equal(new DateOnly(2025, 10, 19), v2008.FilingTo);
        Assert.Equal(new DateOnly(2025, 10, 20), v2025.FilingFrom);
        Assert.Equal((100, 10, 6, 5), (v2008.BankSize, v2008.Standard.QuestionsAsked, v2008.Standard.PassingAnswers, v2008.Standard.FailingAnswers));
        Assert.Equal((128, 20, 12, 9), (v2025.BankSize, v2025.Standard.QuestionsAsked, v2025.Standard.PassingAnswers, v2025.Standard.FailingAnswers));
        Assert.Equal(20, v2008.SeniorQuestionNumbers.Count);
    }

    [Theory]
    [InlineData("2008", 100)]
    [InlineData("2025", 128)]
    public async Task Banks_are_complete(string versionId, int expectedCount)
    {
        var bank = await Repository().GetQuestionBankAsync(versionId);

        Assert.Equal(expectedCount, bank.Count);
        Assert.Equal(Enumerable.Range(1, expectedCount), bank.Questions.Select(q => q.Number));
        Assert.All(bank.Questions, q => Assert.True(q.IsDynamic || q.AcceptedAnswers.Count > 0, $"{q.Id} has no answers"));
        Assert.NotEmpty(bank.Sources);
    }

    [Fact]
    public async Task Every_dynamic_question_has_an_entry()
    {
        var repository = Repository();
        var dynamicAnswers = await repository.GetDynamicAnswersAsync();

        foreach (var versionId in new[] { "2008", "2025" })
        {
            var bank = await repository.GetQuestionBankAsync(versionId);
            foreach (var question in bank.Questions.Where(q => q.IsDynamic))
            {
                Assert.True(dynamicAnswers.ContainsKey(question.DynamicAnswerKey!), $"{question.Id} references '{question.DynamicAnswerKey}'");
            }
        }
    }

    [Fact]
    public async Task The_supreme_law_of_the_land_is_the_constitution_in_both_versions()
    {
        var repository = Repository();

        var q2008 = (await repository.GetQuestionBankAsync("2008")).FindByNumber(1)!;
        var q2025 = (await repository.GetQuestionBankAsync("2025")).FindByNumber(2)!;

        Assert.True(AnswerMatcher.Evaluate("the Constitution", q2008.AcceptedAnswers).IsAccepted);
        Assert.True(AnswerMatcher.Evaluate("the Constitution", q2025.AcceptedAnswers).IsAccepted);
    }

    [Fact]
    public async Task A_filing_date_resolves_to_a_loadable_bank()
    {
        var exam = await Repository().GetExamForFilingDateAsync(new DateOnly(2026, 1, 15));

        Assert.NotNull(exam);
        Assert.Equal("2025", exam.Value.Version.Id);
        Assert.Equal(128, exam.Value.Bank.Count);
    }

    [Fact]
    public async Task Vocabulary_lists_and_topics_load()
    {
        var repository = Repository();

        var reading = await repository.GetVocabularyAsync(VocabularyKind.Reading);
        var writing = await repository.GetVocabularyAsync(VocabularyKind.Writing);
        var topics = await repository.GetDiscoveryTopicsAsync();

        Assert.Contains("citizen", reading.AllWords, StringComparer.OrdinalIgnoreCase);
        Assert.Contains("Congress", writing.AllWords, StringComparer.OrdinalIgnoreCase);
        Assert.NotEmpty(topics);
        Assert.All(topics, t => Assert.NotEmpty(t.Sources));
    }

    [Fact]
    public async Task Repository_caches_per_file_and_invalidates_on_demand()
    {
        var repository = Repository();

        var first = await repository.GetExamVersionsAsync();
        var second = await repository.GetExamVersionsAsync();
        repository.Invalidate();
        var third = await repository.GetExamVersionsAsync();

        Assert.Same(first, second);
        Assert.NotSame(first, third);
    }

    [Fact]
    public async Task The_official_2008_audio_pack_covers_every_question_once()
    {
        var repository = Repository();
        var packs = await repository.GetAudioPacksAsync();
        var bank = await repository.GetQuestionBankAsync("2008");

        var pack = Assert.Single(packs, p => p.Id == "uscis-2008");
        Assert.Equal(AudioPackKind.Official, pack.Kind);
        Assert.Equal(100, pack.Clips.Count);
        Assert.All(pack.Clips, c => Assert.Equal(AudioClipRole.Recording, c.Role));
        Assert.Equal(bank.Questions.Select(q => q.Id).OrderBy(id => id), pack.Clips.Select(c => c.QuestionId).OrderBy(id => id));
        Assert.Equal(pack.Clips.Sum(c => c.Bytes), pack.SizeBytes);
        Assert.NotNull(pack.RecordingFor("2008-036"));
        Assert.Null(pack.PromptFor("2008-036"));
        Assert.NotEmpty(pack.Sources);
    }
}
