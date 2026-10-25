using SimplestCalendar.Core.Calendar;
using SimplestCalendar.Core.Services;

namespace SimplestCalendar.Services;

/// <summary>
/// Process-wide in-memory stores (decisions.md §4).
/// </summary>
public sealed class AppServices
{
    public static AppServices Current { get; private set; } = null!;

    public IEntryStore Entries { get; }
    public ISettingsStore Settings { get; }
    public DateTime ProcessStartedAt { get; } = DateTime.Now;

    private AppServices(IEntryStore entries, ISettingsStore settings)
    {
        Entries = entries;
        Settings = settings;
    }

    public static async Task InitializeAsync()
    {
        var dataDir = PortableDataPaths.GetDataDirectory();
        var entries = new JsonEntryStore(dataDir);
        var settings = new JsonSettingsStore(dataDir);
        await settings.LoadAsync();
        await entries.LoadAsync();

        var monthStart = MonthNavigation.CurrentMonthStart(DateOnly.FromDateTime(DateTime.Now));
        if (entries.WipeBefore(monthStart) > 0)
            await entries.SaveAsync();

        Current = new AppServices(entries, settings);
    }
}
