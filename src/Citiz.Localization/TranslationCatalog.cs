using System.Globalization;

namespace Citiz.Localization;

/// <summary>The strings of one interface language: a flat map from key to text, loaded from <c>i18n/&lt;code&gt;.json</c>.</summary>
public sealed class TranslationCatalog
{
    private readonly IReadOnlyDictionary<string, string> _values;

    /// <summary>Creates a catalog.</summary>
    public TranslationCatalog(string culture, IReadOnlyDictionary<string, string> values)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(culture);
        ArgumentNullException.ThrowIfNull(values);
        Culture = culture;
        _values = values;
    }

    /// <summary>The language code.</summary>
    public string Culture { get; }

    /// <summary>Every key in the catalog.</summary>
    public IEnumerable<string> Keys => _values.Keys;

    /// <summary>Number of strings.</summary>
    public int Count => _values.Count;

    /// <summary>The text for <paramref name="key"/>, or <c>null</c> when the catalog lacks it.</summary>
    public string? Get(string key) => _values.GetValueOrDefault(key);

    /// <summary>Whether the catalog has <paramref name="key"/>.</summary>
    public bool Contains(string key) => _values.ContainsKey(key);

    /// <summary>The text for <paramref name="key"/> with <c>{0}</c>-style placeholders filled, or <c>null</c>.</summary>
    public string? Format(string key, params object?[] args)
    {
        var template = Get(key);
        return template is null ? null : string.Format(CultureInfo.InvariantCulture, template, args);
    }
}
