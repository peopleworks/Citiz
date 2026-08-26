namespace Citiz.Content.Sources;

/// <summary>
/// An official page or document the content worker polls for changes. When its hash changes, the
/// content files in <see cref="Feeds"/> are the ones a human needs to re-verify.
/// </summary>
/// <param name="Id">Stable slug.</param>
/// <param name="Authority">Publishing body.</param>
/// <param name="Title">Document title.</param>
/// <param name="Url">Where it lives.</param>
/// <param name="Format"><c>html</c>, <c>pdf</c>, <c>json</c> or <c>api</c>.</param>
/// <param name="CheckEvery">How often to poll.</param>
/// <param name="Monitor">Whether the worker polls it at all.</param>
/// <param name="RequiresHumanReview">Whether a detected change must go through editorial review (always true for official answers).</param>
/// <param name="Feeds">Content files, relative to the content root, that depend on this source.</param>
/// <param name="LastHash">SHA-256 of the normalized document at the last check, or <c>null</c>.</param>
/// <param name="LastCheckedOn">Date of the last check, or <c>null</c>.</param>
public sealed record MonitoredSource(
    string Id,
    string Authority,
    string Title,
    Uri Url,
    string Format,
    TimeSpan CheckEvery,
    bool Monitor,
    bool RequiresHumanReview,
    IReadOnlyList<string> Feeds,
    string? LastHash,
    DateOnly? LastCheckedOn);
