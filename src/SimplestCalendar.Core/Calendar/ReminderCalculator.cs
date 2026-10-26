using SimplestCalendar.Core.Models;

namespace SimplestCalendar.Core.Calendar;

public static class ReminderCalculator
{
    public static DateTime? GetNotifyTime(Entry entry)
    {
        if (!entry.ReminderEnabled)
            return null;

        var lead = TimeSpan.FromMinutes(Math.Clamp(entry.ReminderLeadMinutes, 1, 60));
        if (entry.StartTime is TimeOnly start)
        {
            var at = entry.Date.ToDateTime(start);
            return at - lead;
        }

        // Untimed: local midnight of that day minus lead (previous evening).
        var midnight = entry.Date.ToDateTime(TimeOnly.MinValue);
        return midnight - lead;
    }

    public static bool ShouldFire(Entry entry, DateTime now) =>
        GetNotifyTime(entry) is DateTime due && due <= now;

    public static bool ShouldSkipAsMissed(Entry entry, DateTime processStartUtcOrLocal, DateTime now)
    {
        // Missed while process was down: notify time already passed before this session's "catch-up" window.
        // Spec: silent skip — if notify time is before process start, skip.
        if (GetNotifyTime(entry) is not DateTime due)
            return true;
        return due < processStartUtcOrLocal && due < now;
    }
}
