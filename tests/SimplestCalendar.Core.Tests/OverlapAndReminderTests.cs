using SimplestCalendar.Core.Calendar;
using SimplestCalendar.Core.Models;

namespace SimplestCalendar.Core.Tests;

public class OverlapAndReminderTests
{
    [Fact]
    public void Detects_timed_overlap()
    {
        var existing = new Entry
        {
            Date = new DateOnly(2026, 10, 25),
            Note = "a",
            StartTime = new TimeOnly(10, 0),
            EndTime = new TimeOnly(12, 0),
            DurationHours = 2
        };
        var candidate = new Entry
        {
            Date = new DateOnly(2026, 10, 25),
            Note = "b",
            StartTime = new TimeOnly(11, 0),
            EndTime = new TimeOnly(13, 0),
            DurationHours = 2
        };
        Assert.True(OverlapDetector.HasOverlap(candidate, [existing]));
    }

    [Fact]
    public void Timed_reminder_is_start_minus_lead()
    {
        var entry = new Entry
        {
            Date = new DateOnly(2026, 10, 25),
            Note = "x",
            StartTime = new TimeOnly(10, 0),
            ReminderEnabled = true,
            ReminderLeadMinutes = 5
        };
        Assert.Equal(new DateTime(2026, 10, 25, 9, 55, 0), ReminderCalculator.GetNotifyTime(entry));
    }

    [Fact]
    public void Untimed_reminder_is_midnight_minus_lead()
    {
        var entry = new Entry
        {
            Date = new DateOnly(2026, 10, 25),
            Note = "x",
            ReminderEnabled = true,
            ReminderLeadMinutes = 5
        };
        Assert.Equal(new DateTime(2026, 10, 24, 23, 55, 0), ReminderCalculator.GetNotifyTime(entry));
    }
}
