using SimplestCalendar.Core.Models;
using SimplestCalendar.Core.Services;

namespace SimplestCalendar.ViewModels;

public sealed class DayEntryListItem
{
    public Entry Entry { get; }
    public string Label { get; }

    public DayEntryListItem(Entry entry)
    {
        Entry = entry;
        Label = EntryWorkflow.FormatListLabel(entry);
    }
}
