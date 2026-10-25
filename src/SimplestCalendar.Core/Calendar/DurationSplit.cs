using SimplestCalendar.Core.Models;

namespace SimplestCalendar.Core.Calendar;

public static class DurationSplit
{
    /// <summary>
    /// Creates one or two day-bound entries for a timed duration in minutes.
    /// Example: 23:30 + 60m → 23:30–00:00 (day D) and 00:00–00:30 (day D+1).
    /// Reminder stays only on the first segment when split.
    /// </summary>
    public static IReadOnlyList<Entry> CreateTimedEntry(
        DateOnly date,
        TimeOnly start,
        int durationMinutes,
        string note,
        bool isStarred,
        bool reminderEnabled,
        int reminderLeadMinutes,
        DateTime? now = null)
    {
        if (durationMinutes < 1)
            throw new ArgumentOutOfRangeException(nameof(durationMinutes), "Duration must be at least 1 minute.");
        if (string.IsNullOrWhiteSpace(note))
            throw new ArgumentException("Note is required.", nameof(note));

        var stamp = now ?? DateTime.Now;
        var startMinutes = start.Hour * 60 + start.Minute;
        var minutesUntilMidnight = (24 * 60) - startMinutes;

        if (durationMinutes <= minutesUntilMidnight)
        {
            return
            [
                new Entry
                {
                    Date = date,
                    Note = note,
                    StartTime = start,
                    DurationMinutes = durationMinutes,
                    EndTime = start.Add(TimeSpan.FromMinutes(durationMinutes)),
                    IsStarred = isStarred,
                    ReminderEnabled = reminderEnabled,
                    ReminderLeadMinutes = reminderLeadMinutes,
                    CreatedAt = stamp,
                    UpdatedAt = stamp
                }
            ];
        }

        var firstMinutes = minutesUntilMidnight;
        var remainingMinutes = durationMinutes - firstMinutes;
        var groupId = Guid.NewGuid();

        var first = new Entry
        {
            Date = date,
            Note = note,
            StartTime = start,
            DurationMinutes = firstMinutes,
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
            DurationMinutes = remainingMinutes,
            EndTime = TimeOnly.MinValue.Add(TimeSpan.FromMinutes(remainingMinutes)),
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
