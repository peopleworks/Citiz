namespace Citiz.Localization;

/// <summary>Loads the catalog of one language from wherever the host keeps it: HTTP in the browser, disk in the CLI.</summary>
public interface ITranslationCatalogLoader
{
    /// <summary>The catalog for <paramref name="culture"/>, or <c>null</c> when there is none.</summary>
    Task<TranslationCatalog?> LoadAsync(string culture, CancellationToken cancellationToken = default);
}
