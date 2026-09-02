using Citiz.Core.Audio;
using Microsoft.JSInterop;

namespace Citiz.SharedUI.Services;

/// <summary>
/// <see cref="IAudioPackStore"/> for the browser: packs live in Cache Storage (one cache per pack
/// version, see <c>citiz.audio</c> in <c>js/citiz.js</c>) and play through an <c>Audio</c> element,
/// so a downloaded pack works offline like the rest of the app.
/// </summary>
public sealed class BrowserAudioPackStore(IJSRuntime js) : IAudioPackStore
{
    /// <inheritdoc />
    public ValueTask<bool> IsSupportedAsync() => js.InvokeAsync<bool>("citiz.audio.supported");

    /// <inheritdoc />
    public async ValueTask<bool> IsReadyAsync(AudioPack pack)
    {
        ArgumentNullException.ThrowIfNull(pack);
        var state = await js.InvokeAsync<string>("citiz.audio.state", pack.CacheKey, Files(pack));
        return state == "ready";
    }

    /// <inheritdoc />
    public async Task<bool> DownloadAsync(AudioPack pack, IProgress<long>? progress, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(pack);
        using var callback = new ProgressCallback(progress);
        using var reference = DotNetObjectReference.Create(callback);
        using var registration = cancellationToken.Register(() => _ = js.InvokeVoidAsync("citiz.audio.cancel"));
        return await js.InvokeAsync<bool>("citiz.audio.download", pack.CacheKey, pack.BaseUrl.AbsoluteUri, Files(pack), pack.Clips.Select(c => c.Bytes).ToArray(), reference);
    }

    /// <inheritdoc />
    public ValueTask DeleteAsync(AudioPack pack)
    {
        ArgumentNullException.ThrowIfNull(pack);
        return js.InvokeVoidAsync("citiz.audio.remove", pack.CacheKey);
    }

    /// <inheritdoc />
    public ValueTask<bool> PlayAsync(AudioPack pack, AudioClip clip)
    {
        ArgumentNullException.ThrowIfNull(pack);
        ArgumentNullException.ThrowIfNull(clip);
        return js.InvokeAsync<bool>("citiz.audio.play", pack.CacheKey, clip.File);
    }

    /// <inheritdoc />
    public ValueTask StopAsync() => js.InvokeVoidAsync("citiz.audio.stop");

    private static string[] Files(AudioPack pack) => pack.Clips.Select(c => c.File).ToArray();

    /// <summary>Receives download progress from JavaScript.</summary>
    public sealed class ProgressCallback(IProgress<long>? progress) : IDisposable
    {
        /// <summary>Called by <c>citiz.audio.download</c> after each file.</summary>
        [JSInvokable]
        public void Report(long bytesDone) => progress?.Report(bytesDone);

        /// <inheritdoc />
        public void Dispose()
        {
        }
    }
}
