using System.Globalization;
using System.Xml;
using Citiz.Content.Files;
using Citiz.Content.Sources;
using Citiz.Core.Content;
using Citiz.Core.Discovery;
using Citiz.Core.English;
using Citiz.Core.Exams;

namespace Citiz.Content;

/// <summary>
/// Turns the file DTOs into domain objects, rejecting anything the domain cannot represent. Every
/// error names the file and the entry, because the person fixing it is usually a content
/// contributor reading the CLI output, not a developer with a debugger.
/// </summary>
public static class ContentMapper
{
    /// <summary>Maps <c>exams/versions.json</c>.</summary>
    /// <exception cref="ContentFormatException">An entry is incomplete or inconsistent.</exception>
    public static IReadOnlyList<ExamVersion> ToExamVersions(ExamVersionsFile file, string path = ContentPaths.ExamVersions)
    {
        ArgumentNullException.ThrowIfNull(file);

        return file.Versions.Select(entry =>
        {
            var id = Require(entry.Id, path, "versions[].id");
            var where = $"version '{id}'";
            return new ExamVersion(
                id,
                Require(entry.DisplayName, path, $"{where} displayName"),
                entry.FilingFrom,
                entry.FilingTo,
                Positive(entry.BankSize, path, $"{where} bankSize"),
                ToRules(entry.Standard, path, $"{where} standard"),
                ToRules(entry.SeniorConsideration, path, $"{where} seniorConsideration"),
                entry.SeniorQuestionNumbers.AsReadOnly(),
                ToReviewStatus(entry.ReviewStatus, path, where),
                ToSources(entry.Sources, path, where));
        }).ToList();
    }

    /// <summary>Maps <c>exams/&lt;version&gt;/questions.json</c>.</summary>
    /// <exception cref="ContentFormatException">An entry is incomplete or inconsistent.</exception>
    public static QuestionBank ToQuestionBank(QuestionsFile file, string path)
    {
        ArgumentNullException.ThrowIfNull(file);

        var versionId = Require(file.VersionId, path, "versionId");
        var defaultStatus = ToReviewStatus(file.ReviewStatus, path, "file");

        var questions = file.Questions.Select(entry =>
        {
            var id = Require(entry.Id, path, "questions[].id");
            var where = $"question '{id}'";
            return new CivicsQuestion(
                id,
                versionId,
                Positive(entry.Number, path, $"{where} number"),
                Require(entry.Category, path, $"{where} category"),
                Require(entry.Subcategory, path, $"{where} subcategory"),
                Require(entry.Prompt, path, $"{where} prompt"),
                entry.AcceptedAnswers.AsReadOnly(),
                Optional(entry.DynamicAnswerKey),
                Optional(entry.Note),
                entry.ReviewStatus is null ? defaultStatus : ToReviewStatus(entry.ReviewStatus, path, where));
        }).ToList();

        try
        {
            return new QuestionBank(versionId, questions, defaultStatus, ToSources(file.Sources, path, "file"));
        }
        catch (ArgumentException ex)
        {
            throw new ContentFormatException(path, ex.Message, ex);
        }
    }

    /// <summary>Maps <c>exams/dynamic-answers.json</c> to a dictionary keyed by <see cref="DynamicAnswer.Key"/>.</summary>
    /// <exception cref="ContentFormatException">An entry is incomplete, or a key repeats.</exception>
    public static IReadOnlyDictionary<string, DynamicAnswer> ToDynamicAnswers(DynamicAnswersFile file, string path = ContentPaths.DynamicAnswers)
    {
        ArgumentNullException.ThrowIfNull(file);

        var result = new Dictionary<string, DynamicAnswer>(StringComparer.Ordinal);
        foreach (var entry in file.Answers)
        {
            var key = Require(entry.Key, path, "answers[].key");
            var where = $"answer '{key}'";
            var answer = new DynamicAnswer(
                key,
                Require(entry.Office, path, $"{where} office"),
                ToScope(entry.Scope, path, where),
                Optional(entry.Holder),
                entry.AcceptedAnswers.AsReadOnly(),
                entry.Since,
                entry.VerifiedOn,
                Optional(entry.LookupHint),
                ToReviewStatus(entry.ReviewStatus, path, where),
                ToSources(entry.Sources, path, where));

            if (!result.TryAdd(key, answer))
            {
                throw new ContentFormatException(path, $"dynamic answer key '{key}' appears more than once.");
            }
        }

        return result;
    }

    /// <summary>Maps a vocabulary file.</summary>
    /// <exception cref="ContentFormatException">An entry is incomplete or the kind is unknown.</exception>
    public static VocabularyList ToVocabulary(VocabularyFile file, string path)
    {
        ArgumentNullException.ThrowIfNull(file);

        var kind = Require(file.Kind, path, "kind").ToLowerInvariant() switch
        {
            "reading" => VocabularyKind.Reading,
            "writing" => VocabularyKind.Writing,
            var other => throw new ContentFormatException(path, $"kind '{other}' must be 'reading' or 'writing'."),
        };

        var groups = file.Groups.Select(g => new VocabularyGroup(
            Require(g.Category, path, "groups[].category"),
            g.Words.AsReadOnly())).ToList();

        return new VocabularyList(kind, groups, ToReviewStatus(file.ReviewStatus, path, "file"), ToSources(file.Sources, path, "file"));
    }

