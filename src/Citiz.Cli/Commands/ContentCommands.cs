using Citiz.Content;
using Citiz.Content.Validation;
using Citiz.Core.Content;

namespace Citiz.Cli.Commands;

/// <summary><c>citiz content ...</c>.</summary>
public static class ContentCommands
{
    /// <summary>Validates the content folder and prints every finding. Exit 1 when there are errors.</summary>
    public static async Task<int> ValidateAsync(CommandLineArguments arguments)
    {
        var root = ConsoleOutput.ContentRoot(arguments);
        Console.WriteLine($"Validating content in {root}");

        var report = await new ContentValidator(new FileContentStore(root)).ValidateAsync();

        foreach (var issue in report.Issues)
        {
            var line = $"  {issue.File}: {issue.Message}";
            switch (issue.Severity)
            {
                case ContentIssueSeverity.Error:
                    ConsoleOutput.Error("ERROR" + line);
                    break;
                case ContentIssueSeverity.Warning:
                    ConsoleOutput.Warning("WARN " + line);
                    break;
                default:
                    ConsoleOutput.Muted("info " + line);
                    break;
            }
        }

        Console.WriteLine();
        if (report.IsValid)
        {
            ConsoleOutput.Success($"Content is valid: 0 errors, {report.WarningCount} warning(s).");
            return 0;
        }

        ConsoleOutput.Error($"Content is not valid: {report.ErrorCount} error(s), {report.WarningCount} warning(s).");
        return 1;
    }

    /// <summary>Prints how much of each file is still waiting for a human to verify it.</summary>
    public static async Task<int> ReportAsync(CommandLineArguments arguments)
    {
        var root = ConsoleOutput.ContentRoot(arguments);
        var report = await new ContentValidator(new FileContentStore(root)).ValidateAsync();

        Console.WriteLine($"Review status of content in {root}");
        Console.WriteLine();
        Console.WriteLine($"  {"File",-36} {"Total",5} {"Approved",8} {"Pending",8}   By status");

        var totalPending = 0;
        foreach (var summary in report.Reviews)
        {
            var approved = summary.Counts.GetValueOrDefault(ReviewStatus.Approved);
            var byStatus = string.Join(", ", summary.Counts.OrderBy(c => c.Key).Select(c => $"{c.Key.ToKebabCase()} {c.Value}"));
            Console.WriteLine($"  {summary.File,-36} {summary.Total,5} {approved,8} {summary.Pending,8}   {byStatus}");
            totalPending += summary.Pending;
        }

        Console.WriteLine();
        if (totalPending == 0)
        {
            ConsoleOutput.Success("Every entry is approved.");
        }
        else
        {
            ConsoleOutput.Warning($"{totalPending} entries still need a content maintainer to verify them against their sources. See content/README.md.");
        }

        if (!report.IsValid)
        {
            ConsoleOutput.Error($"Note: the content also has {report.ErrorCount} validation error(s); run 'citiz content validate'.");
            return 1;
        }

        return 0;
    }
}
