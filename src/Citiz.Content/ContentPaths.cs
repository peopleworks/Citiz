namespace Citiz.Content;

/// <summary>Where each content file lives, relative to the content root. One place to change if the layout does.</summary>
public static class ContentPaths
{
    /// <summary>The civics-test versions and their rules.</summary>
    public const string ExamVersions = "exams/versions.json";

    /// <summary>Answers that change with elections and appointments.</summary>
    public const string DynamicAnswers = "exams/dynamic-answers.json";

    /// <summary>The official reading vocabulary.</summary>
    public const string ReadingVocabulary = "english/reading-vocabulary.json";

    /// <summary>The official writing vocabulary.</summary>
    public const string WritingVocabulary = "english/writing-vocabulary.json";

    /// <summary>The discovery capsules.</summary>
    public const string DiscoveryTopics = "discovery/topics.json";

    /// <summary>The catalog of monitored official sources.</summary>
    public const string MonitoredSources = "sources/sources.json";

    /// <summary>The question bank for one exam version.</summary>
    public static string Questions(string versionId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(versionId);
        return $"exams/{versionId}/questions.json";
    }
}
