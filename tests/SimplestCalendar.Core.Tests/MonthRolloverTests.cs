using SimplestCalendar.Core.Calendar;

namespace SimplestCalendar.Core.Tests;

public class MonthRolloverTests
{
    [Fact]
    public void Detects_month_change()
    {
        Assert.False(MonthRollover.HasRolledOver(new DateOnly(2026, 10, 1), new DateOnly(2026, 10, 31)));
        Assert.True(MonthRollover.HasRolledOver(new DateOnly(2026, 10, 1), new DateOnly(2026, 11, 1)));
    }
}
