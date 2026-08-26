namespace Citiz.SharedUI.Services;

/// <summary>
/// Text to speech. <see cref="WebSpeechService"/> uses the browser's Web Speech API; a future
/// Citiz.Hybrid host is expected to implement this with native platform text-to-speech, since
/// speech synthesis support inside a WebView is inconsistent across mobile platforms. Pages
/// depend on this interface, not on either implementation.
/// </summary>
public interface ISpeechService
{
    /// <summary>Whether the device can speak at all.</summary>
    ValueTask<bool> IsAvailableAsync();

    /// <summary>Whether the voice that would be used for <paramref name="lang"/> runs on the device.</summary>
    ValueTask<bool> IsLocalVoiceAsync(string lang);

    /// <summary>Speaks <paramref name="text"/> in <paramref name="lang"/> at <paramref name="rate"/> (1 is normal).</summary>
    ValueTask SpeakAsync(string text, string lang = "en-US", double rate = 0.9);

    /// <summary>Stops speaking.</summary>
    ValueTask StopAsync();
}
