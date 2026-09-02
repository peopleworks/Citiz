using System.Text.Json.Serialization;

namespace Citiz.Content.Files;

// The on-disk shape of every content file, as plain DTOs. They deliberately mirror the JSON and
// nothing else: ContentMapper turns them into domain objects and reports what is missing or
// malformed. Unknown properties are rejected so a typo like "acceptedAnswer" fails validation
// instead of silently dropping the answers.

/// <summary>A source citation as written in a content file.</summary>
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class SourceFile
{
    /// <inheritdoc cref="Core.Content.SourceReference.Authority"/>
    public string? Authority { get; set; }

    /// <inheritdoc cref="Core.Content.SourceReference.Title"/>
    public string? Title { get; set; }

    /// <inheritdoc cref="Core.Content.SourceReference.Url"/>
    public string? Url { get; set; }

    /// <inheritdoc cref="Core.Content.SourceReference.VerifiedOn"/>
    public DateOnly? VerifiedOn { get; set; }

    /// <inheritdoc cref="Core.Content.SourceReference.License"/>
    public string? License { get; set; }
}

/// <summary><c>exams/versions.json</c>.</summary>
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class ExamVersionsFile
{
    /// <summary>The JSON Schema reference, ignored.</summary>
    [JsonPropertyName("$schema")]
    public string? Schema { get; set; }

    /// <summary>The versions.</summary>
    public List<ExamVersionEntry> Versions { get; set; } = [];
}

/// <summary>One entry in <see cref="ExamVersionsFile"/>.</summary>
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class ExamVersionEntry
{
    /// <inheritdoc cref="Core.Exams.ExamVersion.Id"/>
    public string? Id { get; set; }

    /// <inheritdoc cref="Core.Exams.ExamVersion.DisplayName"/>
    public string? DisplayName { get; set; }

    /// <inheritdoc cref="Core.Exams.ExamVersion.FilingFrom"/>
    public DateOnly? FilingFrom { get; set; }

    /// <inheritdoc cref="Core.Exams.ExamVersion.FilingTo"/>
    public DateOnly? FilingTo { get; set; }

    /// <inheritdoc cref="Core.Exams.ExamVersion.BankSize"/>
    public int BankSize { get; set; }

    /// <inheritdoc cref="Core.Exams.ExamVersion.Standard"/>
    public RulesEntry? Standard { get; set; }

    /// <inheritdoc cref="Core.Exams.ExamVersion.SeniorConsideration"/>
    public RulesEntry? SeniorConsideration { get; set; }

    /// <inheritdoc cref="Core.Exams.ExamVersion.SeniorQuestionNumbers"/>
    public List<int> SeniorQuestionNumbers { get; set; } = [];

    /// <summary>Kebab-case review status.</summary>
    public string? ReviewStatus { get; set; }

    /// <summary>Sources.</summary>
    public List<SourceFile> Sources { get; set; } = [];
}

/// <summary>Administration rules as written in a content file.</summary>
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class RulesEntry
{
    /// <inheritdoc cref="Core.Exams.ExamAdministrationRules.QuestionsAsked"/>
    public int QuestionsAsked { get; set; }

    /// <inheritdoc cref="Core.Exams.ExamAdministrationRules.PassingAnswers"/>
    public int PassingAnswers { get; set; }

    /// <inheritdoc cref="Core.Exams.ExamAdministrationRules.FailingAnswers"/>
    public int FailingAnswers { get; set; }
}

/// <summary><c>exams/&lt;version&gt;/questions.json</c>.</summary>
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class QuestionsFile
{
    /// <summary>The JSON Schema reference, ignored.</summary>
    [JsonPropertyName("$schema")]
    public string? Schema { get; set; }

    /// <summary>The version the bank belongs to.</summary>
    public string? VersionId { get; set; }

    /// <summary>Default review status for every question without its own.</summary>
    public string? ReviewStatus { get; set; }

    /// <summary>Sources.</summary>
    public List<SourceFile> Sources { get; set; } = [];

    /// <summary>The questions.</summary>
    public List<QuestionEntry> Questions { get; set; } = [];
}

