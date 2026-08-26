using System.Reflection;
using Citiz.Content;

namespace Citiz.Cli;

/// <summary>Console formatting and the shared way commands locate the repository's folders.</summary>
public static class ConsoleOutput
{
    /// <summary>The informational version of this build.</summary>
    public static string Version =>
        typeof(ConsoleOutput).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? "0.0.0";

    /// <summary>Prints the usage text.</summary>
    public static void Usage()
    {
        Console.WriteLine($"""
            citiz {Version} — content and translation tooling for Citiz

            Usage:
              citiz content validate [--content <dir>]        Validate every content file. Exit 1 on errors.
              citiz content report   [--content <dir>]        Review-state summary: what still needs a human.
              citiz localization validate [--i18n <dir>]      Check every language pack against en.json.
              citiz localization status   [--i18n <dir>]      Keys and review status per language.
              citiz exam resolve <yyyy-MM-dd> [--content <dir>]
                                                              Which civics-test version applies to that N-400 filing date.
              citiz exam simulate [--version <id>] [--senior] [--seed <n>] [--content <dir>]
                                                              A practice sitting in the terminal, scored like an officer would.

            Options:
              --content <dir>   The content folder. Default: the nearest content/ above the current directory.
              --i18n <dir>      The language-pack folder. Default: src/Citiz.Web/wwwroot/i18n in the repository.
              --help, --version
            """);
    }

    /// <summary>Prints an error line to stderr in red.</summary>
    public static void Error(string message) => WriteColored(Console.Error, ConsoleColor.Red, message);

    /// <summary>Prints a warning line in yellow.</summary>
    public static void Warning(string message) => WriteColored(Console.Out, ConsoleColor.Yellow, message);

    /// <summary>Prints a success line in green.</summary>
    public static void Success(string message) => WriteColored(Console.Out, ConsoleColor.Green, message);

    /// <summary>Prints a dim line.</summary>
    public static void Muted(string message) => WriteColored(Console.Out, ConsoleColor.DarkGray, message);

    /// <summary>Resolves the content folder from <c>--content</c> or by searching upwards.</summary>
    /// <exception cref="CommandException">No content folder was found.</exception>
    public static string ContentRoot(CommandLineArguments arguments)
    {
        ArgumentNullException.ThrowIfNull(arguments);

        var root = arguments.Option("content") ?? FileContentStore.LocateContentRoot();
        if (root is null || !Directory.Exists(root))
        {
            throw new CommandException("Could not find the content folder. Run from inside the repository or pass --content <dir>.");
        }

        return root;
    }

    /// <summary>Resolves the language-pack folder from <c>--i18n</c> or the repository layout.</summary>
    /// <exception cref="CommandException">No folder was found.</exception>
    public static string I18nRoot(CommandLineArguments arguments)
    {
        ArgumentNullException.ThrowIfNull(arguments);

        var explicitPath = arguments.Option("i18n");
        if (explicitPath is not null)
        {
            return Directory.Exists(explicitPath)
                ? explicitPath
                : throw new CommandException($"Language-pack folder '{explicitPath}' does not exist.");
        }

        var repository = RepositoryRoot();
        var candidate = repository is null ? null : Path.Combine(repository, "src", "Citiz.Web", "wwwroot", "i18n");
        return candidate is not null && Directory.Exists(candidate)
            ? candidate
            : throw new CommandException("Could not find src/Citiz.Web/wwwroot/i18n. Run from inside the repository or pass --i18n <dir>.");
    }

    /// <summary>The repository root (the folder containing <c>Citiz.slnx</c>), or <c>null</c>.</summary>
    public static string? RepositoryRoot()
    {
        var directory = new DirectoryInfo(Directory.GetCurrentDirectory());
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "Citiz.slnx")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        return null;
    }

    private static void WriteColored(TextWriter writer, ConsoleColor color, string message)
    {
        var previous = Console.ForegroundColor;
        try
        {
            Console.ForegroundColor = color;
            writer.WriteLine(message);
        }
        finally
        {
            Console.ForegroundColor = previous;
        }
    }
}
