using Citiz.Testing;

namespace Citiz.Localization.Tests;

/// <summary>
/// The language packs that ship in src/Citiz.Web/wwwroot/i18n must agree with en.json. This is the
/// test that reviews a translation pull request on evidence: a mistyped key, a lost {0} or an empty
/// value fails here by name.
/// </summary>
public sealed class LanguagePackTests
{
    private static Dictionary<string, TranslationCatalog> Load()
    {
        var catalogs = new Dictionary<string, TranslationCatalog>(StringComparer.OrdinalIgnoreCase);
        foreach (var file in Directory.EnumerateFiles(RepositoryPaths.I18n, "*.json"))
        {
            var culture = Path.GetFileNameWithoutExtension(file);
            catalogs[culture] = TranslationCatalogJson.Parse(culture, File.ReadAllText(file));
        }

        return catalogs;
    }

    [Fact]
    public void Every_supported_language_has_a_pack_and_every_pack_is_supported()
    {
        var catalogs = Load();

        Assert.Equal(SupportedLanguages.All.Select(l => l.Code).Order(StringComparer.Ordinal), catalogs.Keys.Order(StringComparer.Ordinal));
    }

    [Fact]
    public void Packs_have_the_same_keys_no_empty_values_and_matching_placeholders()
    {
        var issues = TranslationCatalogValidator.Validate(Load());

        Assert.True(issues.Count == 0, string.Join(Environment.NewLine, issues.Select(i => $"{i.Culture}: {i.Kind} {i.Key}")));
    }

    [Fact]
    public void Validator_reports_each_kind_of_problem_by_name()
    {
        var catalogs = new Dictionary<string, TranslationCatalog>
        {
            ["en"] = new("en", new Dictionary<string, string> { ["a"] = "A", ["b"] = "B", ["n"] = "{0} of {1}" }),
            ["es"] = new("es", new Dictionary<string, string> { ["a"] = "", ["c"] = "C", ["n"] = "{0} de todos" }),
        };

        var issues = TranslationCatalogValidator.Validate(catalogs);

        Assert.Contains(issues, i => i is { Culture: "es", Kind: TranslationIssueKind.MissingKey, Key: "b" });
        Assert.Contains(issues, i => i is { Culture: "es", Kind: TranslationIssueKind.ExtraKey, Key: "c" });
        Assert.Contains(issues, i => i is { Culture: "es", Kind: TranslationIssueKind.EmptyValue, Key: "a" });
        Assert.Contains(issues, i => i is { Culture: "es", Kind: TranslationIssueKind.PlaceholderMismatch, Key: "n" });
        Assert.Contains(issues, i => i is { Kind: TranslationIssueKind.UnknownLanguage, Key: "vi" });
    }
}
