using System.Text.Json;
using System.Text.Json.Serialization;

namespace Citiz.Content.Files;

/// <summary>
/// Source-generated serializer metadata for the content files. Source generation keeps the browser
/// build trim-safe and avoids reflection at startup, which matters on the low-end phones the design
/// targets.
/// </summary>
[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    PropertyNameCaseInsensitive = true,
    ReadCommentHandling = JsonCommentHandling.Skip,
    AllowTrailingCommas = true,
    WriteIndented = true)]
[JsonSerializable(typeof(ExamVersionsFile))]
[JsonSerializable(typeof(QuestionsFile))]
[JsonSerializable(typeof(DynamicAnswersFile))]
[JsonSerializable(typeof(VocabularyFile))]
[JsonSerializable(typeof(DiscoveryTopicsFile))]
[JsonSerializable(typeof(SourcesFile))]
[JsonSerializable(typeof(AudioPacksFile))]
public sealed partial class ContentJsonContext : JsonSerializerContext
{
}
