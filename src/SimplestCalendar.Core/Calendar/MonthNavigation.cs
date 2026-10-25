namespace SimplestCalendar.Core.Calendar;

public static class MonthNavigation
{
    public const int MaxMonthsAhead = 11;

    public static DateOnly CurrentMonthStart(DateOnly today) =>
        new(today.Year, today.Month, 1);

    public static bool CanGoNext(DateOnly viewedMonthStart, DateOnly today)
    {
        var current = CurrentMonthStart(today);
        var max = current.AddMonths(MaxMonthsAhead);
        var next = viewedMonthStart.AddMonths(1);
        return next <= max;
    }

    public static bool CanGoPrevious(DateOnly viewedMonthStart, DateOnly today)
    {
        // Past months are not navigable; also cannot go before current month.
        var current = CurrentMonthStart(today);
        return viewedMonthStart > current;
    }

    public static DateOnly Clamp(DateOnly viewedMonthStart, DateOnly today)
    {
        var current = CurrentMonthStart(today);
        var max = current.AddMonths(MaxMonthsAhead);
        var month = new DateOnly(viewedMonthStart.Year, viewedMonthStart.Month, 1);
        if (month < current) return current;
        if (month > max) return max;
        return month;
    }

    public static DateOnly Next(DateOnly viewedMonthStart, DateOnly today)
    {
        if (!CanGoNext(viewedMonthStart, today))
            return Clamp(viewedMonthStart, today);
        return viewedMonthStart.AddMonths(1);
    }

    public static DateOnly Previous(DateOnly viewedMonthStart, DateOnly today)
    {
        if (!CanGoPrevious(viewedMonthStart, today))
            return Clamp(viewedMonthStart, today);
        return viewedMonthStart.AddMonths(-1);
    }
}
