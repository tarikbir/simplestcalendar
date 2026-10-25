using SimplestCalendar.Core.Models;

namespace SimplestCalendar.Core.Calendar;

/// <summary>
/// In-process due detection. Missed notify times before process start are skipped (spec).
/// </summary>
public sealed class ReminderScheduler
{
    private readonly HashSet<Guid> _fired = [];

    public IReadOnlyList<Entry> CollectDue(
        IEnumerable<Entry> entries,
        DateTime now,
        DateTime processStartedAt)
    {
        var due = new List<Entry>();
        foreach (var entry in entries)
        {
            if (_fired.Contains(entry.Id))
                continue;
            if (ReminderCalculator.GetNotifyTime(entry) is not DateTime notify)
                continue;
            if (notify < processStartedAt)
                continue;
            if (notify <= now)
                due.Add(entry);
        }

        return due;
    }

    public void MarkFired(Guid entryId) => _fired.Add(entryId);

    public DateTime? GetNextNotifyTime(IEnumerable<Entry> entries, DateTime now, DateTime processStartedAt)
    {
        DateTime? next = null;
        foreach (var entry in entries)
        {
            if (_fired.Contains(entry.Id))
                continue;
            if (ReminderCalculator.GetNotifyTime(entry) is not DateTime notify)
                continue;
            if (notify < processStartedAt)
                continue;
            if (notify <= now)
                continue;
            if (next is null || notify < next)
                next = notify;
        }

        return next;
    }
}
