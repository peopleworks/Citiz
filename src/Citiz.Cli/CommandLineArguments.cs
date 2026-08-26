namespace Citiz.Cli;

/// <summary>
/// The parsed command line: positional words, <c>--name value</c> options and <c>--flag</c> switches.
/// Small on purpose; the command surface is six verbs and does not need a framework.
/// </summary>
public sealed class CommandLineArguments
{
    private readonly Dictionary<string, string?> _options = new(StringComparer.OrdinalIgnoreCase);

    private CommandLineArguments(IReadOnlyList<string> positionals)
    {
        Positionals = positionals;
    }

    /// <summary>Words that are not options, in order.</summary>
    public IReadOnlyList<string> Positionals { get; }

    /// <summary>Parses <paramref name="args"/>.</summary>
    public static CommandLineArguments Parse(string[] args)
    {
        ArgumentNullException.ThrowIfNull(args);

        var positionals = new List<string>();
        var result = new CommandLineArguments(positionals);

        for (var i = 0; i < args.Length; i++)
        {
            var arg = args[i];
            if (arg.StartsWith("--", StringComparison.Ordinal) && arg.Length > 2)
            {
                var name = arg[2..];
                var equals = name.IndexOf('=', StringComparison.Ordinal);
                if (equals > 0)
                {
                    result._options[name[..equals]] = name[(equals + 1)..];
                }
                else if (i + 1 < args.Length && !args[i + 1].StartsWith("--", StringComparison.Ordinal))
                {
                    result._options[name] = args[++i];
                }
                else
                {
                    result._options[name] = null;
                }
            }
            else if (arg is "-h" or "-?")
            {
                result._options["help"] = null;
            }
            else
            {
                positionals.Add(arg);
            }
        }

        return result;
    }

    /// <summary>The value of <c>--name value</c>, or <c>null</c>.</summary>
    public string? Option(string name) => _options.GetValueOrDefault(name);

    /// <summary>Whether <c>--name</c> was given, with or without a value.</summary>
    public bool HasFlag(string name) => _options.ContainsKey(name);
}

/// <summary>A command could not run; the message is for the person at the terminal.</summary>
public sealed class CommandException(string message, int exitCode = 2) : Exception(message)
{
    /// <summary>Process exit code.</summary>
    public int ExitCode { get; } = exitCode;
}
