namespace Citiz.Content;

/// <summary>
/// Opens content files by path relative to the content root (<c>exams/versions.json</c>). The CLI,
/// API and worker read the folder on disk; the browser fetches the same files over HTTP. Nothing
/// above this interface knows which.
/// </summary>
public interface IContentStore
{
    /// <summary>Opens a content file for reading.</summary>
    /// <exception cref="FileNotFoundException">The file does not exist in this store.</exception>
    Task<Stream> OpenReadAsync(string relativePath, CancellationToken cancellationToken = default);

    /// <summary>Whether a content file exists in this store.</summary>
    Task<bool> ExistsAsync(string relativePath, CancellationToken cancellationToken = default);
}
