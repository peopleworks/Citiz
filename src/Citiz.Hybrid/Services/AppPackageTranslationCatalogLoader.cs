using Citiz.Localization;

namespace Citiz.Hybrid.Services;

/// <summary>
/// Loads <c>i18n/&lt;code&gt;.json</c> language packs bundled into this app's package as
/// <c>MauiAsset</c> items — the Hybrid equivalent of Citiz.Web's HttpTranslationCatalogLoader.
/// </summary>
public sealed class AppPackageTranslationCatalogLoader : ITranslationCatalogLoader
{
    /// <inheritdoc />
    public async Task<TranslationCatalog?> LoadAsync(string culture, CancellationToken cancellationToken = default)
    {
        Stream stream;
        try
        {
            stream = await FileSystem.OpenAppPackageFileAsync($"i18n/{culture}.json");
        }
        catch (FileNotFoundException)
        {
            return null;
        }

        await using (stream.ConfigureAwait(false))
        {
            return await TranslationCatalogJson.ParseAsync(culture, stream, cancellationToken);
        }
    }
}
