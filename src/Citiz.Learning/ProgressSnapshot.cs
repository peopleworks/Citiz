using System.Text.Json;
using System.Text.Json.Serialization;

namespace Citiz.Learning;

/// <summary>
/// The persisted form of a <see cref="ProgressLedger"/>. Versioned so a future change to
/// <see cref="ItemProgress"/> can migrate old browser data instead of discarding it. This is also
/// the export format: the learner can download it and take it elsewhere.
/// </summary>
/// <param name="Version">Schema version; currently <see cref="CurrentVersion"/>.</param>
/// <param name="Items">Progress per item.</param>
public sealed record ProgressSnapshot(int Version, IReadOnlyList<ItemProgress> Items)
{
    /// <summary>The schema version written by this build.</summary>
    public const int CurrentVersion = 1;

    /// <summary>An empty snapshot.</summary>
    public static ProgressSnapshot Empty { get; } = new(CurrentVersion, []);

    /// <summary>Serializes to JSON.</summary>
    public string ToJson() => JsonSerializer.Serialize(this, LearningJsonContext.Default.ProgressSnapshot);

    /// <summary>Parses JSON written by <see cref="ToJson"/>. Returns <see cref="Empty"/> for null, blank or unreadable input rather than throwing: losing progress is worse than starting fresh, and the caller logs it.</summary>
    public static ProgressSnapshot FromJson(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return Empty;
        }

        try
        {
            var snapshot = JsonSerializer.Deserialize(json, LearningJsonContext.Default.ProgressSnapshot);
            return snapshot is null || snapshot.Version > CurrentVersion ? Empty : snapshot;
        }
        catch (JsonException)
        {
            return Empty;
        }
    }
}

/// <summary>Source-generated serializer metadata for progress data.</summary>
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase, WriteIndented = true)]
[JsonSerializable(typeof(ProgressSnapshot))]
public sealed partial class LearningJsonContext : JsonSerializerContext
{
}
