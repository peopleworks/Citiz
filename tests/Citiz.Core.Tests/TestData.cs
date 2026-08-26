using Citiz.Core.Content;
using Citiz.Core.Exams;

namespace Citiz.Core.Tests;

/// <summary>The two real versions and a synthetic bank, so tests read like the rules they check.</summary>
internal static class TestData
{
    public static readonly SourceReference Source = new("USCIS", "Test", new Uri("https://www.uscis.gov/"), null, "Public domain");

    public static readonly ExamVersion V2008 = new(
        "2008",
        "2008 Civics Test",
        null,
        new DateOnly(2025, 10, 19),
        100,
        new ExamAdministrationRules(10, 6, 5),
        new ExamAdministrationRules(10, 6, 5),
        [6, 11, 13, 17, 20, 27, 28, 44, 45, 49, 54, 56, 70, 75, 78, 85, 94, 95, 97, 99],
        ReviewStatus.Approved,
        [Source]);

    public static readonly ExamVersion V2025 = new(
        "2025",
        "2025 Civics Test",
        new DateOnly(2025, 10, 20),
        null,
        128,
        new ExamAdministrationRules(20, 12, 9),
        new ExamAdministrationRules(10, 6, 5),
        [],
        ReviewStatus.Approved,
        [Source]);

    public static readonly ExamVersion[] Versions = [V2008, V2025];

    /// <summary>A bank of <paramref name="size"/> questions for <paramref name="version"/>, numbered 1..size.</summary>
    public static QuestionBank Bank(ExamVersion version, int size) =>
        new(
            version.Id,
            Enumerable.Range(1, size).Select(n => new CivicsQuestion(
                $"{version.Id}-{n:000}",
                version.Id,
                n,
                n <= size / 2 ? "American Government" : "American History",
                n <= size / 2 ? "System of Government" : "1800s",
                $"Question {n}?",
                [$"Answer {n}", $"Alternative {n}"])).ToList(),
            ReviewStatus.Approved,
            [Source]);
}
