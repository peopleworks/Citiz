using System.Text;
using Citiz.Content;

namespace Citiz.Testing;

/// <summary>An <see cref="IContentStore"/> over strings, for tests that need a specific (often broken) content file.</summary>
public sealed class MemoryContentStore : IContentStore
{
    private readonly Dictionary<string, string> _files = new(StringComparer.Ordinal);

    /// <summary>Adds or replaces a file.</summary>
    public MemoryContentStore With(string relativePath, string json)
    {
        _files[relativePath] = json;
        return this;
    }

    /// <inheritdoc />
    public Task<Stream> OpenReadAsync(string relativePath, CancellationToken cancellationToken = default) =>
        _files.TryGetValue(relativePath, out var json)
            ? Task.FromResult<Stream>(new MemoryStream(Encoding.UTF8.GetBytes(json)))
            : throw new FileNotFoundException($"'{relativePath}' is not in the in-memory store.", relativePath);

    /// <inheritdoc />
    public Task<bool> ExistsAsync(string relativePath, CancellationToken cancellationToken = default) =>
        Task.FromResult(_files.ContainsKey(relativePath));
}
