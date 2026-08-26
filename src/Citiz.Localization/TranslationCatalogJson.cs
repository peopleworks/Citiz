using System.Text.Json;
using System.Text.Json.Serialization;

namespace Citiz.Localization;

/// <summary>Reads <c>i18n/&lt;code&gt;.json</c> files: a flat JSON object of key to text.</summary>
public static class TranslationCatalogJson
{
    /// <summary>Parses a catalog from JSON text.</summary>
    /// <exception cref="JsonException">The text is not a flat object of strings.</exception>
    public static TranslationCatalog Parse(string culture, string json)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(culture);
        ArgumentNullException.ThrowIfNull(json);

        var values = JsonSerializer.Deserialize(json, LocalizationJsonContext.Default.DictionaryStringString)
            ?? throw new JsonException($"Language pack '{culture}' is empty.");
        return new TranslationCatalog(culture, values);
    }

    /// <summary>Parses a catalog from a stream.</summary>
    /// <exception cref="JsonException">The content is not a flat object of strings.</exception>
    public static async Task<TranslationCatalog> ParseAsync(string culture, Stream stream, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(culture);
        ArgumentNullException.ThrowIfNull(stream);

        var values = await JsonSerializer.DeserializeAsync(stream, LocalizationJsonContext.Default.DictionaryStringString, cancellationToken).ConfigureAwait(false)
            ?? throw new JsonException($"Language pack '{culture}' is empty.");
        return new TranslationCatalog(culture, values);
    }
}

/// <summary>Source-generated serializer metadata for language packs.</summary>
[JsonSourceGenerationOptions(ReadCommentHandling = JsonCommentHandling.Skip, AllowTrailingCommas = true)]
[JsonSerializable(typeof(Dictionary<string, string>))]
public sealed partial class LocalizationJsonContext : JsonSerializerContext
{
}
