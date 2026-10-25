using System.Reflection;
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
    public string? DataLoadWarning { get; }
    public string AppVersion { get; }
    public int DataSchemaVersion => DataSchema.Current;

    private AppServices(IEntryStore entries, ISettingsStore settings, string? dataLoadWarning, string appVersion)
    {
        Entries = entries;
        Settings = settings;
        DataLoadWarning = dataLoadWarning;
        AppVersion = appVersion;
    }

    public static async Task InitializeAsync()
    {
        var appVersion = ResolveAppVersion();
        DataSchema.AppVersion = appVersion;

        var dataDir = PortableDataPaths.GetDataDirectory();
        var entries = new JsonEntryStore(dataDir);
        var settings = new JsonSettingsStore(dataDir);
        await settings.LoadAsync();
        await entries.LoadAsync();

        var monthStart = MonthNavigation.CurrentMonthStart(DateOnly.FromDateTime(DateTime.Now));
        if (entries.WipeBefore(monthStart) > 0)
            await entries.SaveAsync();

        var warning = JoinWarnings(settings.LoadWarning, entries.LoadWarning);
        Current = new AppServices(entries, settings, warning, appVersion);
    }

    private static string? JoinWarnings(string? a, string? b)
    {
        if (string.IsNullOrWhiteSpace(a)) return string.IsNullOrWhiteSpace(b) ? null : b;
        if (string.IsNullOrWhiteSpace(b)) return a;
        return a + Environment.NewLine + Environment.NewLine + b;
    }

    private static string ResolveAppVersion()
    {
        var asm = Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();
        var info = asm.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
        if (!string.IsNullOrWhiteSpace(info))
        {
            var plus = info.IndexOf('+');
            return plus >= 0 ? info[..plus] : info;
        }

        return asm.GetName().Version?.ToString(3) ?? "0.0.0";
    }
}
