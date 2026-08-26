using Citiz.Content;

namespace Citiz.Hybrid.Services;

/// <summary>
/// An <see cref="IContentStore"/> over the content files bundled into this app's package as
/// <c>MauiAsset</c> items (see Citiz.Hybrid.csproj) — the Hybrid equivalent of Citiz.Web's
/// HttpContentStore, reading from the package instead of over HTTP. Bundled under the
/// <c>content/</c> logical prefix, matching the "content/" URL segment the web host uses.
/// </summary>
public sealed class AppPackageContentStore : IContentStore
{
    /// <inheritdoc />
    public async Task<Stream> OpenReadAsync(string relativePath, CancellationToken cancellationToken = default)
    {
        try
        {
            return await FileSystem.OpenAppPackageFileAsync($"content/{relativePath}");
        }
        catch (FileNotFoundException ex)
        {
            throw new FileNotFoundException($"Content file '{relativePath}' was not found in the app package.", relativePath, ex);
        }
    }

    /// <inheritdoc />
    public async Task<bool> ExistsAsync(string relativePath, CancellationToken cancellationToken = default)
    {
        try
        {
            await using var stream = await FileSystem.OpenAppPackageFileAsync($"content/{relativePath}");
            return true;
        }
        catch (FileNotFoundException)
        {
            return false;
        }
    }
}
