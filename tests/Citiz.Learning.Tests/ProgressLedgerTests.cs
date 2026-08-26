namespace Citiz.Learning.Tests;

public sealed class ProgressLedgerTests
{
    private static readonly DateTimeOffset Day0 = new(2026, 8, 25, 12, 0, 0, TimeSpan.Zero);

    [Theory]
    [InlineData(0, 1)]
    [InlineData(1, 2)]
    [InlineData(2, 4)]
    [InlineData(3, 8)]
    [InlineData(4, 14)]
    [InlineData(9, 14)]
    public void Interval_grows_with_the_streak_and_caps(int streak, int days) =>
        Assert.Equal(TimeSpan.FromDays(days), ReviewScheduler.IntervalFor(streak));

    [Fact]
    public void A_miss_resets_the_streak_and_brings_the_item_back_tomorrow()
    {
        var ledger = new ProgressLedger();

        ledger.Record("2025-002", correct: true, Day0);
        ledger.Record("2025-002", correct: true, Day0.AddDays(2));
        var after = ledger.Record("2025-002", correct: false, Day0.AddDays(6));

        Assert.Equal(3, after.Attempts);
        Assert.Equal(2, after.Correct);
        Assert.Equal(0, after.Streak);
        Assert.Equal(Day0.AddDays(7), after.NextReviewAt);
        Assert.False(after.IsMastered);
    }

    [Fact]
    public void Three_in_a_row_is_mastery()
    {
        var ledger = new ProgressLedger();

        ledger.Record("q", true, Day0);
        ledger.Record("q", true, Day0.AddDays(2));
        var third = ledger.Record("q", true, Day0.AddDays(6));

        Assert.True(third.IsMastered);
        Assert.Equal(1, ledger.MasteredCount);
        Assert.Equal(Day0.AddDays(14), third.NextReviewAt);
    }

    [Fact]
    public void Due_and_weakest_are_ordered_usefully()
    {
        var ledger = new ProgressLedger();
        ledger.Record("late", false, Day0.AddDays(-10));
        ledger.Record("soon", true, Day0.AddDays(-3));
        ledger.Record("future", true, Day0);
        ledger.Record("strong", true, Day0);
        ledger.Record("strong", true, Day0);
        ledger.Record("strong", true, Day0);

        var due = ledger.Due(Day0);
        var weakest = ledger.Weakest(2);

        Assert.Equal(["late", "soon"], due.Select(i => i.ItemId));
        Assert.Equal("late", weakest[0].ItemId);
        Assert.DoesNotContain(weakest, i => i.ItemId == "strong");
    }

    [Fact]
    public void Snapshot_round_trips_through_json()
    {
        var ledger = new ProgressLedger();
        ledger.Record("a", true, Day0);
        ledger.Record("b", false, Day0);

        var json = ledger.ToSnapshot().ToJson();
        var restored = new ProgressLedger(ProgressSnapshot.FromJson(json));

        Assert.Equal(2, restored.SeenCount);
        Assert.Equal(ledger.Get("a"), restored.Get("a"));
        Assert.Equal(ledger.Get("b"), restored.Get("b"));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("not json")]
    [InlineData("""{ "version": 99, "items": [] }""")]
    public void Unreadable_snapshots_become_an_empty_ledger_rather_than_an_exception(string? json)
    {
        var snapshot = ProgressSnapshot.FromJson(json);

        Assert.Empty(snapshot.Items);
        Assert.Equal(0, new ProgressLedger(snapshot).SeenCount);
    }

    [Fact]
    public void Clear_forgets_everything()
    {
        var ledger = new ProgressLedger();
        ledger.Record("a", true, Day0);

        ledger.Clear();

        Assert.Equal(0, ledger.SeenCount);
        Assert.Null(ledger.Get("a"));
    }
}
