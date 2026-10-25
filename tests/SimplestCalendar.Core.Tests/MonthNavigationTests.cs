using SimplestCalendar.Core.Calendar;

namespace SimplestCalendar.Core.Tests;

public class MonthNavigationTests
{
    private static readonly DateOnly Today = new(2026, 10, 25);

    [Fact]
    public void Cannot_go_past_current_month()
    {
        var current = MonthNavigation.CurrentMonthStart(Today);
        Assert.False(MonthNavigation.CanGoPrevious(current, Today));
        Assert.Equal(current, MonthNavigation.Previous(current, Today));
    }

    [Fact]
    public void Can_go_forward_up_to_11_months_not_12()
    {
        var current = MonthNavigation.CurrentMonthStart(Today);
        var at11 = current.AddMonths(11);
        Assert.True(MonthNavigation.CanGoNext(current.AddMonths(10), Today));
        Assert.False(MonthNavigation.CanGoNext(at11, Today));
        Assert.Equal(at11, MonthNavigation.Clamp(current.AddMonths(12), Today));
    }
}
