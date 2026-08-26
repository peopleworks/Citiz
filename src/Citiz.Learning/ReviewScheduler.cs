namespace Citiz.Learning;

/// <summary>
/// Spaced review: the interval before an item comes back grows with the streak of correct answers
/// and collapses on a miss. Deliberately simple and explainable; a learner can be told "you got it
/// right three times, so it comes back in eight days" and it is true.
/// </summary>
public static class ReviewScheduler
{
    private static readonly TimeSpan[] Intervals =
    [
        TimeSpan.FromDays(1),   // streak 0: missed, or never seen
        TimeSpan.FromDays(2),   // streak 1
        TimeSpan.FromDays(4),   // streak 2
        TimeSpan.FromDays(8),   // streak 3: mastered
        TimeSpan.FromDays(14),  // streak 4+
    ];

    /// <summary>The interval before the next review for a given streak.</summary>
    public static TimeSpan IntervalFor(int streak)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(streak);
        return Intervals[Math.Min(streak, Intervals.Length - 1)];
    }

    /// <summary>When an item with the given streak should next be reviewed.</summary>
    public static DateTimeOffset Next(DateTimeOffset now, int streak) => now + IntervalFor(streak);
}
