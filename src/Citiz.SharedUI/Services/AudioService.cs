using Citiz.Content;
using Citiz.Core.Audio;
using Citiz.Core.Exams;

namespace Citiz.SharedUI.Services;

/// <summary>What the learner heard, so the interface can label it honestly.</summary>
public enum AudioSource
{
    /// <summary>Nothing could play.</summary>
    None,

    /// <summary>The device's own text-to-speech.</summary>
    DeviceVoice,

    /// <summary>A clip from a synthetic pack (generated once from the verified text; never official).</summary>
    SyntheticPack,

    /// <summary>A recording published by the authority itself.</summary>
    OfficialRecording,
}

/// <summary>A pack together with whether it is on this device.</summary>
/// <param name="Pack">The pack.</param>
/// <param name="Status">Downloaded or not.</param>
/// <param name="BytesDone">Progress while downloading.</param>
/// <param name="Error">Why the last download failed, if it did.</param>
public sealed record AudioPackState(AudioPack Pack, AudioPackStatus Status, long BytesDone = 0, string? Error = null);

/// <summary>
/// Decides what plays when the learner asks to hear something: a clip from a downloaded pack when
/// one exists, the device voice otherwise. Pages never touch packs directly; they ask this service
/// and show the badge for the <see cref="AudioSource"/> it reports. Downloads are started only from
/// Settings or the one-time offer, always by the learner.
/// </summary>
public sealed class AudioService(ContentRepository content, IAudioPackStore store, ISpeechService speech)
{
    private readonly Dictionary<string, AudioPackState> _states = new(StringComparer.Ordinal);
    private IReadOnlyList<AudioPack>? _packs;
    private bool _supported;

    /// <summary>Raised when a pack is downloaded or deleted, so open pages re-render their buttons.</summary>
    public event Action? Changed;

    /// <summary>Whether this host can keep packs at all.</summary>
    public bool IsSupported => _supported;

    /// <summary>Every pack and its state on this device. Loads the catalog on first call; no packs is a valid answer.</summary>
    public async Task<IReadOnlyList<AudioPackState>> GetStatesAsync()
    {
        if (_packs is null)
        {
            try
            {
                _packs = await content.GetAudioPacksAsync();
                _supported = await store.IsSupportedAsync();
            }
            catch (Exception ex) when (ex is HttpRequestException or ContentFormatException or FileNotFoundException)
            {
                _packs = [];
            }

            foreach (var pack in _packs)
            {
                var ready = _supported && await store.IsReadyAsync(pack);
                _states[pack.Id] = new AudioPackState(pack, ready ? AudioPackStatus.Ready : AudioPackStatus.NotDownloaded);
            }
        }

        return _packs.Select(p => _states[p.Id]).ToList();
    }

    /// <summary>The official recording of a question, when its pack is on the device.</summary>
    public async Task<(AudioPack Pack, AudioClip Clip)?> RecordingForAsync(CivicsQuestion question)
    {
        ArgumentNullException.ThrowIfNull(question);
        foreach (var state in await GetStatesAsync())
        {
            if (state.Status == AudioPackStatus.Ready && state.Pack.Kind == AudioPackKind.Official && state.Pack.RecordingFor(question.Id) is { } clip)
            {
                return (state.Pack, clip);
            }
        }

        return null;
    }

    /// <summary>Whether a synthetic answer clip exists on the device for an accepted answer.</summary>
    public async Task<bool> HasAnswerClipAsync(CivicsQuestion question, int answerIndex) =>
        await FindReadyAsync(p => p.Kind == AudioPackKind.Synthetic && p.AnswerFor(question.Id, answerIndex) is not null) is not null;

    /// <summary>Which source "Listen" would use for a question right now.</summary>
    public async Task<AudioSource> PromptSourceAsync(CivicsQuestion question)
    {
        ArgumentNullException.ThrowIfNull(question);
        if (await FindReadyAsync(p => p.Kind == AudioPackKind.Synthetic && p.PromptFor(question.Id) is not null) is not null)
        {
            return AudioSource.SyntheticPack;
        }

        return await speech.IsAvailableAsync() ? AudioSource.DeviceVoice : AudioSource.None;
    }

