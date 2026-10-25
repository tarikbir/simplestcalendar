using SimplestCalendar.Core.Calendar;

namespace SimplestCalendar.Core.Tests;

public class MonthTitleFormatterTests
{
    [Fact]
    public void Default_MMMM_is_month_name_only()
    {
        var title = MonthTitleFormatter.Format(new DateOnly(2026, 10, 1), "MMMM");
        Assert.Equal("October", title);
    }

    [Fact]
    public void Supports_combined_tokens()
    {
        var title = MonthTitleFormatter.Format(new DateOnly(2026, 10, 1), "MMM yy");
        Assert.Equal("Oct 26", title);
    }
}