/// <summary>One entry in <see cref="QuestionsFile"/>.</summary>
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class QuestionEntry
{
    /// <inheritdoc cref="Core.Exams.CivicsQuestion.Id"/>
    public string? Id { get; set; }

    /// <inheritdoc cref="Core.Exams.CivicsQuestion.Number"/>
    public int Number { get; set; }

    /// <inheritdoc cref="Core.Exams.CivicsQuestion.Category"/>
    public string? Category { get; set; }

    /// <inheritdoc cref="Core.Exams.CivicsQuestion.Subcategory"/>
    public string? Subcategory { get; set; }

    /// <inheritdoc cref="Core.Exams.CivicsQuestion.Prompt"/>
    public string? Prompt { get; set; }

    /// <inheritdoc cref="Core.Exams.CivicsQuestion.AcceptedAnswers"/>
    public List<string> AcceptedAnswers { get; set; } = [];

    /// <inheritdoc cref="Core.Exams.CivicsQuestion.DynamicAnswerKey"/>
    public string? DynamicAnswerKey { get; set; }

    /// <inheritdoc cref="Core.Exams.CivicsQuestion.Note"/>
    public string? Note { get; set; }

    /// <summary>Kebab-case review status; inherits the file's when absent.</summary>
    public string? ReviewStatus { get; set; }
}

/// <summary><c>exams/dynamic-answers.json</c>.</summary>
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class DynamicAnswersFile
{
    /// <summary>The JSON Schema reference, ignored.</summary>
    [JsonPropertyName("$schema")]
    public string? Schema { get; set; }

    /// <summary>The entries.</summary>
    public List<DynamicAnswerEntry> Answers { get; set; } = [];
}

/// <summary>One entry in <see cref="DynamicAnswersFile"/>.</summary>
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class DynamicAnswerEntry
{
    /// <inheritdoc cref="Core.Exams.DynamicAnswer.Key"/>
    public string? Key { get; set; }

    /// <inheritdoc cref="Core.Exams.DynamicAnswer.Office"/>
    public string? Office { get; set; }

    /// <summary><c>federal</c>, <c>state</c> or <c>district</c>.</summary>
    public string? Scope { get; set; }

    /// <inheritdoc cref="Core.Exams.DynamicAnswer.Holder"/>
    public string? Holder { get; set; }

    /// <inheritdoc cref="Core.Exams.DynamicAnswer.AcceptedAnswers"/>
    public List<string> AcceptedAnswers { get; set; } = [];

    /// <inheritdoc cref="Core.Exams.DynamicAnswer.Since"/>
    public DateOnly? Since { get; set; }

    /// <inheritdoc cref="Core.Exams.DynamicAnswer.VerifiedOn"/>
    public DateOnly? VerifiedOn { get; set; }

    /// <inheritdoc cref="Core.Exams.DynamicAnswer.LookupHint"/>
    public string? LookupHint { get; set; }

    /// <summary>Kebab-case review status.</summary>
    public string? ReviewStatus { get; set; }

    /// <summary>Sources.</summary>
    public List<SourceFile> Sources { get; set; } = [];
}

/// <summary><c>english/reading-vocabulary.json</c> and <c>english/writing-vocabulary.json</c>.</summary>
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class VocabularyFile
{
    /// <summary>The JSON Schema reference, ignored.</summary>
    [JsonPropertyName("$schema")]
    public string? Schema { get; set; }

    /// <summary><c>reading</c> or <c>writing</c>.</summary>
    public string? Kind { get; set; }

    /// <summary>Kebab-case review status.</summary>
    public string? ReviewStatus { get; set; }

    /// <summary>Sources.</summary>
    public List<SourceFile> Sources { get; set; } = [];

    /// <summary>The word groups.</summary>
    public List<VocabularyGroupEntry> Groups { get; set; } = [];
}

