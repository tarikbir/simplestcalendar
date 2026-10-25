using Microsoft.UI.Xaml;
using Microsoft.Windows.AppLifecycle;
using SimplestCalendar.Services;

namespace SimplestCalendar;

public partial class App : Application
{
    private Window? _window;
    private AppHost? _host;

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
            var activated = AppInstance.GetCurrent().GetActivatedEventArgs();
            await mainInstance.RedirectActivationToAsync(activated);
            System.Diagnostics.Process.GetCurrentProcess().Kill();
            return;
        }

        mainInstance.Activated += (_, _) =>
        {
            _host?.ShowMainWindow();
        };

        await AppServices.InitializeAsync();
        _window = new MainWindow();
        _host = new AppHost((MainWindow)_window);
        _window.Activate();
        _host.GetType(); // keep host rooted via field
    }
}
