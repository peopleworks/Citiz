using Citiz.Core.Content;

namespace Citiz.Core.Audio;

/// <summary>Where a pack's recordings come from, which decides how the interface labels every clip.</summary>
public enum AudioPackKind
{
    /// <summary>Recorded and published by the authority itself (USCIS's MP3 tracks). Public domain.</summary>
    Official,

    /// <summary>Synthesized once from the verified text with a text-to-speech service. Never presented as official.</summary>
    Synthetic,
}

/// <summary>What a clip contains, which decides where it may be played.</summary>
public enum AudioClipRole
{
    /// <summary>An official track that reads the question <em>and</em> its answers; offered only once the answer is revealed.</summary>
    Recording,

    /// <summary>The question prompt alone, so it can back the "Listen" button in every practice mode.</summary>
    Prompt,

    /// <summary>One accepted answer of a question.</summary>
    Answer,

    /// <summary>One word of the reading or writing vocabulary.</summary>
    Word,
}

/// <summary>One audio file in a pack.</summary>
/// <param name="Id">Unique within the content, e.g. <c>r-2008-036</c>, <c>q-2025-097</c>, <c>a-2025-097-1</c>, <c>w-abraham-lincoln</c>.</param>
/// <param name="Role">What the clip contains.</param>
/// <param name="File">File name relative to the pack's <see cref="AudioPack.BaseUrl"/>.</param>
/// <param name="Bytes">Size on disk, for progress and totals.</param>
/// <param name="Seconds">Duration, shown next to the play button.</param>
/// <param name="Sha256">Hex digest of the file, so a download can be verified.</param>
/// <param name="QuestionId">The question this clip belongs to (<see cref="AudioClipRole.Recording"/>, <see cref="AudioClipRole.Prompt"/>, <see cref="AudioClipRole.Answer"/>).</param>
/// <param name="AnswerIndex">Zero-based index into the question's accepted answers (<see cref="AudioClipRole.Answer"/>).</param>
/// <param name="Word">The vocabulary word (<see cref="AudioClipRole.Word"/>), exactly as listed.</param>
public sealed record AudioClip(
    string Id,
    AudioClipRole Role,
    string File,
    long Bytes,
    double Seconds,
    string Sha256,
    string? QuestionId,
    int? AnswerIndex,
    string? Word);

/// <summary>
/// A set of recordings the learner can download once and keep on the device: the official USCIS
/// tracks for a test, or a synthetic voice generated from the verified text. Packs are listed in
/// <c>content/audio/packs.json</c>; the files themselves live at <see cref="BaseUrl"/>, so hosting can
/// move without a code change. The interface labels every clip with the pack's <see cref="Kind"/>.
/// </summary>
/// <param name="Id">Stable identifier, e.g. <c>uscis-2008</c>.</param>
/// <param name="Kind">Official or synthetic.</param>
/// <param name="Title">Shown in Settings, in English like the content it voices.</param>
/// <param name="Description">One line for Settings: what is inside.</param>
/// <param name="VersionId">The exam version the clips belong to, or <c>null</c> for vocabulary.</param>
/// <param name="Version">Increment when the files change; the device re-downloads.</param>
/// <param name="BaseUrl">Where the files are served from; every <see cref="AudioClip.File"/> is relative to it.</param>
/// <param name="SizeBytes">Total size, quoted before the download.</param>
/// <param name="License">Reuse terms of the audio.</param>
/// <param name="Voice">For synthetic packs, the service and voice used, e.g. <c>ElevenLabs · Sarah</c>.</param>
/// <param name="GeneratedOn">For synthetic packs, when the files were generated.</param>
/// <param name="ReviewStatus">Editorial state, like every other content entry.</param>
/// <param name="Sources">Where the recordings or the text they were generated from come from.</param>
/// <param name="Clips">Every file in the pack.</param>
public sealed record AudioPack(
    string Id,
    AudioPackKind Kind,
    string Title,
    string Description,
    string? VersionId,
    int Version,
    Uri BaseUrl,
    long SizeBytes,
    string License,
    string? Voice,
    DateOnly? GeneratedOn,
    ReviewStatus ReviewStatus,
    IReadOnlyList<SourceReference> Sources,
    IReadOnlyList<AudioClip> Clips)
{
    /// <summary>The key a device caches the pack under; changes with <see cref="Version"/>.</summary>
    public string CacheKey => $"{Id}-v{Version}";

    /// <summary>The official recording of a question (question and answers), if this pack has one.</summary>
    public AudioClip? RecordingFor(string questionId) => Find(AudioClipRole.Recording, c => c.QuestionId == questionId);

    /// <summary>The prompt-only clip of a question, if this pack has one.</summary>
    public AudioClip? PromptFor(string questionId) => Find(AudioClipRole.Prompt, c => c.QuestionId == questionId);

    /// <summary>The clip of one accepted answer, if this pack has one.</summary>
    public AudioClip? AnswerFor(string questionId, int answerIndex) => Find(AudioClipRole.Answer, c => c.QuestionId == questionId && c.AnswerIndex == answerIndex);

    /// <summary>The clip of a vocabulary word, if this pack has one.</summary>
    public AudioClip? WordFor(string word) => Find(AudioClipRole.Word, c => string.Equals(c.Word, word, StringComparison.OrdinalIgnoreCase));

    private AudioClip? Find(AudioClipRole role, Func<AudioClip, bool> predicate) => Clips.FirstOrDefault(c => c.Role == role && predicate(c));
}
