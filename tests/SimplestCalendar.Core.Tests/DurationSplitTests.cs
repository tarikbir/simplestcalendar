using SimplestCalendar.Core.Calendar;

namespace SimplestCalendar.Core.Tests;

public class DurationSplitTests
{
    [Fact]
    public void Same_day_duration_single_entry()
    {
        var result = DurationSplit.CreateTimedEntry(
            new DateOnly(2026, 10, 25), new TimeOnly(18, 0), 120, "Work", false, true, 5);

        Assert.Single(result);
        Assert.Equal(new TimeOnly(20, 0), result[0].EndTime);
        Assert.Equal(120, result[0].DurationMinutes);
        Assert.True(result[0].ReminderEnabled);
    }

    [Fact]
    public void Same_day_with_minutes()
    {
        var result = DurationSplit.CreateTimedEntry(
            new DateOnly(2026, 10, 25), new TimeOnly(18, 0), 135, "Work", false, false, 5);

        Assert.Single(result);
        Assert.Equal(new TimeOnly(20, 15), result[0].EndTime);
    }

    [Fact]
    public void Midnight_crossing_creates_two_segments_reminder_on_first_only()
    {
        var result = DurationSplit.CreateTimedEntry(
            new DateOnly(2026, 10, 25), new TimeOnly(23, 0), 180, "Late", true, true, 5);

        Assert.Equal(2, result.Count);
        Assert.Equal(new DateOnly(2026, 10, 25), result[0].Date);
        Assert.Equal(TimeOnly.MinValue, result[0].EndTime);
        Assert.Equal(60, result[0].DurationMinutes);
        Assert.True(result[0].ReminderEnabled);
        Assert.Equal(new DateOnly(2026, 10, 26), result[1].Date);
        Assert.Equal(new TimeOnly(0, 0), result[1].StartTime);
        Assert.Equal(new TimeOnly(2, 0), result[1].EndTime);
        Assert.Equal(120, result[1].DurationMinutes);
        Assert.False(result[1].ReminderEnabled);
        Assert.Equal(result[0].SplitGroupId, result[1].SplitGroupId);
        Assert.True(result[1].IsStarred);
    }

    [Fact]
    public void Non_aligned_start_preserves_minutes_across_midnight()
    {
        var result = DurationSplit.CreateTimedEntry(
            new DateOnly(2026, 10, 25), new TimeOnly(23, 30), 60, "Late", false, false, 5);

        Assert.Equal(2, result.Count);
        Assert.Equal(30, result[0].DurationMinutes);
        Assert.Equal(TimeOnly.MinValue, result[0].EndTime);
        Assert.Equal(new TimeOnly(0, 30), result[1].EndTime);
        Assert.Equal(30, result[1].DurationMinutes);
    }
}
