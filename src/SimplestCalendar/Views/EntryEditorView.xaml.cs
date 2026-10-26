using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using SimplestCalendar.Services;
using SimplestCalendar.ViewModels;

namespace SimplestCalendar.Views;

public sealed partial class EntryEditorView : UserControl
{
    public EntryEditorViewModel ViewModel { get; }

    public EntryEditorView(EntryEditorViewModel viewModel)
    {
        ViewModel = viewModel;
        InitializeComponent();
        DurationHoursBox.Value = viewModel.DurationHours;
        DurationMinutesBox.Value = viewModel.DurationMinutesPart;
        LeadBox.Value = viewModel.ReminderLeadMinutes;
        SyncDurationEnabled();
        LeadBox.IsEnabled = viewModel.ReminderEnabled;
        StartTimePicker.IsEnabled = viewModel.HasStartTime;
        ViewModel.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(EntryEditorViewModel.HasDuration))
                SyncDurationEnabled();
            if (e.PropertyName == nameof(EntryEditorViewModel.HasStartTime))
                StartTimePicker.IsEnabled = ViewModel.HasStartTime;
            if (e.PropertyName == nameof(EntryEditorViewModel.ReminderEnabled))
                LeadBox.IsEnabled = ViewModel.ReminderEnabled;
        };
    }

    public void FocusNote() => NoteBox.Focus(FocusState.Programmatic);

    private void SyncDurationEnabled()
    {
        var on = ViewModel.HasDuration;
        DurationHoursBox.IsEnabled = on;
        DurationMinutesBox.IsEnabled = on;
    }

    private void EmojiButton_Click(object sender, RoutedEventArgs e)
    {
        NoteBox.Focus(FocusState.Programmatic);
        EmojiPanelService.Show();
    }

    private void LeadPreset_Click(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement { Tag: string tag } && int.TryParse(tag, out var minutes))
        {
            ViewModel.ReminderLeadMinutes = minutes;
            ViewModel.ReminderEnabled = true;
            LeadBox.Value = minutes;
        }
    }

    private void DurationHoursBox_ValueChanged(NumberBox sender, NumberBoxValueChangedEventArgs args)
    {
        if (!double.IsNaN(args.NewValue))
            ViewModel.DurationHours = Math.Max(0, (int)args.NewValue);
    }

    private void DurationMinutesBox_ValueChanged(NumberBox sender, NumberBoxValueChangedEventArgs args)
    {
        if (!double.IsNaN(args.NewValue))
            ViewModel.DurationMinutesPart = Math.Clamp((int)args.NewValue, 0, 59);
    }

    private void LeadBox_ValueChanged(NumberBox sender, NumberBoxValueChangedEventArgs args)
    {
        if (!double.IsNaN(args.NewValue))
            ViewModel.ReminderLeadMinutes = Math.Clamp((int)args.NewValue, 1, 60);
    }

    private void StartTimePicker_TimeChanged(object sender, TimePickerValueChangedEventArgs args)
    {
        // Checkbox owns HasStartTime; do not auto-enable on picker init/change.
        ViewModel.StartTime = args.NewTime;
    }
}
