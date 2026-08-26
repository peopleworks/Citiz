using Citiz.Cli;
using Citiz.Cli.Commands;

// citiz — the maintainer's tool. Every check CI runs is a command here, so a contributor can run
// the same thing locally before opening a pull request. Exit codes: 0 ok, 1 findings, 2 usage.

var arguments = CommandLineArguments.Parse(args);

if (arguments.HasFlag("version"))
{
    Console.WriteLine(ConsoleOutput.Version);
    return 0;
}

if (arguments.Positionals.Count == 0 || arguments.HasFlag("help") || arguments.HasFlag("h"))
{
    ConsoleOutput.Usage();
    return arguments.Positionals.Count == 0 && !arguments.HasFlag("help") && !arguments.HasFlag("h") ? 2 : 0;
}

try
{
    return (arguments.Positionals[0], arguments.Positionals.ElementAtOrDefault(1)) switch
    {
        ("content", "validate") => await ContentCommands.ValidateAsync(arguments),
        ("content", "report") => await ContentCommands.ReportAsync(arguments),
        ("localization", "validate") => await LocalizationCommands.ValidateAsync(arguments),
        ("localization", "status") => await LocalizationCommands.StatusAsync(arguments),
        ("exam", "resolve") => await ExamCommands.ResolveAsync(arguments),
        ("exam", "simulate") => await ExamCommands.SimulateAsync(arguments),
        _ => Unknown(arguments),
    };
}
catch (CommandException ex)
{
    ConsoleOutput.Error(ex.Message);
    return ex.ExitCode;
}

static int Unknown(CommandLineArguments arguments)
{
    ConsoleOutput.Error($"Unknown command '{string.Join(' ', arguments.Positionals.Take(2))}'.");
    ConsoleOutput.Usage();
    return 2;
}
