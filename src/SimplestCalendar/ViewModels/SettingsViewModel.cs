using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SimplestCalendar.Core.Models;
using SimplestCalendar.Core.Services;
using SimplestCalendar.Services;

namespace SimplestCalendar.ViewModels;

public sealed partial class SettingsViewModel : ObservableObject
{
    public ObservableCollection<DayOfWeek> WeekDays { get; } =
    [
        DayOfWeek.Monday,
        DayOfWeek.Tuesday,
        DayOfWeek.Wednesday,
        DayOfWeek.Thursday,
        DayOfWeek.Friday,
        DayOfWeek.Saturday,
        DayOfWeek.Sunday
    ];

    [ObservableProperty]
    private DayOfWeek _firstDayOfWeek = DayOfWeek.Monday;

    [ObservableProperty]
    private string _monthTitleFormat = "MMMM";

    [ObservableProperty]
    private bool _launchAtStartup;

    [ObservableProperty]
    private string _statusMessage = "";

    public string DataPath { get; } = PortableDataPaths.GetDataDirectory();
    public string ThemeLabel => "Dark (only theme in v1)";
    public string PreviewTitle =>
        Core.Calendar.MonthTitleFormatter.Format(DateOnly.FromDateTime(DateTime.Now), MonthTitleFormat);

    public SettingsViewModel()
    {
        var s = AppServices.Current.Settings.Settings;
        FirstDayOfWeek = s.FirstDayOfWeek;
        MonthTitleFormat = s.MonthTitleFormat;
        LaunchAtStartup = s.LaunchAtStartup || StartupRegistration.IsEnabled();
    }

    partial void OnMonthTitleFormatChanged(string value) =>
        OnPropertyChanged(nameof(PreviewTitle));

    [RelayCommand]
    private async Task SaveAsync()
    {
        var store = AppServices.Current.Settings;
        store.Settings.FirstDayOfWeek = FirstDayOfWeek;
        store.Settings.MonthTitleFormat = string.IsNullOrWhiteSpace(MonthTitleFormat) ? "MMMM" : MonthTitleFormat.Trim();
        store.Settings.LaunchAtStartup = LaunchAtStartup;
        store.Settings.Theme = AppTheme.Dark;
        await store.SaveAsync();

        try
        {
            var exe = Environment.ProcessPath ?? Path.Combine(AppContext.BaseDirectory, "SimplestCalendar.exe");
            StartupRegistration.SetEnabled(LaunchAtStartup, exe);
        }
        catch (Exception ex)
        {
            StatusMessage = $"Saved preferences, but startup registration failed: {ex.Message}";
            return;
        }

        StatusMessage = "Saved.";
        SettingsChanged?.Invoke();
    }

    public event Action? SettingsChanged;
}
