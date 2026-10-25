using System.Runtime.InteropServices;

namespace SimplestCalendar.Services;

/// <summary>
/// Backup channel when <see cref="Microsoft.Windows.AppLifecycle.AppInstance.Activated"/>
/// fails to restore a tray-hidden window (wrong thread / no foreground rights).
/// </summary>
internal static class SingleInstanceWakeup
{
    private const string EventName = "Local\\SimplestCalendar.ShowWindow";

    public static void SignalShow()
    {
        try
        {
            using var ev = EventWaitHandle.OpenExisting(EventName);
            ev.Set();
        }
        catch (WaitHandleCannotBeOpenedException)
        {
            // Primary instance not listening yet — ignore.
        }
        catch (UnauthorizedAccessException)
        {
            // Different session / integrity — ignore.
        }
    }

    public static IDisposable StartListening(Action showMainWindow)
    {
        var ev = new EventWaitHandle(false, EventResetMode.AutoReset, EventName);
        var cts = new CancellationTokenSource();
        var thread = new Thread(() =>
        {
            try
            {
                while (!cts.IsCancellationRequested)
                {
                    if (ev.WaitOne(500))
                        showMainWindow();
                }
            }
            catch (ObjectDisposedException)
            {
                // Shutting down.
            }
        })
        {
            IsBackground = true,
            Name = "SimplestCalendar.SingleInstanceWakeup"
        };
        thread.Start();
        return new Lifetime(ev, cts, thread);
    }

    public static void AllowForegroundFor(int processId)
    {
        if (processId > 0)
            _ = AllowSetForegroundWindow(processId);
    }

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool AllowSetForegroundWindow(int dwProcessId);

    private sealed class Lifetime(EventWaitHandle ev, CancellationTokenSource cts, Thread thread) : IDisposable
    {
        public void Dispose()
        {
            cts.Cancel();
            try { thread.Join(1000); } catch { /* ignore */ }
            cts.Dispose();
            ev.Dispose();
        }
    }
}