/// <summary>One group in <see cref="VocabularyFile"/>.</summary>
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class VocabularyGroupEntry
{
    /// <inheritdoc cref="Core.English.VocabularyGroup.Category"/>
    public string? Category { get; set; }

    /// <inheritdoc cref="Core.English.VocabularyGroup.Words"/>
    public List<string> Words { get; set; } = [];
}

/// <summary><c>discovery/topics.json</c>.</summary>
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class DiscoveryTopicsFile
{
    /// <summary>The JSON Schema reference, ignored.</summary>
    [JsonPropertyName("$schema")]
    public string? Schema { get; set; }

    /// <summary>The capsules.</summary>
    public List<DiscoveryTopicEntry> Topics { get; set; } = [];
}

/// <summary>One entry in <see cref="DiscoveryTopicsFile"/>.</summary>
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class DiscoveryTopicEntry
{
    /// <inheritdoc cref="Core.Discovery.DiscoveryTopic.Id"/>
    public string? Id { get; set; }

    /// <inheritdoc cref="Core.Discovery.DiscoveryTopic.Category"/>
    public string? Category { get; set; }

    /// <inheritdoc cref="Core.Discovery.DiscoveryTopic.Title"/>
    public string? Title { get; set; }

    /// <inheritdoc cref="Core.Discovery.DiscoveryTopic.Summary"/>
    public string? Summary { get; set; }

    /// <inheritdoc cref="Core.Discovery.DiscoveryTopic.SimpleEnglish"/>
    public string? SimpleEnglish { get; set; }

    /// <inheritdoc cref="Core.Discovery.DiscoveryTopic.EstimatedMinutes"/>
    public int EstimatedMinutes { get; set; }

    /// <inheritdoc cref="Core.Discovery.DiscoveryTopic.Difficulty"/>
    public string? Difficulty { get; set; }

    /// <inheritdoc cref="Core.Discovery.DiscoveryTopic.Vocabulary"/>
    public List<string> Vocabulary { get; set; } = [];

    /// <inheritdoc cref="Core.Discovery.DiscoveryTopic.RelatedQuestionIds"/>
    public List<string> RelatedQuestionIds { get; set; } = [];

    /// <inheritdoc cref="Core.Discovery.DiscoveryTopic.RelatedPlaces"/>
    public List<string> RelatedPlaces { get; set; } = [];

    /// <summary>Kebab-case review status.</summary>
    public string? ReviewStatus { get; set; }

    /// <summary>Sources.</summary>
    public List<SourceFile> Sources { get; set; } = [];
}

/// <summary><c>sources/sources.json</c>.</summary>
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class SourcesFile
{
    /// <summary>The JSON Schema reference, ignored.</summary>
    [JsonPropertyName("$schema")]
    public string? Schema { get; set; }

    /// <summary>The monitored sources.</summary>
    public List<MonitoredSourceEntry> Sources { get; set; } = [];
}

/// <summary>One entry in <see cref="SourcesFile"/>.</summary>
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class MonitoredSourceEntry
{
    /// <inheritdoc cref="Sources.MonitoredSource.Id"/>
    public string? Id { get; set; }

    /// <inheritdoc cref="Sources.MonitoredSource.Authority"/>
    public string? Authority { get; set; }

    /// <inheritdoc cref="Sources.MonitoredSource.Title"/>
    public string? Title { get; set; }

    /// <inheritdoc cref="Sources.MonitoredSource.Url"/>
    public string? Url { get; set; }

    /// <inheritdoc cref="Sources.MonitoredSource.Format"/>
    public string? Format { get; set; }

    /// <summary>ISO 8601 duration, e.g. <c>P7D</c>.</summary>
    public string? CheckEvery { get; set; }

    /// <inheritdoc cref="Sources.MonitoredSource.Monitor"/>
    public bool Monitor { get; set; }

    /// <inheritdoc cref="Sources.MonitoredSource.RequiresHumanReview"/>
    public bool RequiresHumanReview { get; set; }

    /// <inheritdoc cref="Sources.MonitoredSource.Feeds"/>
    public List<string> Feeds { get; set; } = [];

    /// <inheritdoc cref="Sources.MonitoredSource.LastHash"/>
    public string? LastHash { get; set; }

    /// <inheritdoc cref="Sources.MonitoredSource.LastCheckedOn"/>
    public DateOnly? LastCheckedOn { get; set; }
}

