#if IOS || MACCATALYST
using AVFoundation;
using Citiz.SharedUI.Services;

namespace Citiz.Hybrid.Services;

/// <summary>
/// Text to speech through Apple's <see cref="AVSpeechSynthesizer"/>, on the device, choosing the
/// best voice installed for the language. iOS ships one compact ("Default" quality) voice per
/// language, which is the flat, computer-like one; "Enhanced" and "Premium" voices are free
/// downloads (Settings → Accessibility → Spoken Content → Voices) and this service prefers them
/// automatically the moment one exists. MAUI's own TextToSpeech can only pick a language, not a
/// voice or a quality, and passes the rate straight to Apple's 0–1 scale — the two reasons this
/// class exists (Pedro heard the first version "too fast to understand" on 2026-09-01).
/// </summary>
public sealed class AppleSpeechService : ISpeechService
{
    // Tie-breakers among voices of equal quality; Apple's well-known en-US voices first, so a
    // novelty voice ("Bahh", "Zarvox"…) that Mac Catalyst also lists is never picked by accident.
    private static readonly string[] PreferredNames = ["Ava", "Zoe", "Evan", "Nathan", "Samantha", "Allison", "Nicky", "Aaron", "Susan", "Tom", "Alex"];

    private readonly AVSpeechSynthesizer _synthesizer = new();
    private readonly Dictionary<string, AVSpeechSynthesisVoice?> _chosen = new(StringComparer.OrdinalIgnoreCase);
    private bool _audioSessionConfigured;

    public AppleSpeechService()
    {
        var voices = AVSpeechSynthesisVoice.GetSpeechVoices();
        Console.WriteLine($"[Citiz] {voices.Length} speech voices installed: " +
            string.Join(", ", voices.Where(v => v.Language.StartsWith("en", StringComparison.OrdinalIgnoreCase)).Select(v => $"{v.Name} ({v.Language}, {v.Quality})")));
    }

    /// <summary>Whether any voice is installed.</summary>
    public ValueTask<bool> IsAvailableAsync() => new(AVSpeechSynthesisVoice.GetSpeechVoices().Length > 0);

    /// <summary>Whether a voice for <paramref name="lang"/> exists; Apple's voices synthesize on the device.</summary>
    public ValueTask<bool> IsLocalVoiceAsync(string lang) => new(Voice(lang) is not null);

    /// <summary>Speaks <paramref name="text"/>; <paramref name="rate"/> is on the Web Speech scale (1 is normal).</summary>
    public ValueTask SpeakAsync(string text, string lang = "en-US", double rate = 0.9)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return default;
        }

        var voice = Voice(lang);
        var utterance = new AVSpeechUtterance(text)
        {
            Voice = voice,
            Rate = (float)Math.Clamp(rate * AVSpeechUtterance.DefaultSpeechRate, AVSpeechUtterance.MinimumSpeechRate, AVSpeechUtterance.MaximumSpeechRate),
        };

        MainThread.BeginInvokeOnMainThread(() =>
        {
            ConfigureAudioSession();
            if (_synthesizer.Speaking)
            {
                _synthesizer.StopSpeaking(AVSpeechBoundary.Immediate);
            }

            _synthesizer.SpeakUtterance(utterance);
        });

        return default;
    }

    /// <summary>Stops the utterance in progress, if any.</summary>
    public ValueTask StopAsync()
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            if (_synthesizer.Speaking)
            {
                _synthesizer.StopSpeaking(AVSpeechBoundary.Immediate);
            }
        });

        return default;
    }

    private AVSpeechSynthesisVoice? Voice(string lang)
    {
        if (_chosen.TryGetValue(lang, out var cached))
        {
            return cached;
        }

        var voice = ChooseVoice(lang);
        _chosen[lang] = voice;
        Console.WriteLine(voice is null ? $"[Citiz] no voice for {lang}" : $"[Citiz] voice for {lang}: {voice.Name} ({voice.Quality}, {voice.Identifier})");
        return voice;
    }

    /// <summary>
    /// Highest quality first. Among equal quality, a known natural voice; if only compact voices
    /// exist, the system default for the language rather than an arbitrary one.
    /// </summary>
    private static AVSpeechSynthesisVoice? ChooseVoice(string lang)
    {
        var all = AVSpeechSynthesisVoice.GetSpeechVoices();
        var language = lang.Split('-')[0];
        var candidates = all.Where(v => string.Equals(v.Language, lang, StringComparison.OrdinalIgnoreCase)).ToList();
        if (candidates.Count == 0)
        {
            candidates = all.Where(v => v.Language.StartsWith(language + "-", StringComparison.OrdinalIgnoreCase)).ToList();
        }

        if (candidates.Count == 0)
        {
            return AVSpeechSynthesisVoice.FromLanguage(lang);
        }

        var bestQuality = candidates.Max(v => (int)v.Quality);
        var best = candidates.Where(v => (int)v.Quality == bestQuality).OrderBy(Rank).ThenBy(v => v.Name, StringComparer.Ordinal).First();
        return bestQuality > (int)AVSpeechSynthesisVoiceQuality.Default || Rank(best) < PreferredNames.Length
            ? best
            : AVSpeechSynthesisVoice.FromLanguage(lang) ?? best;

        static int Rank(AVSpeechSynthesisVoice v)
        {
            var index = Array.FindIndex(PreferredNames, n => v.Name.StartsWith(n, StringComparison.OrdinalIgnoreCase));
            return index < 0 ? PreferredNames.Length : index;
        }
    }

    private void ConfigureAudioSession()
    {
        if (_audioSessionConfigured)
        {
            return;
        }

        _audioSessionConfigured = true;
        try
        {
            // Playback: audible with the ring/silent switch on silent, like any study app's audio.
            var session = AVAudioSession.SharedInstance();
            session.SetCategory(AVAudioSessionCategory.Playback);
            session.SetActive(true);
        }
        catch (Exception)
        {
            // Speech still works with the default session; only the silent-switch behaviour differs.
        }
    }
}
#endif
