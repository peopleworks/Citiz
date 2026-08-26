using Citiz.Content.Validation;
using Citiz.Testing;

namespace Citiz.Content.Tests;

public sealed class ContentValidatorTests
{
    private const string Source = """{ "authority": "USCIS", "title": "T", "url": "https://www.uscis.gov/", "verifiedOn": null, "license": "Public domain" }""";

    private static string Versions(string seniorNumbers = "[1, 2]", string filingTo2008 = "\"2025-10-19\"") => $$"""
        {
          "versions": [
            { "id": "2008", "displayName": "2008", "filingFrom": null, "filingTo": {{filingTo2008}}, "bankSize": 2,
              "standard": { "questionsAsked": 2, "passingAnswers": 2, "failingAnswers": 1 },
              "seniorConsideration": { "questionsAsked": 2, "passingAnswers": 2, "failingAnswers": 1 },
              "seniorQuestionNumbers": {{seniorNumbers}}, "reviewStatus": "approved", "sources": [{{Source}}] },
            { "id": "2025", "displayName": "2025", "filingFrom": "2025-10-20", "filingTo": null, "bankSize": 2,
              "standard": { "questionsAsked": 2, "passingAnswers": 2, "failingAnswers": 1 },
              "seniorConsideration": { "questionsAsked": 2, "passingAnswers": 2, "failingAnswers": 1 },
              "seniorQuestionNumbers": [], "reviewStatus": "needs-review", "sources": [{{Source}}] }
          ]
        }
        """;

    private static string Bank(string versionId, string secondId = null!, string dynamicKey = "president") => $$"""
        {
          "versionId": "{{versionId}}", "reviewStatus": "needs-review", "sources": [{{Source}}],
          "questions": [
            { "id": "{{versionId}}-001", "number": 1, "category": "C", "subcategory": "S", "prompt": "P?", "acceptedAnswers": ["A"] },
            { "id": "{{secondId ?? versionId + "-002"}}", "number": 2, "category": "C", "subcategory": "S", "prompt": "Who?", "acceptedAnswers": [], "dynamicAnswerKey": "{{dynamicKey}}" }
          ]
        }
        """;

    private const string DynamicAnswers = $$"""
        { "answers": [ { "key": "president", "office": "President", "scope": "federal", "holder": "X", "acceptedAnswers": ["X"], "since": null, "verifiedOn": null, "lookupHint": null, "reviewStatus": "needs-review", "sources": [{{Source}}] } ] }
        """;

    private const string Vocabulary = $$"""
        { "kind": "{KIND}", "reviewStatus": "approved", "sources": [{{Source}}], "groups": [ { "category": "People", "words": ["Lincoln"] } ] }
        """;

    private static string Topics(string relatedId = "2025-001") => $$"""
        { "topics": [ { "id": "t", "category": "history", "title": "T", "summary": "S", "simpleEnglish": "E", "estimatedMinutes": 3, "difficulty": "beginner",
            "vocabulary": ["a"], "relatedQuestionIds": ["{{relatedId}}"], "relatedPlaces": [], "reviewStatus": "draft", "sources": [{{Source}}] } ] }
        """;

    private const string Sources = """
        { "sources": [ { "id": "s", "authority": "USCIS", "title": "T", "url": "https://www.uscis.gov/", "format": "html", "checkEvery": "P7D", "monitor": true, "requiresHumanReview": true, "feeds": ["exams/versions.json"], "lastHash": null, "lastCheckedOn": null } ] }
        """;

    private static MemoryContentStore Valid() => new MemoryContentStore()
        .With(ContentPaths.ExamVersions, Versions())
        .With(ContentPaths.Questions("2008"), Bank("2008"))
        .With(ContentPaths.Questions("2025"), Bank("2025"))
        .With(ContentPaths.DynamicAnswers, DynamicAnswers)
        .With(ContentPaths.ReadingVocabulary, Vocabulary.Replace("{KIND}", "reading", StringComparison.Ordinal))
        .With(ContentPaths.WritingVocabulary, Vocabulary.Replace("{KIND}", "writing", StringComparison.Ordinal))
        .With(ContentPaths.DiscoveryTopics, Topics())
        .With(ContentPaths.MonitoredSources, Sources);

    [Fact]
    public async Task A_consistent_repository_is_valid_and_reports_what_is_unverified()
    {
        var report = await new ContentValidator(Valid()).ValidateAsync();

        Assert.True(report.IsValid, string.Join("; ", report.Issues.Select(i => i.Message)));
        Assert.Contains(report.Issues, i => i.Severity == ContentIssueSeverity.Warning && i.Message.Contains("65/20", StringComparison.Ordinal));
        Assert.Equal(2, report.Reviews.Single(r => r.File == ContentPaths.Questions("2008")).Pending);
    }

