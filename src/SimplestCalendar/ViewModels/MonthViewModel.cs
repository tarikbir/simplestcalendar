using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Text;
using Microsoft.UI.Xaml.Media;
using SimplestCalendar.Core.Calendar;
using SimplestCalendar.Core.Models;
using SimplestCalendar.Core.Sorting;
using EntryWorkflow = SimplestCalendar.Core.Services.EntryWorkflow;
using SimplestCalendar.Services;
using Windows.UI;
using FontWeight = Windows.UI.Text.FontWeight;

namespace SimplestCalendar.ViewModels;

public sealed partial class DayEntrySummaryLine : ObservableObject
{
    public Entry Entry { get; }
    public string Label { get; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(LineBrush))]
    [NotifyPropertyChangedFor(nameof(LineBackground))]
    private bool _isDayHovered;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(LineBrush))]
    [NotifyPropertyChangedFor(nameof(LineBackground))]
    private bool _isEntryHovered;

    public Brush LineBrush
    {
        get
        {
            if (IsEntryHovered)
                return new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));
            if (IsDayHovered)
                return new SolidColorBrush(Color.FromArgb(255, 230, 230, 238));
            return new SolidColorBrush(Color.FromArgb(255, 168, 168, 176));
        }
    }

    public Brush LineBackground
    {
        get
        {
            if (IsEntryHovered)
                return new SolidColorBrush(Color.FromArgb(255, 70, 90, 120));
            if (IsDayHovered)
                return new SolidColorBrush(Color.FromArgb(40, 255, 255, 255));
            return new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));
        }
    }

    public DayEntrySummaryLine(Entry entry)
    {
        Entry = entry;
        Label = FormatSummary(entry);
    }

    private static string FormatSummary(Entry e)
    {
        var star = e.IsStarred ? "★ " : "";
        if (e.StartTime is null)
            return $"{star}{e.Note}";
        if (e.EndTime is TimeOnly end)
        {
            var endText = end == TimeOnly.MinValue ? "00:00" : end.ToString("HH:mm");
            return $"{star}{e.StartTime.Value:HH:mm}-{endText} {e.Note}";
        }

        return $"{star}{e.StartTime.Value:HH:mm} {e.Note}";
    }
}

public sealed partial class DayCellViewModel : ObservableObject
{
    public DateOnly Date { get; }
    public bool IsInViewedMonth { get; }
    public bool IsToday { get; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CellBackground))]
    private bool _isHovered;

    public ObservableCollection<DayEntrySummaryLine> EntryLines { get; } = [];

    public string DayNumber => Date.Day.ToString();

    public double OpacityForMonth => IsInViewedMonth ? 1.0 : 0.35;

    public FontWeight TodayWeight => IsToday ? FontWeights.Bold : FontWeights.Normal;

    public Brush TodayBrush => IsToday
        ? new SolidColorBrush(Color.FromArgb(255, 96, 205, 255))
        : new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));

    public Brush CellBackground
    {
        get
        {
            if (IsHovered)
                return new SolidColorBrush(Color.FromArgb(255, 55, 55, 58));
            if (IsToday)
                return new SolidColorBrush(Color.FromArgb(255, 24, 52, 78));
            return new SolidColorBrush(Color.FromArgb(255, 32, 32, 36));
        }
    }

    public DayCellViewModel(DayCell cell, IEnumerable<Entry> entries)
    {
        Date = cell.Date;
        IsInViewedMonth = cell.IsInViewedMonth;
        IsToday = cell.IsToday;
        foreach (var entry in EntrySort.Order(entries.Where(e => e.Date == cell.Date)))
            EntryLines.Add(new DayEntrySummaryLine(entry));
    }

    partial void OnIsHoveredChanged(bool value)
    {
        foreach (var line in EntryLines)
            line.IsDayHovered = value;
    }
}

public sealed partial class MonthViewModel : ObservableObject
{
    private readonly AppServices _services;

    [ObservableProperty]
    private string _title = "";

    [ObservableProperty]
    private bool _canGoNext;

    [ObservableProperty]
    private bool _canGoPrevious;

    [ObservableProperty]
    private DateOnly _viewedMonth;

    public ObservableCollection<string> WeekdayHeaders { get; } = [];
    public ObservableCollection<DayCellViewModel> Days { get; } = [];

    public MonthViewModel(AppServices services)
    {
        _services = services;
        var today = DateOnly.FromDateTime(DateTime.Now);
        ViewedMonth = MonthNavigation.CurrentMonthStart(today);
        Refresh();
    }

    public void Refresh()
    {
        var today = DateOnly.FromDateTime(DateTime.Now);
        ViewedMonth = MonthNavigation.Clamp(ViewedMonth, today);
        var settings = _services.Settings.Settings;

        Title = MonthTitleFormatter.Format(ViewedMonth, settings.MonthTitleFormat);
        CanGoNext = MonthNavigation.CanGoNext(ViewedMonth, today);
        CanGoPrevious = MonthNavigation.CanGoPrevious(ViewedMonth, today);

        WeekdayHeaders.Clear();
        foreach (var h in MonthGridBuilder.WeekdayHeaders(settings.FirstDayOfWeek))
            WeekdayHeaders.Add(h);

        Days.Clear();
        foreach (var cell in MonthGridBuilder.Build(ViewedMonth, settings.FirstDayOfWeek, today))
            Days.Add(new DayCellViewModel(cell, _services.Entries.Entries));
    }

    public async Task DeleteEntryAsync(Entry entry)
    {
        EntryWorkflow.Delete(_services.Entries, entry);
        await _services.Entries.SaveAsync();
        Refresh();
    }

    [RelayCommand]
    private void GoNext()
    {
        var today = DateOnly.FromDateTime(DateTime.Now);
        ViewedMonth = MonthNavigation.Next(ViewedMonth, today);
        Refresh();
    }

    [RelayCommand]
    private void GoPrevious()
    {
        var today = DateOnly.FromDateTime(DateTime.Now);
        ViewedMonth = MonthNavigation.Previous(ViewedMonth, today);
        Refresh();
    }

    [RelayCommand]
    private void GoToday()
    {
        ViewedMonth = MonthNavigation.CurrentMonthStart(DateOnly.FromDateTime(DateTime.Now));
        Refresh();
    }
}
