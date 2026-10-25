using SimplestCalendar.Core.Calendar;
using SimplestCalendar.Core.Models;

namespace SimplestCalendar.Core.Services;

public sealed record EntryDraft(
    DateOnly Date,
    string Note,
    TimeOnly? StartTime,
    int? DurationHours,
    bool IsStarred,
    bool ReminderEnabled,
    int ReminderLeadMinutes);

public static class EntryWorkflow
{
    public static string? Validate(EntryDraft draft)
    {
        if (string.IsNullOrWhiteSpace(draft.Note))
            return "Note is required.";
        if (draft.DurationHours is int hours)
        {
            if (draft.StartTime is null)
                return "Duration requires a start time.";
            if (hours < 1)
                return "Duration must be at least 1 hour.";
        }

        return null;
    }

    public static IReadOnlyList<Entry> Materialize(EntryDraft draft, DateTime? now = null)
    {
        var error = Validate(draft);
        if (error is not null)
            throw new InvalidOperationException(error);

        var stamp = now ?? DateTime.Now;
        if (draft.StartTime is null)
        {
            return
            [
                new Entry
                {
                    Date = draft.Date,
                    Note = draft.Note.Trim(),
                    IsStarred = draft.IsStarred,
                    ReminderEnabled = draft.ReminderEnabled,
                    ReminderLeadMinutes = draft.ReminderLeadMinutes > 0 ? draft.ReminderLeadMinutes : 5,
                    CreatedAt = stamp,
                    UpdatedAt = stamp
                }
            ];
        }

        if (draft.DurationHours is int durationHours)
        {
            return DurationSplit.CreateTimedEntry(
                draft.Date,
                draft.StartTime.Value,
                durationHours,
                draft.Note.Trim(),
                draft.IsStarred,
                draft.ReminderEnabled,
                draft.ReminderLeadMinutes > 0 ? draft.ReminderLeadMinutes : 5,
                stamp);
        }

        return
        [
            new Entry
            {
                Date = draft.Date,
                Note = draft.Note.Trim(),
                StartTime = draft.StartTime,
                IsStarred = draft.IsStarred,
                ReminderEnabled = draft.ReminderEnabled,
                ReminderLeadMinutes = draft.ReminderLeadMinutes > 0 ? draft.ReminderLeadMinutes : 5,
                CreatedAt = stamp,
                UpdatedAt = stamp
            }
        ];
    }

    /// <summary>
    /// Removes the edited entry (and its split group if any), then inserts materialized drafts.
    /// </summary>
    public static IReadOnlyList<Entry> Save(
        IEntryStore store,
        EntryDraft draft,
        Entry? editing,
        out bool hasOverlapWarning)
    {
        hasOverlapWarning = false;
        var created = Materialize(draft);

        // Detect overlap against same-day timed entries excluding the ones we will remove.
        var removeIds = new HashSet<Guid>();
        if (editing is not null)
        {
            if (editing.SplitGroupId is Guid group)
            {
                foreach (var e in store.Entries.Where(x => x.SplitGroupId == group))
                    removeIds.Add(e.Id);
            }
            else
            {
                removeIds.Add(editing.Id);
            }
        }

        foreach (var piece in created)
        {
            var sameDay = store.Entries.Where(e => e.Date == piece.Date && !removeIds.Contains(e.Id));
            if (OverlapDetector.HasOverlap(piece, sameDay))
                hasOverlapWarning = true;
        }

        foreach (var id in removeIds)
            store.Remove(id);

        foreach (var piece in created)
            store.Upsert(piece);

        return created;
    }

    public static void Delete(IEntryStore store, Entry entry)
    {
        if (entry.SplitGroupId is Guid group)
        {
            foreach (var id in store.Entries.Where(e => e.SplitGroupId == group).Select(e => e.Id).ToList())
                store.Remove(id);
        }
        else
        {
            store.Remove(entry.Id);
        }
    }

    public static string FormatListLabel(Entry e)
    {
        var star = e.IsStarred ? "★ " : "";
        if (e.StartTime is null)
            return $"{star}{e.Note}";
        if (e.EndTime is TimeOnly end)
        {
            var endText = end == TimeOnly.MinValue ? "00:00" : end.ToString("HH:mm");
            return $"{star}{e.StartTime.Value:HH:mm}-{endText}  {e.Note}";
        }

        return $"{star}{e.StartTime.Value:HH:mm}  {e.Note}";
    }
}
