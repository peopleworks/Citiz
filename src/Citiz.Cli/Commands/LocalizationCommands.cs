using System.Text.Json;
using Citiz.Localization;

namespace Citiz.Cli.Commands;

/// <summary><c>citiz localization ...</c>.</summary>
public static class LocalizationCommands
{
    /// <summary>Checks every language pack against the English reference. Exit 1 on findings.</summary>
    public static async Task<int> ValidateAsync(CommandLineArguments arguments)
    {
        var root = ConsoleOutput.I18nRoot(arguments);
        Console.WriteLine($"Validating language packs in {root}");

        var (catalogs, parseErrors) = await LoadAsync(root);
        foreach (var error in parseErrors)
        {
            ConsoleOutput.Error("ERROR  " + error);
        }

        var issues = TranslationCatalogValidator.Validate(catalogs);
        foreach (var group in issues.GroupBy(i => i.Culture, StringComparer.Ordinal).OrderBy(g => g.Key, StringComparer.Ordinal))
        {
            foreach (var kind in group.GroupBy(i => i.Kind))
            {
                var keys = kind.Select(i => i.Key).ToList();
                var shown = string.Join(", ", keys.Take(8)) + (keys.Count > 8 ? $" (+{keys.Count - 8} more)" : string.Empty);
                ConsoleOutput.Error($"ERROR  {group.Key}: {Describe(kind.Key)} — {shown}");
            }
        }

        Console.WriteLine();
        if (parseErrors.Count == 0 && issues.Count == 0)
        {
            ConsoleOutput.Success($"All {catalogs.Count} language packs are complete and consistent with en.json.");
            return 0;
        }

        ConsoleOutput.Error($"{parseErrors.Count + issues.Count} problem(s) found.");
        return 1;
    }

    /// <summary>Prints keys and review status per language.</summary>
    public static async Task<int> StatusAsync(CommandLineArguments arguments)
    {
        var root = ConsoleOutput.I18nRoot(arguments);
        var (catalogs, parseErrors) = await LoadAsync(root);

        foreach (var error in parseErrors)
        {
            ConsoleOutput.Error("ERROR  " + error);
        }

        var reference = catalogs.GetValueOrDefault(SupportedLanguages.Fallback);

        Console.WriteLine($"  {"Code",-8} {"Language",-24} {"Keys",5} {"Missing",8}   Review status");
        foreach (var language in SupportedLanguages.All)
        {
            var catalog = catalogs.GetValueOrDefault(language.Code);
            var keys = catalog?.Count ?? 0;
            var missing = reference is null || catalog is null ? reference?.Count ?? 0 : reference.Keys.Count(k => !catalog.Contains(k));
            Console.WriteLine($"  {language.Code,-8} {language.EnglishName,-24} {keys,5} {missing,8}   {language.Status}");
        }

        foreach (var extra in catalogs.Keys.Where(c => !SupportedLanguages.IsSupported(c)).Order(StringComparer.Ordinal))
        {
            ConsoleOutput.Warning($"  {extra,-8} {"(not in SupportedLanguages.All)",-24} {catalogs[extra].Count,5}");
        }

        return parseErrors.Count == 0 ? 0 : 1;
    }

    private static async Task<(Dictionary<string, TranslationCatalog> Catalogs, List<string> Errors)> LoadAsync(string root)
    {
        var catalogs = new Dictionary<string, TranslationCatalog>(StringComparer.OrdinalIgnoreCase);
        var errors = new List<string>();

        foreach (var file in Directory.EnumerateFiles(root, "*.json").Order(StringComparer.Ordinal))
        {
            var culture = Path.GetFileNameWithoutExtension(file);
            try
            {
                catalogs[culture] = TranslationCatalogJson.Parse(culture, await File.ReadAllTextAsync(file));
            }
            catch (JsonException ex)
            {
                errors.Add($"{Path.GetFileName(file)}: {ex.Message}");
            }
        }

        return (catalogs, errors);
    }

    private static string Describe(TranslationIssueKind kind) => kind switch
    {
        TranslationIssueKind.MissingKey => "missing keys",
        TranslationIssueKind.ExtraKey => "keys not in en.json",
        TranslationIssueKind.EmptyValue => "empty values",
        TranslationIssueKind.PlaceholderMismatch => "placeholders differ from en.json",
        TranslationIssueKind.UnknownLanguage => "language pack and SupportedLanguages.All disagree",
        _ => kind.ToString(),
    };
}
