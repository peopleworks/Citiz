namespace Citiz.Testing;

/// <summary>Locates the repository from wherever the test runner put the binaries.</summary>
public static class RepositoryPaths
{
    private static readonly Lazy<string> RootLazy = new(Locate);

    /// <summary>The folder containing <c>Citiz.slnx</c>.</summary>
    public static string Root => RootLazy.Value;

    /// <summary>The <c>content/</c> folder.</summary>
    public static string Content => Path.Combine(Root, "content");

    /// <summary>The <c>src/Citiz.Web/wwwroot/i18n</c> folder.</summary>
    public static string I18n => Path.Combine(Root, "src", "Citiz.Web", "wwwroot", "i18n");

    private static string Locate()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "Citiz.slnx")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new InvalidOperationException($"Citiz.slnx not found above {AppContext.BaseDirectory}.");
    }
}
