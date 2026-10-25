using SimplestCalendar.Core.Models;

namespace SimplestCalendar.Core.Sorting;

/// <summary>
/// Sort: starred first; within group untimed before timed; timed by start ascending;
/// then CreatedAt, then Id. See decisions.md / data-model.md.
/// </summary>
public static class EntrySort
{
    public static IOrderedEnumerable<Entry> Order(IEnumerable<Entry> entries) =>
        entries
            .OrderByDescending(e => e.IsStarred)
            .ThenBy(e => e.IsUntimed ? 0 : 1)
            .ThenBy(e => e.StartTime ?? TimeOnly.MinValue)
            .ThenBy(e => e.CreatedAt)
            .ThenBy(e => e.Id);

    public static List<Entry> OrderToList(IEnumerable<Entry> entries) =>
        Order(entries).ToList();
}
