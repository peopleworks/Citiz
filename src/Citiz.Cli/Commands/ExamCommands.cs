using System.Globalization;
using Citiz.Content;
using Citiz.Core.Content;
using Citiz.Core.Exams;

namespace Citiz.Cli.Commands;

/// <summary><c>citiz exam ...</c>.</summary>
public static class ExamCommands
{
    /// <summary>Prints the version that applies to an N-400 filing date.</summary>
    public static async Task<int> ResolveAsync(CommandLineArguments arguments)
    {
        var text = arguments.Positionals.ElementAtOrDefault(2)
            ?? throw new CommandException("Usage: citiz exam resolve <yyyy-MM-dd>");

        if (!DateOnly.TryParseExact(text, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var filingDate))
        {
            throw new CommandException($"'{text}' is not a date in yyyy-MM-dd form.");
        }

        var repository = new ContentRepository(new FileContentStore(ConsoleOutput.ContentRoot(arguments)));
        var versions = await repository.GetExamVersionsAsync();
        var version = ExamPolicy.Resolve(filingDate, versions);

        if (version is null)
        {
            ConsoleOutput.Warning($"No civics-test version in content/exams/versions.json covers an N-400 filed on {filingDate:yyyy-MM-dd}.");
            return 1;
        }

        Console.WriteLine($"N-400 filed on {filingDate:yyyy-MM-dd}: {version.DisplayName} (version {version.Id})");
        Console.WriteLine($"  Bank: {version.BankSize} questions. Asked: up to {version.Standard.QuestionsAsked}. Pass: {version.Standard.PassingAnswers} correct. Stop: {version.Standard.FailingAnswers} incorrect.");
        Console.WriteLine($"  65/20 consideration: up to {version.SeniorConsideration.QuestionsAsked} questions, {version.SeniorConsideration.PassingAnswers} to pass" +
                          (version.HasSeniorDesignation ? $", from {version.SeniorQuestionNumbers.Count} designated questions." : ". Designated question list not yet recorded."));
        Console.WriteLine($"  Review status: {version.ReviewStatus.ToKebabCase()}. Sources: {string.Join("; ", version.Sources.Select(s => s.Url))}");
        return 0;
    }

    /// <summary>Runs a practice sitting in the terminal.</summary>
    public static async Task<int> SimulateAsync(CommandLineArguments arguments)
    {
        var repository = new ContentRepository(new FileContentStore(ConsoleOutput.ContentRoot(arguments)));
        var versions = await repository.GetExamVersionsAsync();

        var requested = arguments.Option("version");
        var version = requested is null
            ? versions.FirstOrDefault(v => v.IsCurrent) ?? versions[0]
            : versions.FirstOrDefault(v => string.Equals(v.Id, requested, StringComparison.OrdinalIgnoreCase))
              ?? throw new CommandException($"Unknown exam version '{requested}'. Known: {string.Join(", ", versions.Select(v => v.Id))}.");

        var senior = arguments.HasFlag("senior");
        var seedText = arguments.Option("seed");
        var random = seedText is null ? Random.Shared : new Random(int.Parse(seedText, CultureInfo.InvariantCulture));

        var bank = await repository.GetQuestionBankAsync(version.Id);
        var dynamicAnswers = await repository.GetDynamicAnswersAsync();

        ExamSession session;
        try
        {
            session = ExamSession.Start(version, bank, senior, random);
        }
        catch (InvalidOperationException ex)
        {
            throw new CommandException(ex.Message, 1);
        }

        Console.WriteLine();
        Console.WriteLine($"{version.DisplayName}{(senior ? " — 65/20 special consideration" : string.Empty)}");
        Console.WriteLine($"Up to {session.Rules.QuestionsAsked} questions. {session.Rules.PassingAnswers} correct to pass; {session.Rules.FailingAnswers} incorrect ends the test.");
        if (bank.ReviewStatus != ReviewStatus.Approved)
        {
            ConsoleOutput.Warning($"This bank is marked '{bank.ReviewStatus.ToKebabCase()}': it has not yet been verified against the official document.");
        }

        Console.WriteLine("Type your answer and press Enter. Type 'quit' to stop.");
        Console.WriteLine();

        while (session.CurrentQuestion is { } question)
        {
            Console.WriteLine($"Question {session.Position + 1} (#{question.Number}, {question.Subcategory})");
            Console.WriteLine($"  {question.Prompt}");
            Console.Write("> ");
            var response = Console.ReadLine();
            if (response is null || response.Trim().Equals("quit", StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine("Stopped.");
                return 0;
            }

            var answers = question.ResolveAnswers(dynamicAnswers);
            bool correct;

            if (answers.Count == 0)
            {
                var hint = question.DynamicAnswerKey is { } key && dynamicAnswers.TryGetValue(key, out var dynamic) ? dynamic.LookupHint : null;
                ConsoleOutput.Warning("  The answer depends on where you live, so Citiz cannot check it." + (hint is null ? string.Empty : $" {hint}"));
                Console.Write("  Was your answer correct? [y/N] ");
                correct = Console.ReadLine()?.Trim().StartsWith('y') == true;
            }
            else
            {
                var match = AnswerMatcher.Evaluate(response, answers);
                correct = match.IsAccepted;
                if (correct)
                {
                    ConsoleOutput.Success($"  Correct — matched \"{match.MatchedAnswer}\".");
                }
                else if (match.Kind == AnswerMatchKind.Close)
                {
                    ConsoleOutput.Warning($"  Close to \"{match.MatchedAnswer}\", but not accepted as typed.");
                }
                else
                {
                    ConsoleOutput.Error("  Not an accepted answer.");
                }

                Console.WriteLine($"  Accepted answers: {string.Join(" | ", answers)}");
                if (question.Note is not null)
                {
                    ConsoleOutput.Muted($"  Note: {question.Note}");
                }
            }

            session.Record(correct, response);
            Console.WriteLine($"  Score: {session.Correct} correct, {session.Incorrect} incorrect.");
            Console.WriteLine();
        }

        Console.WriteLine(session.Outcome switch
        {
            ExamOutcome.Passed => $"PASSED after {session.Position} question(s): {session.Correct} correct.",
            ExamOutcome.Failed => $"NOT PASSED after {session.Position} question(s): {session.Incorrect} incorrect.",
            _ => $"Ended after {session.Position} question(s).",
        });
        Console.WriteLine("This is practice. It is not a USCIS result and does not predict one.");
        return 0;
    }
}
