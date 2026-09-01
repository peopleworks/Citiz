namespace Citiz.Localization;

/// <summary>
/// The interface languages Citiz ships. Adding one is: add a line here, add
/// <c>src/Citiz.Web/wwwroot/i18n/&lt;code&gt;.json</c>, run <c>citiz localization validate</c>.
/// The initial five follow the most spoken home languages in the United States after English
/// (see the design document, section 8.1); Arabic is included to prove right-to-left support.
/// </summary>
public static class SupportedLanguages
{
    /// <summary>The language every other pack falls back to, and the source of every translation.</summary>
    public const string Fallback = "en";

    /// <summary>Every supported interface language, in menu order.</summary>
    public static IReadOnlyList<LanguageDefinition> All { get; } =
    [
        new("en", "English", "English", TextDirection.LeftToRight, TranslationReviewStatus.Source),
        new("es", "Spanish", "Español", TextDirection.LeftToRight, TranslationReviewStatus.Reviewed),
        new("zh-Hans", "Chinese (Simplified)", "简体中文", TextDirection.LeftToRight, TranslationReviewStatus.MachineDraft),
        new("zh-Hant", "Chinese (Traditional)", "繁體中文", TextDirection.LeftToRight, TranslationReviewStatus.MachineDraft),
        new("fil", "Filipino", "Filipino", TextDirection.LeftToRight, TranslationReviewStatus.MachineDraft),
        new("vi", "Vietnamese", "Tiếng Việt", TextDirection.LeftToRight, TranslationReviewStatus.MachineDraft),
        new("ar", "Arabic", "العربية", TextDirection.RightToLeft, TranslationReviewStatus.MachineDraft),
    ];

    /// <summary>The languages a learner can practise in. The naturalization interview is in English; Spanish is offered for the interface-only use case of studying the vocabulary of the process.</summary>
    public static IReadOnlyList<LanguageDefinition> StudyLanguages { get; } = All.Where(l => l.Code is "en" or "es").ToList();

    /// <summary>Finds a language by code (case-insensitive), or <c>null</c>.</summary>
    public static LanguageDefinition? Find(string? code) =>
        code is null ? null : All.FirstOrDefault(l => string.Equals(l.Code, code, StringComparison.OrdinalIgnoreCase));

    /// <summary>Whether <paramref name="code"/> is a supported interface language.</summary>
    public static bool IsSupported(string? code) => Find(code) is not null;

    /// <summary>
    /// The supported language closest to a browser language tag (<c>navigator.language</c>):
    /// <c>es-MX</c> to <c>es</c>, <c>zh-TW</c> and <c>zh-HK</c> to <c>zh-Hant</c>, other <c>zh</c> to
    /// <c>zh-Hans</c>, <c>tl</c> to <c>fil</c>, anything unknown to <see cref="Fallback"/>.
    /// </summary>
    public static string NormalizeBrowserLanguage(string? browserLanguage)
    {
        if (string.IsNullOrWhiteSpace(browserLanguage))
        {
            return Fallback;
        }

        var tag = browserLanguage.Trim();
        if (IsSupported(tag))
        {
            return Find(tag)!.Code;
        }

        var parts = tag.Split('-', StringSplitOptions.RemoveEmptyEntries);
        var primary = parts[0].ToLowerInvariant();

        if (primary == "zh")
        {
            var traditional = parts.Skip(1).Any(p =>
                p.Equals("Hant", StringComparison.OrdinalIgnoreCase) ||
                p.Equals("TW", StringComparison.OrdinalIgnoreCase) ||
                p.Equals("HK", StringComparison.OrdinalIgnoreCase) ||
                p.Equals("MO", StringComparison.OrdinalIgnoreCase));
            return traditional ? "zh-Hant" : "zh-Hans";
        }

        if (primary == "tl")
        {
            return "fil";
        }

        return Find(primary)?.Code ?? Fallback;
    }
}
