using Microsoft.Windows.AppNotifications;
using Microsoft.Windows.AppNotifications.Builder;
using SimplestCalendar.Core.Models;

namespace SimplestCalendar.Services;

public sealed class ToastNotificationService
{
    private bool _registered;
    public event Action<Guid>? EntryOpenedFromToast;

    public void Initialize()
    {
        if (_registered)
            return;

        AppNotificationManager.Default.NotificationInvoked += OnNotificationInvoked;
        AppNotificationManager.Default.Register();
        _registered = true;
    }

    public void Unregister()
    {
        if (!_registered)
            return;
        try
        {
            AppNotificationManager.Default.Unregister();
        }
        catch
        {
            // Best-effort on shutdown
        }

        _registered = false;
    }

    public void ShowReminder(Entry entry)
    {
        var time = entry.StartTime is TimeOnly t ? t.ToString("HH:mm") : "all day";
        var builder = new AppNotificationBuilder()
            .AddArgument("entryId", entry.Id.ToString("N"))
            .AddText("Simplest Calendar")
            .AddText($"{time} — {entry.Note}");

        AppNotificationManager.Default.Show(builder.BuildNotification());
    }

    private void OnNotificationInvoked(AppNotificationManager sender, AppNotificationActivatedEventArgs args)
    {
        string? idText = null;
        if (args.Arguments is { Count: > 0 } map && map.TryGetValue("entryId", out var fromMap))
            idText = fromMap;
        else if (!string.IsNullOrWhiteSpace(args.Argument))
        {
            // Fallback: "entryId=<guid>" style
            foreach (var part in args.Argument.Split(';', StringSplitOptions.RemoveEmptyEntries))
            {
                var kv = part.Split('=', 2);
                if (kv.Length == 2 && kv[0].Equals("entryId", StringComparison.OrdinalIgnoreCase))
                    idText = kv[1];
            }
        }

        if (idText is not null && Guid.TryParseExact(idText, "N", out var id))
            EntryOpenedFromToast?.Invoke(id);
    }
}
