using SimplestCalendar.Core.Calendar;
using SimplestCalendar.Core.Models;

namespace SimplestCalendar.Core.Tests;

public class OverlapTests
{
    [Fact]
    public void Untimed_never_overlaps()
    {
        var existing = new Entry
        {
            Note = "a",
            StartTime = new TimeOnly(10, 0),
            EndTime = new TimeOnly(12, 0),
            DurationHours = 2
        };
        var candidate = new Entry { Note = "b", StartTime = null };
        Assert.False(OverlapDetector.HasOverlap(candidate, [existing]));
    }

    [Fact]
    public void Adjacent_ranges_do_not_overlap()
    {
        var existing = new Entry
        {
            Note = "a",
            StartTime = new TimeOnly(10, 0),
            EndTime = new TimeOnly(12, 0),
            DurationHours = 2
        };
        var candidate = new Entry
        {
            Note = "b",
            StartTime = new TimeOnly(12, 0),
            EndTime = new TimeOnly(13, 0),
            DurationHours = 1
        };
        Assert.False(OverlapDetector.HasOverlap(candidate, [existing]));
    }
}
