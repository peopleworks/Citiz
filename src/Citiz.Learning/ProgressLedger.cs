namespace Citiz.Learning;

/// <summary>
/// The learner's progress across every item, in memory. Hosts persist it through
/// <see cref="ToSnapshot"/> and restore it through the snapshot constructor; in the browser that is
/// <c>localStorage</c>, and nothing leaves the device.
/// </summary>
public sealed class ProgressLedger
{
    private readonly Dictionary<string, ItemProgress> _items = new(StringComparer.Ordinal);

    /// <summary>Creates an empty ledger.</summary>
    public ProgressLedger()
    {
    }

    /// <summary>Restores a ledger from a snapshot.</summary>
    public ProgressLedger(ProgressSnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        foreach (var item in snapshot.Items)
        {
            _items[item.ItemId] = item;
        }
    }

    /// <summary>Every item practised at least once.</summary>
    public IReadOnlyCollection<ItemProgress> Items => _items.Values;

    /// <summary>Items practised at least once.</summary>
    public int SeenCount => _items.Count;

    /// <summary>Items answered correctly <see cref="ItemProgress.MasteryStreak"/> times in a row.</summary>
    public int MasteredCount => _items.Values.Count(i => i.IsMastered);

    /// <summary>Progress for one item, or <c>null</c> if never practised.</summary>
    public ItemProgress? Get(string itemId) => _items.GetValueOrDefault(itemId);

    /// <summary>Records one attempt and reschedules the item.</summary>
    public ItemProgress Record(string itemId, bool correct, DateTimeOffset now)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(itemId);

        var previous = _items.GetValueOrDefault(itemId);
        var streak = correct ? (previous?.Streak ?? 0) + 1 : 0;

        var updated = new ItemProgress(
            itemId,
            (previous?.Attempts ?? 0) + 1,
            (previous?.Correct ?? 0) + (correct ? 1 : 0),
            streak,
            now,
            ReviewScheduler.Next(now, streak));

        _items[itemId] = updated;
        return updated;
    }

    /// <summary>Items due for review at <paramref name="now"/>, most overdue first.</summary>
    public IReadOnlyList<ItemProgress> Due(DateTimeOffset now) =>
        _items.Values.Where(i => i.IsDue(now)).OrderBy(i => i.NextReviewAt).ToList();

    /// <summary>Items the learner has missed more often than not, weakest first.</summary>
    public IReadOnlyList<ItemProgress> Weakest(int count) =>
        _items.Values.Where(i => i.Attempts > 0 && !i.IsMastered).OrderBy(i => i.Accuracy).ThenByDescending(i => i.Attempts).Take(count).ToList();

    /// <summary>Forgets everything. The learner owns their data; this is the delete button.</summary>
    public void Clear() => _items.Clear();

    /// <summary>A serializable copy of the ledger.</summary>
    public ProgressSnapshot ToSnapshot() => new(ProgressSnapshot.CurrentVersion, _items.Values.OrderBy(i => i.ItemId, StringComparer.Ordinal).ToList());
}
