namespace Citiz.Localization;

/// <summary>
/// The interface's translation lookup. Holds the current <see cref="LanguageProfile"/>, the catalog
/// for its interface language and the English fallback, and raises <see cref="Changed"/> when the
/// profile changes so components re-render without a page reload.
/// </summary>
public sealed class LocalizationService
{
    private readonly ITranslationCatalogLoader _loader;
    private readonly Dictionary<string, TranslationCatalog> _cache = new(StringComparer.OrdinalIgnoreCase);
    private TranslationCatalog? _current;
    private TranslationCatalog? _fallback;

    /// <summary>Creates the service over <paramref name="loader"/>.</summary>
    public LocalizationService(ITranslationCatalogLoader loader)
    {
        ArgumentNullException.ThrowIfNull(loader);
        _loader = loader;
    }

    /// <summary>Raised after the profile or catalog changes.</summary>
    public event Action? Changed;

    /// <summary>The current profile.</summary>
    public LanguageProfile Profile { get; private set; } = LanguageProfile.Default;

    /// <summary>The current interface language.</summary>
    public LanguageDefinition Language => SupportedLanguages.Find(Profile.InterfaceCulture) ?? SupportedLanguages.All[0];

    /// <summary>Whether <see cref="InitializeAsync"/> has completed.</summary>
    public bool IsInitialized { get; private set; }

    /// <summary>Loads the fallback catalog and applies <paramref name="profile"/>.</summary>
    public async Task InitializeAsync(LanguageProfile profile, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(profile);

        _fallback = await LoadCachedAsync(SupportedLanguages.Fallback, cancellationToken).ConfigureAwait(false);
        await SetProfileAsync(profile, cancellationToken).ConfigureAwait(false);
        IsInitialized = true;
    }

    /// <summary>Applies a new profile, loading its interface catalog if needed.</summary>
    public async Task SetProfileAsync(LanguageProfile profile, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(profile);

        var interfaceCulture = SupportedLanguages.IsSupported(profile.InterfaceCulture) ? profile.InterfaceCulture : SupportedLanguages.Fallback;
        Profile = profile with { InterfaceCulture = interfaceCulture };
        _current = await LoadCachedAsync(interfaceCulture, cancellationToken).ConfigureAwait(false);
        Changed?.Invoke();
    }

    /// <summary>Changes only the interface language, keeping help in step with it.</summary>
    public Task SetInterfaceCultureAsync(string culture, CancellationToken cancellationToken = default) =>
        SetProfileAsync(Profile with { InterfaceCulture = culture, HelpCulture = culture }, cancellationToken);

    /// <summary>Changes only the study language.</summary>
    public Task SetStudyCultureAsync(string culture, CancellationToken cancellationToken = default) =>
        SetProfileAsync(Profile with { StudyCulture = culture }, cancellationToken);

    /// <summary>
    /// The text for <paramref name="key"/> in the interface language, falling back to English, then
    /// to the key in brackets so a missing string is visible rather than blank.
    /// </summary>
    public string T(string key)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        return _current?.Get(key) ?? _fallback?.Get(key) ?? $"[{key}]";
    }

    /// <summary>Like <see cref="T(string)"/>, with <c>{0}</c>-style placeholders filled.</summary>
    public string T(string key, params object?[] args)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        return _current?.Format(key, args) ?? _fallback?.Format(key, args) ?? $"[{key}]";
    }

    private async Task<TranslationCatalog?> LoadCachedAsync(string culture, CancellationToken cancellationToken)
    {
        if (_cache.TryGetValue(culture, out var cached))
        {
            return cached;
        }

        var catalog = await _loader.LoadAsync(culture, cancellationToken).ConfigureAwait(false);
        if (catalog is not null)
        {
            _cache[culture] = catalog;
        }

        return catalog;
    }
}
