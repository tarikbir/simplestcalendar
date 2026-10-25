using SimplestCalendar.Core.Models;

namespace SimplestCalendar.Core.Calendar;

public static class OverlapDetector
{
    /// <summary>
    /// Timed ranges on the same date. End at midnight (TimeOnly.MinValue) means end of day exclusive next midnight.
    /// </summary>
    public static bool HasOverlap(Entry candidate, IEnumerable<Entry> existingOnSameDay, Guid? ignoreId = null)
    {
        if (candidate.StartTime is null)
            return false;

        var (cStart, cEnd) = ToRange(candidate);
        foreach (var other in existingOnSameDay)
        {
            if (ignoreId is Guid id && other.Id == id)
                continue;
            if (other.StartTime is null)
                continue;
            var (oStart, oEnd) = ToRange(other);
            if (cStart < oEnd && oStart < cEnd)
                return true;
        }

        return false;
    }

    private static (int StartMinutes, int EndMinutes) ToRange(Entry e)
    {
        var start = e.StartTime!.Value.Hour * 60 + e.StartTime.Value.Minute;
        int end;
        if (e.EndTime is null && e.DurationMinutes is int minutes)
            end = start + minutes;
        else if (e.EndTime is TimeOnly et)
            end = et == TimeOnly.MinValue && e.StartTime != TimeOnly.MinValue
                ? 24 * 60
                : et.Hour * 60 + et.Minute;
        else
            end = start; // zero-length if malformed

        if (end <= start)
            end = 24 * 60;
        return (start, end);
    }
}
