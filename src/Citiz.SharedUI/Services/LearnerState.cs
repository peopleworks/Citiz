using System.Text.Json;
using System.Text.Json.Serialization;
using Citiz.Learning;
using Citiz.Localization;

namespace Citiz.SharedUI.Services;

/// <summary>What the learner told Citiz about their exam. All optional; nothing here identifies a person.</summary>
/// <param name="FilingDate">When they filed Form N-400, if they know it.</param>
/// <param name="VersionId">An explicit version choice, which wins over the filing date.</param>
/// <param name="SeniorConsideration">Whether they qualify for the 65/20 special consideration.</param>
/// <param name="InterviewDate">When their naturalization interview is, if USCIS already scheduled it. Only used for the countdown on Home — never the exam version, which is decided by <paramref name="FilingDate"/>.</param>
public sealed record ExamSettings(DateOnly? FilingDate, string? VersionId, bool SeniorConsideration, DateOnly? InterviewDate = null)
{
    /// <summary>No choices made yet.</summary>
    public static ExamSettings Empty { get; } = new(null, null, false, null);
}

/// <summary>Source-generated serializer metadata for the settings kept in localStorage.</summary>
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(ExamSettings))]
[JsonSerializable(typeof(LanguageProfile))]
public sealed partial class WebJsonContext : JsonSerializerContext
{
}

/// <summary>
/// Everything Citiz remembers about the learner, kept in the browser's localStorage: language
/// profile, exam settings and the progress ledger. The learner can export it as a file or delete
/// it, and nothing in it ever leaves the device.
/// </summary>
public sealed class LearnerState(BrowserStorage storage, LocalizationService localization)
{
    private const string ProfileKey = "citiz.profile";
    private const string ExamKey = "citiz.exam";
    private const string ProgressKey = "citiz.progress";
    private const string NameKey = "citiz.name";
    private const string ThemeKey = "citiz.theme";

    /// <summary>Raised after settings or progress change.</summary>
    public event Action? Changed;

    /// <summary>The learner's exam settings.</summary>
    public ExamSettings Exam { get; private set; } = ExamSettings.Empty;

    /// <summary>The learner's progress.</summary>
    public ProgressLedger Progress { get; private set; } = new();

    /// <summary>What the learner wants to be called, for the Home greeting. Never sent anywhere.</summary>
    public string? Name { get; private set; }

    /// <summary>The chosen appearance: <c>"light"</c>, <c>"dark"</c>, or <c>null</c> to follow the system.</summary>
    public string? Theme { get; private set; }

    /// <summary>Whether <see cref="InitializeAsync"/> has completed.</summary>
    public bool IsInitialized { get; private set; }

    /// <summary>Loads everything from localStorage and applies the language profile (browser language on first visit).</summary>
    public async Task InitializeAsync()
    {
        if (IsInitialized)
        {
            return;
        }

        var profileJson = await storage.GetAsync(ProfileKey);
        var profile = Deserialize(profileJson, WebJsonContext.Default.LanguageProfile)
            ?? LanguageProfile.ForInterface(SupportedLanguages.NormalizeBrowserLanguage(await storage.GetBrowserLanguageAsync()));

        await localization.InitializeAsync(profile);
        await ApplyDocumentLanguageAsync();

        Exam = Deserialize(await storage.GetAsync(ExamKey), WebJsonContext.Default.ExamSettings) ?? ExamSettings.Empty;
        Progress = new ProgressLedger(ProgressSnapshot.FromJson(await storage.GetAsync(ProgressKey)));
        Name = NormalizeName(await storage.GetAsync(NameKey));

        var theme = await storage.GetAsync(ThemeKey);
        Theme = theme is "light" or "dark" ? theme : null;

        IsInitialized = true;
        Changed?.Invoke();
    }

    /// <summary>Changes the learner's name (used only for the Home greeting) and remembers it.</summary>
    public async Task SetNameAsync(string? name)
    {
        Name = NormalizeName(name);
        if (Name is null)
        {
            await storage.RemoveAsync(NameKey);
        }
        else
        {
            await storage.SetAsync(NameKey, Name);
        }

        Changed?.Invoke();
    }

    /// <summary>Changes the appearance (<c>"light"</c>, <c>"dark"</c>, or <c>null</c> for system) and applies it immediately.</summary>
    public async Task SetThemeAsync(string? theme)
    {
        Theme = theme is "light" or "dark" ? theme : null;
        if (Theme is null)
        {
            await storage.RemoveAsync(ThemeKey);
        }
        else
        {
            await storage.SetAsync(ThemeKey, Theme);
        }

        await storage.ApplyThemeAsync(Theme);
        Changed?.Invoke();
    }

    /// <summary>Changes the language profile and remembers it.</summary>
    public async Task SetLanguageProfileAsync(LanguageProfile profile)
    {
        await localization.SetProfileAsync(profile);
        await storage.SetAsync(ProfileKey, JsonSerializer.Serialize(localization.Profile, WebJsonContext.Default.LanguageProfile));
        await ApplyDocumentLanguageAsync();
        Changed?.Invoke();
    }

    /// <summary>Changes the exam settings and remembers them.</summary>
    public async Task SetExamAsync(ExamSettings settings)
    {
        Exam = settings;
        await storage.SetAsync(ExamKey, JsonSerializer.Serialize(settings, WebJsonContext.Default.ExamSettings));
        Changed?.Invoke();
    }

    /// <summary>Records one practice attempt and persists the ledger.</summary>
    public async Task RecordAsync(string itemId, bool correct)
    {
        Progress.Record(itemId, correct, DateTimeOffset.UtcNow);
        await SaveProgressAsync();
    }

    /// <summary>Persists the ledger.</summary>
    public async Task SaveProgressAsync()
    {
        await storage.SetAsync(ProgressKey, Progress.ToSnapshot().ToJson());
        Changed?.Invoke();
    }

    /// <summary>The learner's data as a portable JSON document.</summary>
    public string Export() => Progress.ToSnapshot().ToJson();

    /// <summary>Deletes everything Citiz stored in this browser.</summary>
    public async Task ClearAllAsync()
    {
        await storage.RemoveAsync(ProfileKey);
        await storage.RemoveAsync(ExamKey);
        await storage.RemoveAsync(ProgressKey);
        await storage.RemoveAsync(NameKey);
        await storage.RemoveAsync(ThemeKey);
        Exam = ExamSettings.Empty;
        Progress = new ProgressLedger();
        Name = null;
        Theme = null;
        await storage.ApplyThemeAsync(null);
        Changed?.Invoke();
    }

    private ValueTask ApplyDocumentLanguageAsync() =>
        storage.SetDocumentLanguageAsync(localization.Language.Code, localization.Language.HtmlDirection);

    private static string? NormalizeName(string? name) => string.IsNullOrWhiteSpace(name) ? null : name.Trim();

    private static T? Deserialize<T>(string? json, System.Text.Json.Serialization.Metadata.JsonTypeInfo<T> typeInfo)
        where T : class
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        try
        {
            return JsonSerializer.Deserialize(json, typeInfo);
        }
        catch (JsonException)
        {
            return null;
        }
    }
}