    [Fact]
    public async Task Bank_size_mismatch_is_an_error()
    {
        var store = Valid().With(ContentPaths.ExamVersions, Versions().Replace("\"bankSize\": 2", "\"bankSize\": 3", StringComparison.Ordinal));

        var report = await new ContentValidator(store).ValidateAsync();

        Assert.Contains(report.Issues, i => i.Severity == ContentIssueSeverity.Error && i.Message.Contains("bankSize", StringComparison.Ordinal));
    }

    [Fact]
    public async Task Wrong_question_id_is_an_error()
    {
        var store = Valid().With(ContentPaths.Questions("2025"), Bank("2025", secondId: "2025-099"));

        var report = await new ContentValidator(store).ValidateAsync();

        Assert.Contains(report.Issues, i => i.Severity == ContentIssueSeverity.Error && i.Message.Contains("should have id '2025-002'", StringComparison.Ordinal));
    }

    [Fact]
    public async Task Unknown_dynamic_key_is_an_error()
    {
        var store = Valid().With(ContentPaths.Questions("2025"), Bank("2025", dynamicKey: "emperor"));

        var report = await new ContentValidator(store).ValidateAsync();

        Assert.Contains(report.Issues, i => i.Severity == ContentIssueSeverity.Error && i.Message.Contains("'emperor'", StringComparison.Ordinal));
    }

    [Fact]
    public async Task Overlapping_versions_are_an_error()
    {
        var store = Valid().With(ContentPaths.ExamVersions, Versions(filingTo2008: "\"2025-10-20\""));

        var report = await new ContentValidator(store).ValidateAsync();

        Assert.Contains(report.Issues, i => i.Severity == ContentIssueSeverity.Error && i.Message.Contains("overlap", StringComparison.Ordinal));
    }

    [Fact]
    public async Task Senior_designation_must_point_at_real_questions()
    {
        var store = Valid().With(ContentPaths.ExamVersions, Versions(seniorNumbers: "[1, 99]"));

        var report = await new ContentValidator(store).ValidateAsync();

        Assert.Contains(report.Issues, i => i.Severity == ContentIssueSeverity.Error && i.Message.Contains("question 99", StringComparison.Ordinal));
    }

    [Fact]
    public async Task Topic_referencing_a_missing_question_is_an_error()
    {
        var store = Valid().With(ContentPaths.DiscoveryTopics, Topics("2025-777"));

        var report = await new ContentValidator(store).ValidateAsync();

        Assert.Contains(report.Issues, i => i.Severity == ContentIssueSeverity.Error && i.Message.Contains("2025-777", StringComparison.Ordinal));
    }

    [Fact]
    public async Task Malformed_json_and_unknown_properties_are_errors_not_crashes()
    {
        var store = Valid()
            .With(ContentPaths.DynamicAnswers, "{ not json")
            .With(ContentPaths.ReadingVocabulary, """{ "kind": "reading", "reviewStatus": "approved", "sources": [], "groupz": [] }""");

        var report = await new ContentValidator(store).ValidateAsync();

        Assert.Contains(report.Issues, i => i.File == ContentPaths.DynamicAnswers && i.Message.Contains("invalid JSON", StringComparison.Ordinal));
        Assert.Contains(report.Issues, i => i.File == ContentPaths.ReadingVocabulary && i.Severity == ContentIssueSeverity.Error);
    }

    [Fact]
    public async Task Missing_file_is_an_error()
    {
        var store = Valid().With(ContentPaths.Questions("2025"), Bank("2025"));
        var withoutSources = new MemoryContentStore();
        foreach (var path in new[] { ContentPaths.ExamVersions, ContentPaths.Questions("2008"), ContentPaths.Questions("2025"), ContentPaths.DynamicAnswers, ContentPaths.ReadingVocabulary, ContentPaths.WritingVocabulary, ContentPaths.DiscoveryTopics })
        {
            await using var stream = await store.OpenReadAsync(path);
            using var reader = new StreamReader(stream);
            withoutSources.With(path, await reader.ReadToEndAsync());
        }

        var report = await new ContentValidator(withoutSources).ValidateAsync();

        Assert.Contains(report.Issues, i => i.File == ContentPaths.MonitoredSources && i.Message.Contains("missing", StringComparison.Ordinal));
    }

    [Fact]
    public void Mapper_rejects_inconsistent_rules_with_the_file_name()
    {
        var file = new Files.ExamVersionsFile
        {
            Versions =
            [
                new Files.ExamVersionEntry
                {
                    Id = "2030",
                    DisplayName = "x",
                    BankSize = 10,
                    Standard = new Files.RulesEntry { QuestionsAsked = 10, PassingAnswers = 6, FailingAnswers = 6 },
                    SeniorConsideration = new Files.RulesEntry { QuestionsAsked = 10, PassingAnswers = 6, FailingAnswers = 5 },
                    ReviewStatus = "approved",
                },
            ],
        };

        var ex = Assert.Throws<ContentFormatException>(() => ContentMapper.ToExamVersions(file));

        Assert.Equal(ContentPaths.ExamVersions, ex.File);
        Assert.Contains("version '2030' standard", ex.Message, StringComparison.Ordinal);
    }
}
