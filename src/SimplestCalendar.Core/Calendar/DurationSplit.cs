using SimplestCalendar.Core.Models;

namespace SimplestCalendar.Core.Calendar;

public static class DurationSplit
{
    /// <summary>
    /// Creates one or two day-bound entries for a timed duration in whole hours.
    /// Example: 23:00 + 3h → 23:00–00:00 (day D) and 00:00–02:00 (day D+1).
    /// Reminder stays only on the first segment when split.
    /// </summary>
    public static IReadOnlyList<Entry> CreateTimedEntry(
        DateOnly date,
        TimeOnly start,
        int durationHours,
        string note,
        bool isStarred,
        bool reminderEnabled,
        int reminderLeadMinutes,
        DateTime? now = null)
    {
        if (durationHours < 1)
            throw new ArgumentOutOfRangeException(nameof(durationHours), "Duration must be at least 1 hour.");
        if (string.IsNullOrWhiteSpace(note))
            throw new ArgumentException("Note is required.", nameof(note));

        var stamp = now ?? DateTime.Now;
        var startMinutes = start.Hour * 60 + start.Minute;
        var minutesUntilMidnight = (24 * 60) - startMinutes;
        var durationMinutes = durationHours * 60;

        if (durationMinutes <= minutesUntilMidnight)
        {
            return
            [
                new Entry
                {
                    Date = date,
                    Note = note,
                    StartTime = start,
                    DurationHours = durationHours,
                    EndTime = start.Add(TimeSpan.FromHours(durationHours)),
                    IsStarred = isStarred,
                    ReminderEnabled = reminderEnabled,
                    ReminderLeadMinutes = reminderLeadMinutes,
                    CreatedAt = stamp,
                    UpdatedAt = stamp
                }
            ];
        }

        // First segment: start → midnight (counts as hours = minutesUntilMidnight / 60, at least the wall span).
        var firstHours = (int)Math.Ceiling(minutesUntilMidnight / 60.0);
        var remainingHours = durationHours - (minutesUntilMidnight / 60);
        // Spec whiteboard example uses whole-hour starts (23:00 + 3 → 1h + 2h).
        if (minutesUntilMidnight % 60 == 0)
        {
            firstHours = minutesUntilMidnight / 60;
            remainingHours = durationHours - firstHours;
        }
        else
        {
            // Non-aligned start: consume remaining minutes of the day as first partial hour bucket.
            firstHours = 1;
            remainingHours = durationHours - 1;
        }

        if (remainingHours < 1)
            remainingHours = 1;

        var groupId = Guid.NewGuid();
        var first = new Entry
        {
            Date = date,
            Note = note,
            StartTime = start,
            DurationHours = firstHours,
            EndTime = TimeOnly.MinValue,
            IsStarred = isStarred,
            ReminderEnabled = reminderEnabled,
            ReminderLeadMinutes = reminderLeadMinutes,
            SplitGroupId = groupId,
            SplitIndex = 0,
            CreatedAt = stamp,
            UpdatedAt = stamp
        };

        var second = new Entry
        {
            Date = date.AddDays(1),
            Note = note,
            StartTime = TimeOnly.MinValue,
            DurationHours = remainingHours,
            EndTime = TimeOnly.MinValue.AddHours(remainingHours),
            IsStarred = isStarred,
            ReminderEnabled = false,
            ReminderLeadMinutes = reminderLeadMinutes,
            SplitGroupId = groupId,
            SplitIndex = 1,
            CreatedAt = stamp,
            UpdatedAt = stamp
        };

        return [first, second];
    }
}
