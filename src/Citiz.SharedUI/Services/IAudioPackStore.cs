using Citiz.Core.Audio;

namespace Citiz.SharedUI.Services;

/// <summary>Whether a pack's files are on this device.</summary>
public enum AudioPackStatus
{
    /// <summary>Not on the device; Listen uses the device voice.</summary>
    NotDownloaded,

    /// <summary>A download is in progress.</summary>
    Downloading,

    /// <summary>Every file is on the device; clips play offline.</summary>
    Ready,
}

/// <summary>
/// Where a host keeps downloaded audio packs and how it plays them. The browser keeps them in
/// Cache Storage and plays through an <c>Audio</c> element; the native hosts keep files in app data
/// and hand bytes to the same player. Either way the only network traffic is the download the
/// learner starts, from the pack's own host, and nothing about what is studied leaves the device.
/// </summary>
public interface IAudioPackStore
{
    /// <summary>Whether this host can keep packs at all (a browser without Cache Storage cannot).</summary>
    ValueTask<bool> IsSupportedAsync();

    /// <summary>Whether every file of <paramref name="pack"/> is on the device.</summary>
    ValueTask<bool> IsReadyAsync(AudioPack pack);

    /// <summary>Downloads every file of <paramref name="pack"/>, reporting bytes done; returns <c>false</c> when cancelled.</summary>
    Task<bool> DownloadAsync(AudioPack pack, IProgress<long>? progress, CancellationToken cancellationToken);

    /// <summary>Removes the pack's files from the device.</summary>
    ValueTask DeleteAsync(AudioPack pack);

    /// <summary>Plays one clip of a downloaded pack; <c>false</c> when the file is not on the device.</summary>
    ValueTask<bool> PlayAsync(AudioPack pack, AudioClip clip);

    /// <summary>Stops whatever clip is playing.</summary>
    ValueTask StopAsync();
}
