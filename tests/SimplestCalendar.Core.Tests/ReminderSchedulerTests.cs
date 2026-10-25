using SimplestCalendar.Core.Calendar;
using SimplestCalendar.Core.Models;

namespace SimplestCalendar.Core.Tests;

public class ReminderSchedulerTests
{
    [Fact]
    public void Skips_missed_before_process_start_and_fires_once()
    {
        var scheduler = new ReminderScheduler();
        var processStart = new DateTime(2026, 10, 25, 10, 0, 0);
        var entry = new Entry
        {
            Id = Guid.NewGuid(),
            Date = new DateOnly(2026, 10, 25),
            Note = "x",
            StartTime = new TimeOnly(9, 0),
            ReminderEnabled = true,
            ReminderLeadMinutes = 5
        };

        // Notify was 08:55 — before process start → skip
        Assert.Empty(scheduler.CollectDue([entry], processStart.AddMinutes(1), processStart));

        var live = new Entry
        {
            Id = Guid.NewGuid(),
            Date = new DateOnly(2026, 10, 25),
            Note = "y",
            StartTime = new TimeOnly(10, 10),
            ReminderEnabled = true,
            ReminderLeadMinutes = 5
        };
        var now = new DateTime(2026, 10, 25, 10, 6, 0); // notify 10:05
        var due = scheduler.CollectDue([live], now, processStart);
        Assert.Single(due);
        scheduler.MarkFired(live.Id);
        Assert.Empty(scheduler.CollectDue([live], now.AddMinutes(1), processStart));
    }
}
