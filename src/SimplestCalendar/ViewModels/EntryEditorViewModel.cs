using CommunityToolkit.Mvvm.ComponentModel;
using SimplestCalendar.Core.Models;
using SimplestCalendar.Core.Services;

namespace SimplestCalendar.ViewModels;

public sealed partial class EntryEditorViewModel : ObservableObject
{
    public DateOnly Date { get; }
    public Entry? Editing { get; }
    public bool IsEdit => Editing is not null;

    [ObservableProperty]
    private string _note = "";

    [ObservableProperty]
    private bool _hasStartTime;

    [ObservableProperty]
    private TimeSpan _startTime = TimeSpan.Zero;

    [ObservableProperty]
    private bool _hasDuration;

    [ObservableProperty]
    private int _durationHours = 1;

    [ObservableProperty]
    private int _durationMinutesPart;

    [ObservableProperty]
    private bool _isStarred;

    [ObservableProperty]
    private bool _reminderEnabled;

    [ObservableProperty]
    private int _reminderLeadMinutes = 5;

    [ObservableProperty]
    private string? _validationMessage;

    public string DialogTitle => IsEdit ? $"Edit — {Date:dddd, MMM d}" : $"New — {Date:dddd, MMM d}";

    public EntryEditorViewModel(DateOnly date, Entry? editing = null)
    {
        Date = date;
        Editing = editing;
        if (editing is null)
            return;

        Note = editing.Note;
        HasStartTime = editing.StartTime is not null;
        if (editing.StartTime is TimeOnly start)
            StartTime = new TimeSpan(start.Hour, start.Minute, 0);

        HasDuration = editing.DurationMinutes is > 0;
        if (editing.DurationMinutes is > 0)
        {
            DurationHours = editing.DurationMinutes.Value / 60;
            DurationMinutesPart = editing.DurationMinutes.Value % 60;
            if (DurationHours == 0 && DurationMinutesPart == 0)
            {
                DurationHours = 1;
                DurationMinutesPart = 0;
            }
        }

        IsStarred = editing.IsStarred;
        ReminderEnabled = editing.ReminderEnabled;
        ReminderLeadMinutes = editing.ReminderLeadMinutes > 0 ? editing.ReminderLeadMinutes : 5;
    }

    partial void OnHasDurationChanged(bool value)
    {
        if (value)
            HasStartTime = true;
    }

    public bool TryBuildDraft(out EntryDraft draft)
    {
        draft = default!;
        ValidationMessage = null;

        TimeOnly? start = HasStartTime
            ? new TimeOnly(StartTime.Hours, StartTime.Minutes)
            : null;

        int? duration = null;
        if (HasDuration)
        {
            var total = DurationHours * 60 + DurationMinutesPart;
            duration = total;
        }

        draft = new EntryDraft(
            Date,
            Note ?? "",
            start,
            duration,
            IsStarred,
            ReminderEnabled,
            ReminderLeadMinutes);

        var error = EntryWorkflow.Validate(draft);
        if (error is not null)
        {
            ValidationMessage = error;
            return false;
        }

        return true;
    }
}
