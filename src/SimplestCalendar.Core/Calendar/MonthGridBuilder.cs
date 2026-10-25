namespace SimplestCalendar.Core.Calendar;

public sealed record DayCell(DateOnly Date, bool IsInViewedMonth, bool IsToday);

public static class MonthGridBuilder
{
    public static IReadOnlyList<DayCell> Build(DateOnly monthStart, DayOfWeek firstDayOfWeek, DateOnly today)
    {
        monthStart = new DateOnly(monthStart.Year, monthStart.Month, 1);
        var daysInMonth = DateTime.DaysInMonth(monthStart.Year, monthStart.Month);
        var firstOfMonth = monthStart;
        var offset = ((int)firstOfMonth.DayOfWeek - (int)firstDayOfWeek + 7) % 7;
        var gridStart = firstOfMonth.AddDays(-offset);
        var cells = new List<DayCell>(42);
        for (var i = 0; i < 42; i++)
        {
            var date = gridStart.AddDays(i);
            cells.Add(new DayCell(
                date,
                date.Month == monthStart.Month && date.Year == monthStart.Year,
                date == today));
        }

        // Drop trailing week if entirely outside month (optional density); keep 6 rows for stable layout.
        _ = daysInMonth;
        return cells;
    }

    public static IReadOnlyList<string> WeekdayHeaders(DayOfWeek firstDayOfWeek, System.Globalization.CultureInfo? culture = null)
    {
        culture ??= new System.Globalization.CultureInfo("en-US");
        var names = new string[7];
        for (var i = 0; i < 7; i++)
        {
            var day = (DayOfWeek)(((int)firstDayOfWeek + i) % 7);
            names[i] = culture.DateTimeFormat.GetAbbreviatedDayName(day);
        }

        return names;
    }
}
