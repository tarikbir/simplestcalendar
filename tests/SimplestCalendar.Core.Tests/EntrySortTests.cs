using SimplestCalendar.Core.Models;
using SimplestCalendar.Core.Sorting;

namespace SimplestCalendar.Core.Tests;

public class EntrySortTests
{
    [Fact]
    public void Starred_then_untimed_then_timed_by_start()
    {
        var a = new Entry { Note = "late", StartTime = new TimeOnly(15, 0), IsStarred = false };
        var b = new Entry { Note = "star timed", StartTime = new TimeOnly(10, 0), IsStarred = true };
        var c = new Entry { Note = "untimed", StartTime = null, IsStarred = false };
        var d = new Entry { Note = "star untimed", StartTime = null, IsStarred = true };
        var e = new Entry { Note = "early", StartTime = new TimeOnly(9, 0), IsStarred = false };

        var ordered = EntrySort.OrderToList([a, b, c, d, e]);
        Assert.Equal(["star untimed", "star timed", "untimed", "early", "late"], ordered.Select(x => x.Note));
    }
}
