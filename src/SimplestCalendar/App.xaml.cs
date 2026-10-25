using Microsoft.UI.Xaml;
using Microsoft.Windows.AppLifecycle;
using SimplestCalendar.Services;

namespace SimplestCalendar;

public partial class App : Application
{
    private Window? _window;
    private AppHost? _host;
    private IDisposable? _wakeup;

    public App()
    {
        InitializeComponent();
        RequestedTheme = ApplicationTheme.Dark;
    }

    protected override async void OnLaunched(LaunchActivatedEventArgs args)
    {
        var mainInstance = AppInstance.FindOrRegisterForKey("SimplestCalendar");
        if (!mainInstance.IsCurrent)
        {
            // Wake tray-hidden primary before redirect (Activated alone is unreliable).
            try
            {
                var primaryPid = (int)mainInstance.ProcessId;
                SingleInstanceWakeup.AllowForegroundFor(primaryPid);
            }
            catch
            {
                // ProcessId may be unavailable on some runtimes.
            }

            SingleInstanceWakeup.SignalShow();
            var activated = AppInstance.GetCurrent().GetActivatedEventArgs();
            await mainInstance.RedirectActivationToAsync(activated);
            System.Diagnostics.Process.GetCurrentProcess().Kill();
            return;
        }

        // AppInstance.Activated can fire off the UI thread; Show/Activate must run on it.
        mainInstance.Activated += (_, _) => RequestShowMainWindow();

        await AppServices.InitializeAsync();
        _window = new MainWindow();
        // Listen before AppHost so a second launch can wake us even if tray/toast init is slow.
        _wakeup = SingleInstanceWakeup.StartListening(RequestShowMainWindow);
        _host = new AppHost((MainWindow)_window);
        _window.Activate();
        _host.ShowMainWindow();
    }

    private void RequestShowMainWindow()
    {
        var queue = _window?.DispatcherQueue;
        if (queue is null)
            return;
        _ = queue.TryEnqueue(() => _host?.ShowMainWindow());
    }
}
