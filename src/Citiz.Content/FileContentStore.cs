namespace Citiz.Content;

/// <summary>An <see cref="IContentStore"/> over a folder on disk, normally the repository's <c>content/</c>.</summary>
public sealed class FileContentStore : IContentStore
{
    /// <summary>Creates a store rooted at <paramref name="root"/>.</summary>
    /// <exception cref="DirectoryNotFoundException">The folder does not exist.</exception>
    public FileContentStore(string root)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(root);
        Root = Path.GetFullPath(root);
        if (!Directory.Exists(Root))
        {
            throw new DirectoryNotFoundException($"Content root '{Root}' does not exist.");
        }
    }

    /// <summary>Absolute path of the content folder.</summary>
    public string Root { get; }

    /// <inheritdoc />
    public Task<Stream> OpenReadAsync(string relativePath, CancellationToken cancellationToken = default)
    {
        var path = Resolve(relativePath);
        if (!File.Exists(path))
        {
            throw new FileNotFoundException($"Content file '{relativePath}' not found under '{Root}'.", path);
        }

        Stream stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, FileOptions.Asynchronous);
        return Task.FromResult(stream);
    }

    /// <inheritdoc />
    public Task<bool> ExistsAsync(string relativePath, CancellationToken cancellationToken = default) =>
        Task.FromResult(File.Exists(Resolve(relativePath)));

    /// <summary>
    /// Finds the repository's <c>content/</c> folder by walking up from <paramref name="start"/>
    /// (default: the current directory) until a folder containing <c>content/exams/versions.json</c>
    /// appears. Returns <c>null</c> when none is found. Used by the CLI, API and worker so they work
    /// from any folder inside a checkout.
    /// </summary>
    public static string? LocateContentRoot(string? start = null)
    {
        var directory = new DirectoryInfo(start ?? Directory.GetCurrentDirectory());
        while (directory is not null)
        {
            var candidate = Path.Combine(directory.FullName, "content");
            if (File.Exists(Path.Combine(candidate, "exams", "versions.json")))
            {
                return candidate;
            }

            directory = directory.Parent;
        }

        return null;
    }

    private string Resolve(string relativePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(relativePath);
        var full = Path.GetFullPath(Path.Combine(Root, relativePath.Replace('/', Path.DirectorySeparatorChar)));
        if (!full.StartsWith(Root, StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException($"Path '{relativePath}' escapes the content root.", nameof(relativePath));
        }

        return full;
    }
}
