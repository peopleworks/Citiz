using System.Text.RegularExpressions;

namespace Citiz.Localization;

/// <summary>What is wrong with one key of one language pack.</summary>
public enum TranslationIssueKind
{
    /// <summary>The reference has the key and this pack does not.</summary>
    MissingKey,

    /// <summary>This pack has a key the reference does not.</summary>
    ExtraKey,

    /// <summary>The value is empty or whitespace.</summary>
    EmptyValue,

    /// <summary>The value's <c>{n}</c> placeholders differ from the reference's.</summary>
    PlaceholderMismatch,

    /// <summary>The pack is not among <see cref="SupportedLanguages.All"/>, or a supported language has no pack.</summary>
    UnknownLanguage,
}

/// <summary>One finding of <see cref="TranslationCatalogValidator"/>.</summary>
/// <param name="Culture">The pack.</param>
/// <param name="Kind">What is wrong.</param>
/// <param name="Key">The key, or the culture code for pack-level findings.</param>
public sealed record TranslationIssue(string Culture, TranslationIssueKind Kind, string Key);

/// <summary>
/// Checks language packs against the reference (English): same keys, no empty values, same
/// placeholders. Runs in CI so a translation pull request is reviewed on evidence.
/// </summary>
public static partial class TranslationCatalogValidator
{
    /// <summary>Validates <paramref name="catalogs"/> (keyed by culture) against <paramref name="reference"/>.</summary>
    public static IReadOnlyList<TranslationIssue> Validate(
        IReadOnlyDictionary<string, TranslationCatalog> catalogs,
        string reference = SupportedLanguages.Fallback)
    {
        ArgumentNullException.ThrowIfNull(catalogs);

        var issues = new List<TranslationIssue>();

        if (!catalogs.TryGetValue(reference, out var referenceCatalog))
        {
            issues.Add(new TranslationIssue(reference, TranslationIssueKind.MissingKey, "*"));
            return issues;
        }

        foreach (var language in SupportedLanguages.All.Where(l => !catalogs.ContainsKey(l.Code)))
        {
            issues.Add(new TranslationIssue(language.Code, TranslationIssueKind.UnknownLanguage, language.Code));
        }

        var referenceKeys = referenceCatalog.Keys.ToHashSet(StringComparer.Ordinal);

        foreach (var (culture, catalog) in catalogs.OrderBy(c => c.Key, StringComparer.Ordinal))
        {
            if (!SupportedLanguages.IsSupported(culture))
            {
                issues.Add(new TranslationIssue(culture, TranslationIssueKind.UnknownLanguage, culture));
            }

            var keys = catalog.Keys.ToHashSet(StringComparer.Ordinal);

            issues.AddRange(referenceKeys.Except(keys).Order(StringComparer.Ordinal).Select(k => new TranslationIssue(culture, TranslationIssueKind.MissingKey, k)));
            issues.AddRange(keys.Except(referenceKeys).Order(StringComparer.Ordinal).Select(k => new TranslationIssue(culture, TranslationIssueKind.ExtraKey, k)));

            foreach (var key in keys.Intersect(referenceKeys).Order(StringComparer.Ordinal))
            {
                var value = catalog.Get(key)!;
                if (string.IsNullOrWhiteSpace(value))
                {
                    issues.Add(new TranslationIssue(culture, TranslationIssueKind.EmptyValue, key));
                    continue;
                }

                if (!Placeholders(value).SetEquals(Placeholders(referenceCatalog.Get(key)!)))
                {
                    issues.Add(new TranslationIssue(culture, TranslationIssueKind.PlaceholderMismatch, key));
                }
            }
        }

        return issues;
    }

    private static HashSet<string> Placeholders(string value) =>
        PlaceholderRegex().Matches(value).Select(m => m.Value).ToHashSet(StringComparer.Ordinal);

    [GeneratedRegex(@"\{\d+\}")]
    private static partial Regex PlaceholderRegex();
}
