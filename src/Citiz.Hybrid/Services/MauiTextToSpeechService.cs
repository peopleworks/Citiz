using Citiz.SharedUI.Services;

namespace Citiz.Hybrid.Services;

/// <summary>
/// Text to speech through the platform engine, via .NET MAUI's <see cref="ITextToSpeech"/>:
/// Google Text-to-Speech (or the installed engine) on Android, AVSpeechSynthesizer on iOS and
/// macOS. Android's system WebView exposes no speech voices at all — the browser-based
/// <see cref="WebSpeechService"/> reports "cannot read text aloud" there, confirmed on the
/// Android 16 emulator on 2026-09-01 — so the native host speaks natively, which is exactly the
/// seam <see cref="ISpeechService"/> was carved out for. The platform engines synthesize on the
/// device with their installed voices, so <see cref="IsLocalVoiceAsync"/> answers <c>true</c>
/// whenever a voice for the language exists.
/// </summary>
public sealed class MauiTextToSpeechService(ITextToSpeech tts) : ISpeechService
{
    private IReadOnlyList<Microsoft.Maui.Media.Locale>? _locales;
    private CancellationTokenSource? _current;

    /// <summary>Whether the platform engine has at least one voice installed.</summary>
    public async ValueTask<bool> IsAvailableAsync() => (await LocalesAsync().ConfigureAwait(false)).Count > 0;

    /// <summary>Whether a voice for <paramref name="lang"/> is installed; platform voices run on the device.</summary>
    public async ValueTask<bool> IsLocalVoiceAsync(string lang) => Find(await LocalesAsync().ConfigureAwait(false), lang) is not null;

    /// <summary>
    /// Speaks <paramref name="text"/> in <paramref name="lang"/>. Returns as soon as speech starts,
    /// like the browser implementation, so a page's click handler does not wait for the sentence
    /// to end. <paramref name="rate"/> is on the Web Speech scale (1 is normal) and is mapped to the
    /// platform engine's scale by <see cref="PlatformRate"/>.
    /// </summary>
    public async ValueTask SpeakAsync(string text, string lang = "en-US", double rate = 0.9)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return;
        }

        await StopAsync().ConfigureAwait(false);
        var locale = Find(await LocalesAsync().ConfigureAwait(false), lang);
        var cts = new CancellationTokenSource();
        _current = cts;
        _ = SpeakInBackgroundAsync(text, locale, PlatformRate(rate), cts);
    }

    /// <summary>Stops the utterance in progress, if any.</summary>
    public ValueTask StopAsync()
    {
        var current = Interlocked.Exchange(ref _current, null);
        current?.Cancel();
        return ValueTask.CompletedTask;
    }

    private async Task SpeakInBackgroundAsync(string text, Microsoft.Maui.Media.Locale? locale, float rate, CancellationTokenSource cts)
    {
        try
        {
            await tts.SpeakAsync(text, new SpeechOptions { Locale = locale, Rate = rate }, cts.Token).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            // Stopped by StopAsync or superseded by the next utterance.
        }
        catch (Exception)
        {
            // A missing or misbehaving engine must never take the page down; the page already asked
            // IsAvailableAsync and disclosed the state to the learner.
        }
        finally
        {
            Interlocked.CompareExchange(ref _current, null, cts);
            cts.Dispose();
        }
    }

    /// <summary>
    /// MAUI documents <see cref="SpeechOptions.Rate"/> as 0.1–2.0 but hands the value straight to the
    /// platform. Android's engine takes 1 as normal, so the Web Speech value passes through; Apple's
    /// <c>AVSpeechUtterance.Rate</c> runs from 0 (slowest) to 1 (fastest) with 0.5 as normal, so the
    /// same 0.9 there is nearly the maximum — Pedro heard it on the iOS simulator on 2026-09-01 as
    /// "too fast to understand". Halving it gives the learner the slightly slow pace intended.
    /// </summary>
    private static float PlatformRate(double rate)
    {
#if IOS || MACCATALYST
        return (float)Math.Clamp(rate * 0.5, 0.05, 1.0);
#else
        return (float)Math.Clamp(rate, 0.1, 2.0);
#endif
    }

    private async Task<IReadOnlyList<Microsoft.Maui.Media.Locale>> LocalesAsync()
    {
        if (_locales is not null)
        {
            return _locales;
        }

        try
        {
            _locales = (await tts.GetLocalesAsync().ConfigureAwait(false)).ToList();
        }
        catch (Exception)
        {
            _locales = [];
        }

        return _locales;
    }

    private static Microsoft.Maui.Media.Locale? Find(IReadOnlyList<Microsoft.Maui.Media.Locale> locales, string lang)
    {
        var parts = lang.Split('-', 2);
        var language = parts[0];
        var country = parts.Length > 1 ? parts[1] : null;

        return locales.FirstOrDefault(l => Same(l.Language, language) && country is not null && Same(l.Country, country))
            ?? locales.FirstOrDefault(l => Same(l.Language, language));

        static bool Same(string? a, string? b) => string.Equals(a, b, StringComparison.OrdinalIgnoreCase);
    }
}
