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
        DurationBox.Value = viewModel.DurationHours;
        LeadBox.Value = viewModel.ReminderLeadMinutes;
        DurationBox.IsEnabled = viewModel.HasDuration;
        LeadBox.IsEnabled = viewModel.ReminderEnabled;
        ViewModel.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(EntryEditorViewModel.HasDuration))
                DurationBox.IsEnabled = ViewModel.HasDuration;
            if (e.PropertyName == nameof(EntryEditorViewModel.ReminderEnabled))
                LeadBox.IsEnabled = ViewModel.ReminderEnabled;
        };
    }

    public void FocusNote() => NoteBox.Focus(FocusState.Programmatic);

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

    private void DurationBox_ValueChanged(NumberBox sender, NumberBoxValueChangedEventArgs args)
    {
        if (!double.IsNaN(args.NewValue))
            ViewModel.DurationHours = Math.Max(1, (int)args.NewValue);
    }

    private void LeadBox_ValueChanged(NumberBox sender, NumberBoxValueChangedEventArgs args)
    {
        if (!double.IsNaN(args.NewValue))
            ViewModel.ReminderLeadMinutes = Math.Max(1, (int)args.NewValue);
    }
}