    /// <summary>Maps <c>discovery/topics.json</c>.</summary>
    /// <exception cref="ContentFormatException">An entry is incomplete.</exception>
    public static IReadOnlyList<DiscoveryTopic> ToDiscoveryTopics(DiscoveryTopicsFile file, string path = ContentPaths.DiscoveryTopics)
    {
        ArgumentNullException.ThrowIfNull(file);

        return file.Topics.Select(entry =>
        {
            var id = Require(entry.Id, path, "topics[].id");
            var where = $"topic '{id}'";
            return new DiscoveryTopic(
                id,
                Require(entry.Category, path, $"{where} category"),
                Require(entry.Title, path, $"{where} title"),
                Require(entry.Summary, path, $"{where} summary"),
                Require(entry.SimpleEnglish, path, $"{where} simpleEnglish"),
                Positive(entry.EstimatedMinutes, path, $"{where} estimatedMinutes"),
                Require(entry.Difficulty, path, $"{where} difficulty"),
                entry.Vocabulary.AsReadOnly(),
                entry.RelatedQuestionIds.AsReadOnly(),
                entry.RelatedPlaces.AsReadOnly(),
                ToReviewStatus(entry.ReviewStatus, path, where),
                ToSources(entry.Sources, path, where));
        }).ToList();
    }

    /// <summary>Maps <c>sources/sources.json</c>.</summary>
    /// <exception cref="ContentFormatException">An entry is incomplete or a duration is malformed.</exception>
    public static IReadOnlyList<MonitoredSource> ToMonitoredSources(SourcesFile file, string path = ContentPaths.MonitoredSources)
    {
        ArgumentNullException.ThrowIfNull(file);

        return file.Sources.Select(entry =>
        {
            var id = Require(entry.Id, path, "sources[].id");
            var where = $"source '{id}'";
            return new MonitoredSource(
                id,
                Require(entry.Authority, path, $"{where} authority"),
                Require(entry.Title, path, $"{where} title"),
                ToUri(Require(entry.Url, path, $"{where} url"), path, where),
                Require(entry.Format, path, $"{where} format"),
                ToDuration(Require(entry.CheckEvery, path, $"{where} checkEvery"), path, where),
                entry.Monitor,
                entry.RequiresHumanReview,
                entry.Feeds.AsReadOnly(),
                Optional(entry.LastHash),
                entry.LastCheckedOn);
        }).ToList();
    }

    private static ExamAdministrationRules ToRules(RulesEntry? entry, string path, string where)
    {
        if (entry is null)
        {
            throw new ContentFormatException(path, $"{where} is missing.");
        }

        try
        {
            return new ExamAdministrationRules(entry.QuestionsAsked, entry.PassingAnswers, entry.FailingAnswers);
        }
        catch (ArgumentException ex)
        {
            throw new ContentFormatException(path, $"{where}: {ex.Message}", ex);
        }
    }

    private static IReadOnlyList<SourceReference> ToSources(List<SourceFile> sources, string path, string where) =>
        sources.Select((s, i) =>
        {
            var label = $"{where} sources[{i}]";
            return new SourceReference(
                Require(s.Authority, path, $"{label} authority"),
                Require(s.Title, path, $"{label} title"),
                ToUri(Require(s.Url, path, $"{label} url"), path, label),
                s.VerifiedOn,
                Require(s.License, path, $"{label} license"));
        }).ToList();

    private static ReviewStatus ToReviewStatus(string? value, string path, string where)
    {
        if (value is null)
        {
            throw new ContentFormatException(path, $"{where} has no reviewStatus.");
        }

        return ReviewStatuses.TryParse(value, out var status)
            ? status
            : throw new ContentFormatException(path, $"{where} reviewStatus '{value}' is not one of draft, needs-review, approved, outdated.");
    }

    private static DynamicAnswerScope ToScope(string? value, string path, string where) =>
        value?.ToLowerInvariant() switch
        {
            "federal" => DynamicAnswerScope.Federal,
            "state" => DynamicAnswerScope.State,
            "district" => DynamicAnswerScope.District,
            _ => throw new ContentFormatException(path, $"{where} scope '{value}' must be federal, state or district."),
        };

    private static Uri ToUri(string value, string path, string where) =>
        Uri.TryCreate(value, UriKind.Absolute, out var uri) && (uri.Scheme == Uri.UriSchemeHttps || uri.Scheme == Uri.UriSchemeHttp)
            ? uri
            : throw new ContentFormatException(path, $"{where} url '{value}' is not an absolute http(s) URL.");

    private static TimeSpan ToDuration(string value, string path, string where)
    {
        try
        {
            return XmlConvert.ToTimeSpan(value);
        }
        catch (FormatException ex)
        {
            throw new ContentFormatException(path, $"{where} checkEvery '{value}' is not an ISO 8601 duration such as P7D.", ex);
        }
    }

    private static string Require(string? value, string path, string field) =>
        string.IsNullOrWhiteSpace(value)
            ? throw new ContentFormatException(path, $"{field} is missing or empty.")
            : value.Trim();

    private static string? Optional(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static int Positive(int value, string path, string field) =>
        value > 0 ? value : throw new ContentFormatException(path, $"{field} must be positive, was {value.ToString(CultureInfo.InvariantCulture)}.");
}