    /// <summary>Which source word chips would use right now.</summary>
    public async Task<AudioSource> WordSourceAsync()
    {
        if (await FindReadyAsync(p => p.Kind == AudioPackKind.Synthetic && p.Clips.Any(c => c.Role == AudioClipRole.Word)) is not null)
        {
            return AudioSource.SyntheticPack;
        }

        return await speech.IsAvailableAsync() ? AudioSource.DeviceVoice : AudioSource.None;
    }

    /// <summary>Reads the question prompt: the synthetic clip if downloaded, else the device voice.</summary>
    public Task<AudioSource> PlayPromptAsync(CivicsQuestion question)
    {
        ArgumentNullException.ThrowIfNull(question);
        return PlayOrSpeakAsync(p => p.PromptFor(question.Id), question.Prompt);
    }

    /// <summary>Reads one accepted answer: the synthetic clip if downloaded, else the device voice.</summary>
    public Task<AudioSource> PlayAnswerAsync(CivicsQuestion question, int answerIndex, string text)
    {
        ArgumentNullException.ThrowIfNull(question);
        return PlayOrSpeakAsync(p => p.AnswerFor(question.Id, answerIndex), text);
    }

    /// <summary>Reads a vocabulary word: the synthetic clip if downloaded, else the device voice.</summary>
    public Task<AudioSource> PlayWordAsync(string word) => PlayOrSpeakAsync(p => p.WordFor(word), word);

    /// <summary>Plays the official recording of a question (question and answers). Never falls back: there is no substitute for the real thing.</summary>
    public async Task<AudioSource> PlayRecordingAsync(CivicsQuestion question)
    {
        if (await RecordingForAsync(question) is { } found && await store.PlayAsync(found.Pack, found.Clip))
        {
            return AudioSource.OfficialRecording;
        }

        return AudioSource.None;
    }

    /// <summary>Stops any clip and any speech.</summary>
    public async Task StopAsync()
    {
        await store.StopAsync();
        await speech.StopAsync();
    }

    /// <summary>Downloads a pack, reporting progress through <see cref="GetStatesAsync"/> and <see cref="Changed"/>.</summary>
    public async Task DownloadAsync(string packId, CancellationToken cancellationToken = default)
    {
        await GetStatesAsync();
        if (!_states.TryGetValue(packId, out var state) || state.Status != AudioPackStatus.NotDownloaded)
        {
            return;
        }

        Update(state with { Status = AudioPackStatus.Downloading, BytesDone = 0, Error = null });
        var progress = new Progress<long>(done => Update(_states[packId] with { BytesDone = done }));

        try
        {
            var completed = await store.DownloadAsync(state.Pack, progress, cancellationToken);
            Update(state with { Status = completed && await store.IsReadyAsync(state.Pack) ? AudioPackStatus.Ready : AudioPackStatus.NotDownloaded });
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            Update(state with { Status = AudioPackStatus.NotDownloaded, Error = ex.Message });
        }
    }

    /// <summary>Removes a pack from the device.</summary>
    public async Task DeleteAsync(string packId)
    {
        await GetStatesAsync();
        if (!_states.TryGetValue(packId, out var state))
        {
            return;
        }

        await store.StopAsync();
        await store.DeleteAsync(state.Pack);
        Update(state with { Status = AudioPackStatus.NotDownloaded, BytesDone = 0, Error = null });
    }

    private async Task<AudioSource> PlayOrSpeakAsync(Func<AudioPack, AudioClip?> clipOf, string text)
    {
        foreach (var state in await GetStatesAsync())
        {
            if (state.Status == AudioPackStatus.Ready && state.Pack.Kind == AudioPackKind.Synthetic && clipOf(state.Pack) is { } clip)
            {
                if (await store.PlayAsync(state.Pack, clip))
                {
                    return AudioSource.SyntheticPack;
                }
            }
        }

        if (!await speech.IsAvailableAsync())
        {
            return AudioSource.None;
        }

        await store.StopAsync();
        await speech.SpeakAsync(text);
        return AudioSource.DeviceVoice;
    }

    private async Task<AudioPack?> FindReadyAsync(Func<AudioPack, bool> predicate) =>
        (await GetStatesAsync()).FirstOrDefault(s => s.Status == AudioPackStatus.Ready && predicate(s.Pack))?.Pack;

    private void Update(AudioPackState state)
    {
        _states[state.Pack.Id] = state;
        Changed?.Invoke();
    }
}
