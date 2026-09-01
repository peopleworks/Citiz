#if ANDROID
using Android.Speech.Tts;
using Citiz.SharedUI.Services;
using AndroidTextToSpeech = Android.Speech.Tts.TextToSpeech;

namespace Citiz.Hybrid.Services;

/// <summary>
/// Text to speech through Android's <see cref="Android.Speech.Tts.TextToSpeech"/> engine (Google
/// Text-to-Speech on most devices), choosing the best voice installed for the language that
/// synthesizes on the device: voices carry a <see cref="Voice.Quality"/> ("higher is better") and
/// a network flag, and the engine's default is not always the best local one. Android's system
/// WebView exposes no speech voices at all, so the browser implementation is unusable here, and
/// MAUI's TextToSpeech can pick a language but not a voice — the two reasons this class exists.
/// Learners get better voices by installing voice data in Settings → System → Languages → Text-to-
/// speech output; this service picks them up on the next launch.
/// </summary>
public sealed class AndroidSpeechService : Java.Lang.Object, ISpeechService, AndroidTextToSpeech.IOnInitListener
{
    private readonly TaskCompletionSource<bool> _ready = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private readonly AndroidTextToSpeech _tts;
    private readonly Dictionary<string, bool> _voiceSet = new(StringComparer.OrdinalIgnoreCase);
    private int _utterance;

    public AndroidSpeechService()
    {
        _tts = new AndroidTextToSpeech(Android.App.Application.Context, this);
    }

    /// <inheritdoc />
    public void OnInit(OperationResult status)
    {
        Android.Util.Log.Info("Citiz", $"Text-to-speech engine {_tts.DefaultEngine}: {status}");
        _ready.TrySetResult(status == OperationResult.Success);
    }

    /// <summary>Whether the engine initialised and has any voice.</summary>
    public async ValueTask<bool> IsAvailableAsync()
    {
        if (!await _ready.Task.ConfigureAwait(false))
        {
            return false;
        }

        try
        {
            return _tts.Voices?.Count > 0 || _tts.IsLanguageAvailable(Java.Util.Locale.Us) >= LanguageAvailableResult.Available;
        }
        catch (Exception)
        {
            return false;
        }
    }

    /// <summary>Whether a voice for <paramref name="lang"/> that does not need the network is installed.</summary>
    public async ValueTask<bool> IsLocalVoiceAsync(string lang) => await _ready.Task.ConfigureAwait(false) && await EnsureVoiceAsync(lang).ConfigureAwait(false);

    /// <summary>Speaks <paramref name="text"/>; <paramref name="rate"/> is on the Web Speech scale (1 is normal), which is Android's too.</summary>
    public async ValueTask SpeakAsync(string text, string lang = "en-US", double rate = 0.9)
    {
        if (string.IsNullOrWhiteSpace(text) || !await _ready.Task.ConfigureAwait(false))
        {
            return;
        }

        await EnsureVoiceAsync(lang).ConfigureAwait(false);
        _tts.SetSpeechRate((float)Math.Clamp(rate, 0.1, 2.0));
        _tts.Speak(text, QueueMode.Flush, null, $"citiz-{Interlocked.Increment(ref _utterance)}");
    }

    /// <summary>Stops the utterance in progress, if any.</summary>
    public async ValueTask StopAsync()
    {
        if (await _ready.Task.ConfigureAwait(false))
        {
            _tts.Stop();
        }
    }

    /// <summary>Selects the best local voice for <paramref name="lang"/> once; returns whether one exists.</summary>
    private Task<bool> EnsureVoiceAsync(string lang)
    {
        lock (_voiceSet)
        {
            if (_voiceSet.TryGetValue(lang, out var known))
            {
                return Task.FromResult(known);
            }

            var found = ChooseVoice(lang);
            _voiceSet[lang] = found;
            return Task.FromResult(found);
        }
    }

    private bool ChooseVoice(string lang)
    {
        var parts = lang.Split('-', 2);
        var language = parts[0];
        var country = parts.Length > 1 ? parts[1] : null;

        try
        {
            var voices = _tts.Voices?
                .Where(v => v.Locale is not null
                    && string.Equals(v.Locale.Language, language, StringComparison.OrdinalIgnoreCase)
                    && !v.IsNetworkConnectionRequired
                    && !(v.Features?.Contains(AndroidTextToSpeech.Engine.KeyFeatureNotInstalled) ?? false))
                .ToList() ?? [];

            // Region match, then quality, then the engine's own default (the voice Google tuned as the
            // sensible one for the language) ahead of an arbitrary name, then latency.
            var defaultName = _tts.DefaultVoice?.Name;
            var best = voices
                .OrderByDescending(v => country is not null && string.Equals(v.Locale!.Country, country, StringComparison.OrdinalIgnoreCase))
                .ThenByDescending(v => v.Quality)
                .ThenByDescending(v => string.Equals(v.Name, defaultName, StringComparison.Ordinal))
                .ThenBy(v => v.Latency)
                .ThenBy(v => v.Name, StringComparer.Ordinal)
                .FirstOrDefault();

            Android.Util.Log.Info("Citiz", $"{voices.Count} local voices for {language}: " + string.Join(", ", voices.Select(v => $"{v.Name} q={v.Quality} l={v.Latency}")));

            if (best is not null && _tts.SetVoice(best) == OperationResult.Success)
            {
                Android.Util.Log.Info("Citiz", $"Voice for {lang}: {best.Name} (quality {best.Quality}, latency {best.Latency})");
                return true;
            }
        }
        catch (Exception ex)
        {
            Android.Util.Log.Warn("Citiz", $"Voice selection failed: {ex.Message}");
        }

        var result = _tts.SetLanguage(Java.Util.Locale.ForLanguageTag(lang));
        Android.Util.Log.Info("Citiz", $"Fallback SetLanguage({lang}): {result}");
        return result >= LanguageAvailableResult.Available;
    }

    /// <inheritdoc />
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _tts.Shutdown();
            _tts.Dispose();
        }

        base.Dispose(disposing);
    }
}
#endif
