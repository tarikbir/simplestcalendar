using Microsoft.UI.Xaml;
using SimplestCalendar.Core.Models;
using SimplestCalendar.Core.Services;

namespace SimplestCalendar.Views;

public sealed partial class CompactEntryView : Window
{
    public string TitleText { get; }
    public string WhenText { get; }
    public string NoteText { get; }

    public CompactEntryView(Entry entry)
    {
        TitleText = "Simplest Calendar";
        WhenText = $"{entry.Date:MMM d} · {EntryWorkflow.FormatListLabel(entry)}";
        NoteText = entry.Note;
        InitializeComponent();
        AppWindow.Resize(new Windows.Graphics.SizeInt32(400, 220));
    }

    private void Close_Click(object sender, RoutedEventArgs e) => Close();
}
