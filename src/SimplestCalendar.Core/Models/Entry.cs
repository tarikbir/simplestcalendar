namespace SimplestCalendar.Core.Models;

public sealed class Entry
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateOnly Date { get; set; }
    public string Note { get; set; } = string.Empty;
    public TimeOnly? StartTime { get; set; }
    public int? DurationHours { get; set; }
    public TimeOnly? EndTime { get; set; }
    public bool IsStarred { get; set; }
    public bool ReminderEnabled { get; set; }
    public int ReminderLeadMinutes { get; set; } = 5;
    public Guid? SplitGroupId { get; set; }
    public int? SplitIndex { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime UpdatedAt { get; set; } = DateTime.Now;

    public bool IsUntimed => StartTime is null;
}
