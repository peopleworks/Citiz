using System.Security.Cryptography;
using Citiz.Core.Audio;
using Citiz.SharedUI.Services;
using Microsoft.JSInterop;

namespace Citiz.Hybrid.Services;

/// <summary>
/// <see cref="IAudioPackStore"/> for the native hosts: packs are downloaded into the app's data
/// directory (one folder per pack version, a marker file once complete) and played by handing the
/// bytes to the WebView's audio player, since a <c>BlazorWebView</c> serves only the app's own
/// static files. Downloaded files are checked against the manifest's SHA-256 before the pack counts
/// as ready, so a truncated or tampered file is never played.
/// </summary>
public sealed class AppDataAudioPackStore : IAudioPackStore, IDisposable
{
    private const string CompleteMarker = ".complete";
    private readonly HttpClient _http = new() { Timeout = TimeSpan.FromMinutes(10) };
    private readonly IJSRuntime _js;
    private readonly string _root;

    /// <summary>Creates the store over the app data directory.</summary>
    public AppDataAudioPackStore(IJSRuntime js)
    {
        _js = js;
        _root = Path.Combine(FileSystem.AppDataDirectory, "audio");
    }

    /// <inheritdoc />
    public ValueTask<bool> IsSupportedAsync() => new(true);

    /// <inheritdoc />
    public ValueTask<bool> IsReadyAsync(AudioPack pack)
    {
        ArgumentNullException.ThrowIfNull(pack);
        return new(File.Exists(Path.Combine(Folder(pack), CompleteMarker)));
    }

    /// <inheritdoc />
    public async Task<bool> DownloadAsync(AudioPack pack, IProgress<long>? progress, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(pack);
        var folder = Folder(pack);
        Directory.CreateDirectory(folder);
        long done = 0;

        foreach (var clip in pack.Clips)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var target = Path.Combine(folder, clip.File);
            if (!File.Exists(target) || !await MatchesAsync(target, clip.Sha256, cancellationToken).ConfigureAwait(false))
            {
                var bytes = await _http.GetByteArrayAsync(new Uri(pack.BaseUrl, clip.File), cancellationToken).ConfigureAwait(false);
                if (!string.Equals(Convert.ToHexStringLower(SHA256.HashData(bytes)), clip.Sha256, StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidDataException($"{clip.File} did not match its digest.");
                }

                await File.WriteAllBytesAsync(target, bytes, cancellationToken).ConfigureAwait(false);
            }

            done += clip.Bytes;
            progress?.Report(done);
        }

        await File.WriteAllTextAsync(Path.Combine(folder, CompleteMarker), pack.Version.ToString(System.Globalization.CultureInfo.InvariantCulture), cancellationToken).ConfigureAwait(false);
        return true;
    }

    /// <inheritdoc />
    public ValueTask DeleteAsync(AudioPack pack)
    {
        ArgumentNullException.ThrowIfNull(pack);
        var folder = Folder(pack);
        if (Directory.Exists(folder))
        {
            Directory.Delete(folder, recursive: true);
        }

        return default;
    }

    /// <inheritdoc />
    public async ValueTask<bool> PlayAsync(AudioPack pack, AudioClip clip)
    {
        ArgumentNullException.ThrowIfNull(pack);
        ArgumentNullException.ThrowIfNull(clip);
        var path = Path.Combine(Folder(pack), clip.File);
        if (!File.Exists(path))
        {
            return false;
        }

        var bytes = await File.ReadAllBytesAsync(path).ConfigureAwait(false);
        return await _js.InvokeAsync<bool>("citiz.audio.playBase64", Convert.ToBase64String(bytes), "audio/mpeg");
    }

    /// <inheritdoc />
    public ValueTask StopAsync() => _js.InvokeVoidAsync("citiz.audio.stop");

    /// <inheritdoc />
    public void Dispose() => _http.Dispose();

    private string Folder(AudioPack pack) => Path.Combine(_root, pack.CacheKey);

    private static async Task<bool> MatchesAsync(string path, string sha256, CancellationToken cancellationToken)
    {
        await using var stream = File.OpenRead(path);
        var digest = await SHA256.HashDataAsync(stream, cancellationToken).ConfigureAwait(false);
        return string.Equals(Convert.ToHexStringLower(digest), sha256, StringComparison.OrdinalIgnoreCase);
    }
}
