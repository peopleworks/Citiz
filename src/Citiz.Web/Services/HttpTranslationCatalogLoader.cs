using System.Net;
using Citiz.Localization;

namespace Citiz.Web.Services;

/// <summary>Loads <c>i18n/&lt;code&gt;.json</c> over HTTP; a missing pack is <c>null</c>, not an error, and the service falls back to English.</summary>
public sealed class HttpTranslationCatalogLoader(HttpClient http) : ITranslationCatalogLoader
{
    /// <inheritdoc />
    public async Task<TranslationCatalog?> LoadAsync(string culture, CancellationToken cancellationToken = default)
    {
        using var response = await http.GetAsync($"i18n/{culture}.json", cancellationToken);
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();
        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        return await TranslationCatalogJson.ParseAsync(culture, stream, cancellationToken);
    }
}
