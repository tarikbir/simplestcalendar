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
    private string _startTimeText = "";

    [ObservableProperty]
    private bool _hasDuration;

    [ObservableProperty]
    private int _durationHours = 1;

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
        StartTimeText = editing.StartTime?.ToString("HH:mm") ?? "";
        HasDuration = editing.DurationHours is > 0;
        DurationHours = editing.DurationHours is > 0 ? editing.DurationHours.Value : 1;
        IsStarred = editing.IsStarred;
        ReminderEnabled = editing.ReminderEnabled;
        ReminderLeadMinutes = editing.ReminderLeadMinutes > 0 ? editing.ReminderLeadMinutes : 5;
    }

    public bool TryBuildDraft(out EntryDraft draft)
    {
        draft = default!;
        ValidationMessage = null;

        TimeOnly? start = null;
        if (!string.IsNullOrWhiteSpace(StartTimeText))
        {
            if (!TimeOnly.TryParse(StartTimeText.Trim(), out var parsed))
            {
                ValidationMessage = "Start time must look like HH:mm.";
                return false;
            }

            start = parsed;
        }

        int? duration = HasDuration ? DurationHours : null;
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
