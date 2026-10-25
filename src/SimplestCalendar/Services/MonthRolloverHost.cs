using Microsoft.UI.Dispatching;
using SimplestCalendar.Core.Calendar;

namespace SimplestCalendar.Services;

public sealed class MonthRolloverHost : IDisposable
{
    private readonly DispatcherQueueTimer _timer;
    private DateOnly _knownMonthStart;
    private bool _disposed;

    public event Action? MonthChanged;

    public MonthRolloverHost()
    {
        _knownMonthStart = MonthRollover.CurrentMonthStart(DateOnly.FromDateTime(DateTime.Now));
        _timer = DispatcherQueue.GetForCurrentThread().CreateTimer();
        _timer.Interval = TimeSpan.FromMinutes(1);
        _timer.Tick += async (_, _) => await CheckAsync();
        _timer.Start();
    }

    public async Task CheckAsync()
    {
        if (_disposed)
            return;

        var today = DateOnly.FromDateTime(DateTime.Now);
        if (!MonthRollover.HasRolledOver(_knownMonthStart, today))
            return;

        _knownMonthStart = MonthRollover.CurrentMonthStart(today);
        var removed = AppServices.Current.Entries.WipeBefore(_knownMonthStart);
        if (removed > 0)
            await AppServices.Current.Entries.SaveAsync();
        MonthChanged?.Invoke();
    }

    public void Dispose()
    {
        if (_disposed)
            return;
        _disposed = true;
        _timer.Stop();
    }
}