/// <summary><c>audio/packs.json</c>.</summary>
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class AudioPacksFile
{
    /// <summary>The JSON Schema reference, ignored.</summary>
    [JsonPropertyName("$schema")]
    public string? Schema { get; set; }

    /// <summary>The packs.</summary>
    public List<AudioPackEntry> Packs { get; set; } = [];
}

/// <summary>One pack in <see cref="AudioPacksFile"/>.</summary>
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class AudioPackEntry
{
    /// <inheritdoc cref="Core.Audio.AudioPack.Id"/>
    public string? Id { get; set; }

    /// <summary><c>official</c> or <c>synthetic</c>.</summary>
    public string? Kind { get; set; }

    /// <inheritdoc cref="Core.Audio.AudioPack.Title"/>
    public string? Title { get; set; }

    /// <inheritdoc cref="Core.Audio.AudioPack.Description"/>
    public string? Description { get; set; }

    /// <inheritdoc cref="Core.Audio.AudioPack.VersionId"/>
    public string? VersionId { get; set; }

    /// <inheritdoc cref="Core.Audio.AudioPack.Version"/>
    public int Version { get; set; }

    /// <inheritdoc cref="Core.Audio.AudioPack.BaseUrl"/>
    public string? BaseUrl { get; set; }

    /// <inheritdoc cref="Core.Audio.AudioPack.SizeBytes"/>
    public long SizeBytes { get; set; }

    /// <inheritdoc cref="Core.Audio.AudioPack.License"/>
    public string? License { get; set; }

    /// <inheritdoc cref="Core.Audio.AudioPack.Voice"/>
    public string? Voice { get; set; }

    /// <inheritdoc cref="Core.Audio.AudioPack.GeneratedOn"/>
    public DateOnly? GeneratedOn { get; set; }

    /// <summary>Kebab-case review status.</summary>
    public string? ReviewStatus { get; set; }

    /// <summary>Sources.</summary>
    public List<SourceFile> Sources { get; set; } = [];

    /// <summary>The clips.</summary>
    public List<AudioClipEntry> Clips { get; set; } = [];
}

/// <summary>One clip in <see cref="AudioPackEntry"/>.</summary>
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class AudioClipEntry
{
    /// <inheritdoc cref="Core.Audio.AudioClip.Id"/>
    public string? Id { get; set; }

    /// <summary><c>recording</c>, <c>prompt</c>, <c>answer</c> or <c>word</c>.</summary>
    public string? Role { get; set; }

    /// <inheritdoc cref="Core.Audio.AudioClip.File"/>
    public string? File { get; set; }

    /// <inheritdoc cref="Core.Audio.AudioClip.Bytes"/>
    public long Bytes { get; set; }

    /// <inheritdoc cref="Core.Audio.AudioClip.Seconds"/>
    public double Seconds { get; set; }

    /// <inheritdoc cref="Core.Audio.AudioClip.Sha256"/>
    public string? Sha256 { get; set; }

    /// <inheritdoc cref="Core.Audio.AudioClip.QuestionId"/>
    public string? QuestionId { get; set; }

    /// <inheritdoc cref="Core.Audio.AudioClip.AnswerIndex"/>
    public int? AnswerIndex { get; set; }

    /// <inheritdoc cref="Core.Audio.AudioClip.Word"/>
    public string? Word { get; set; }
}
