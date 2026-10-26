using SimplestCalendar.Core.Calendar;
using SimplestCalendar.Core.Models;

namespace SimplestCalendar.Core.Services;

public sealed record EntryDraft(
    DateOnly Date,
    string Note,
    TimeOnly? StartTime,
    int? DurationMinutes,
    bool IsStarred,
    bool ReminderEnabled,
    int ReminderLeadMinutes);

public static class EntryWorkflow
{
    public static string? Validate(EntryDraft draft)
    {
        if (string.IsNullOrWhiteSpace(draft.Note))
            return "Note is required.";
        if (draft.DurationMinutes is int minutes)
        {
            if (draft.StartTime is null)
                return "Duration requires a start time.";
            if (minutes < 1)
                return "Duration must be at least 1 minute.";
        }

        if (draft.ReminderEnabled && (draft.ReminderLeadMinutes < 1 || draft.ReminderLeadMinutes > 60))
            return "Lead minutes must be between 1 and 60.";

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
                    ReminderLeadMinutes = ClampLeadMinutes(draft.ReminderLeadMinutes),
                    CreatedAt = stamp,
                    UpdatedAt = stamp
                }
            ];
        }

        if (draft.DurationMinutes is int durationMinutes)
        {
            return DurationSplit.CreateTimedEntry(
                draft.Date,
                draft.StartTime.Value,
                durationMinutes,
                draft.Note.Trim(),
                draft.IsStarred,
                draft.ReminderEnabled,
                ClampLeadMinutes(draft.ReminderLeadMinutes),
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
                ReminderLeadMinutes = ClampLeadMinutes(draft.ReminderLeadMinutes),
                CreatedAt = stamp,
                UpdatedAt = stamp
            }
        ];
    }

    /// <summary>
    /// Removes the edited entry only, then inserts materialized drafts.
    /// Split siblings are left alone (independent entities).
    /// </summary>
    public static IReadOnlyList<Entry> Save(
        IEntryStore store,
        EntryDraft draft,
        Entry? editing,
        out bool hasOverlapWarning)
    {
        hasOverlapWarning = false;
        var created = Materialize(draft);

        var removeIds = new HashSet<Guid>();
        if (editing is not null)
            removeIds.Add(editing.Id);

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

    private static int ClampLeadMinutes(int leadMinutes) =>
        leadMinutes > 0 ? Math.Clamp(leadMinutes, 1, 60) : 5;

    public static void Delete(IEntryStore store, Entry entry) => store.Remove(entry.Id);

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
