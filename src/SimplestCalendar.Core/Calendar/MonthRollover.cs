namespace SimplestCalendar.Core.Calendar;

public static class MonthRollover
{
    public static bool HasRolledOver(DateOnly lastKnownMonthStart, DateOnly today) =>
        MonthNavigation.CurrentMonthStart(today) > lastKnownMonthStart;

    public static DateOnly CurrentMonthStart(DateOnly today) =>
        MonthNavigation.CurrentMonthStart(today);
}
