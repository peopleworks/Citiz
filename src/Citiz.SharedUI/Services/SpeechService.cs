using Microsoft.JSInterop;

namespace Citiz.SharedUI.Services;

/// <summary>
/// Text to speech through the browser's Web Speech API. Most browsers synthesize on the device;
/// some offer network voices, so <see cref="IsLocalVoiceAsync"/> lets the interface disclose which
/// one it is using, as Docs/Privacy/LOCAL_VS_CLOUD.md requires.
/// </summary>
public sealed class SpeechService(IJSRuntime js)
{
    /// <summary>Whether the browser can speak at all.</summary>
    public ValueTask<bool> IsAvailableAsync() => js.InvokeAsync<bool>("citiz.speech.available");

    /// <summary>Whether the voice that would be used for <paramref name="lang"/> runs on the device.</summary>
    public ValueTask<bool> IsLocalVoiceAsync(string lang) => js.InvokeAsync<bool>("citiz.speech.isLocal", lang);

    /// <summary>Speaks <paramref name="text"/> in <paramref name="lang"/> at <paramref name="rate"/> (1 is normal).</summary>
    public ValueTask SpeakAsync(string text, string lang = "en-US", double rate = 0.9) => js.InvokeVoidAsync("citiz.speech.speak", text, lang, rate);

    /// <summary>Stops speaking.</summary>
    public ValueTask StopAsync() => js.InvokeVoidAsync("citiz.speech.stop");
}
