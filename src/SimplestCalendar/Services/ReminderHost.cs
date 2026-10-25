using Microsoft.UI.Dispatching;
using SimplestCalendar.Core.Calendar;
using SimplestCalendar.Core.Models;

namespace SimplestCalendar.Services;

public sealed class ReminderHost : IDisposable
{
    private readonly ReminderScheduler _scheduler = new();
    private readonly ToastNotificationService _toasts;
    private readonly DispatcherQueueTimer _timer;
    private bool _disposed;

    public ReminderHost(ToastNotificationService toasts)
    {
        _toasts = toasts;
        _timer = DispatcherQueue.GetForCurrentThread().CreateTimer();
        _timer.Interval = TimeSpan.FromSeconds(30);
        _timer.Tick += (_, _) => Tick();
        _timer.Start();
    }

    public void Tick()
    {
        if (_disposed)
            return;

        var now = DateTime.Now;
        var due = _scheduler.CollectDue(
            AppServices.Current.Entries.Entries,
            now,
            AppServices.Current.ProcessStartedAt);

        foreach (var entry in due)
        {
            _toasts.ShowReminder(entry);
            _scheduler.MarkFired(entry.Id);
        }
    }

    public void Dispose()
    {
        if (_disposed)
            return;
        _disposed = true;
        _timer.Stop();
    }
}
