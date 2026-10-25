using H.NotifyIcon;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Windowing;
using SimplestCalendar.Views;

namespace SimplestCalendar.Services;

/// <summary>
/// Owns main window lifetime, tray, reminders, and month rollover for a single process.
/// </summary>
public sealed class AppHost : IDisposable
{
    public static AppHost? Current { get; private set; }

    private readonly MainWindow _mainWindow;
    private readonly TaskbarIcon _tray;
    private readonly ToastNotificationService _toasts = new();
    private readonly ReminderHost _reminders;
    private readonly MonthRolloverHost _rollover;
    private SettingsWindow? _settingsWindow;
    private bool _quitRequested;

    public AppHost(MainWindow mainWindow)
    {
        Current = this;
        _mainWindow = mainWindow;

        WindowBoundsService.Apply(_mainWindow, AppServices.Current.Settings.Settings.Window);

        _toasts.Initialize();
        _toasts.EntryOpenedFromToast += id =>
            _mainWindow.DispatcherQueue.TryEnqueue(() => ShowCompactForEntry(id));

        _reminders = new ReminderHost(_toasts);
        _reminders.Tick();
        _rollover = new MonthRolloverHost();
        _rollover.MonthChanged += () =>
        {
            _mainWindow.DispatcherQueue.TryEnqueue(RefreshMainPage);
        };

        _tray = new TaskbarIcon
        {
            ToolTipText = "Simplest Calendar"
        };
        try
        {
            _tray.Icon = new System.Drawing.Icon(
                Path.Combine(AppContext.BaseDirectory, "Assets", "AppIcon.ico"));
        }
        catch
        {
            // Tray still works without custom icon on some setups.
        }

        var menu = new MenuFlyout();
        var showItem = new MenuFlyoutItem { Text = "Show" };
        showItem.Click += (_, _) => ShowMainWindow();
        var settingsItem = new MenuFlyoutItem { Text = "Settings" };
        settingsItem.Click += (_, _) => ShowSettings();
        var quitItem = new MenuFlyoutItem { Text = "Quit" };
        quitItem.Click += (_, _) => Quit();
        menu.Items.Add(showItem);
        menu.Items.Add(settingsItem);
        menu.Items.Add(new MenuFlyoutSeparator());
        menu.Items.Add(quitItem);
        _tray.ContextFlyout = menu;
        _tray.LeftClickCommand = new RelayActionCommand(ShowMainWindow);

        _mainWindow.AppWindow.Closing += AppWindow_Closing;
    }

    private async void AppWindow_Closing(AppWindow sender, AppWindowClosingEventArgs args)
    {
        if (_quitRequested)
            return;

        args.Cancel = true;
        try
        {
            await WindowBoundsService.PersistMainWindowAsync(_mainWindow);
        }
        catch
        {
            // Still hide even if persist fails.
        }

        _mainWindow.AppWindow.Hide();
    }

    public void ShowMainWindow()
    {
        _mainWindow.AppWindow.Show();
        _mainWindow.Activate();
    }

    public void ShowSettings()
    {
        if (_settingsWindow is null)
        {
            _settingsWindow = new SettingsWindow();
            _settingsWindow.ViewModel.SettingsChanged += RefreshMainPage;
            _settingsWindow.Closed += (_, _) => _settingsWindow = null;
        }

        _settingsWindow.Activate();
        _settingsWindow.AppWindow.Show();
    }

    public void ShowCompactForEntry(Guid entryId)
    {
        var entry = AppServices.Current.Entries.Entries.FirstOrDefault(e => e.Id == entryId);
        if (entry is null)
            return;

        var view = new CompactEntryView(entry);
        view.Activate();
    }

    private void RefreshMainPage()
    {
        if (_mainWindow.ContentFrame.Content is MainPage page)
            page.ViewModel.Refresh();
    }

    public void Quit()
    {
        _quitRequested = true;
        try
        {
            WindowBoundsService.PersistMainWindowAsync(_mainWindow).GetAwaiter().GetResult();
        }
        catch
        {
            // ignore
        }

        Dispose();
        _mainWindow.Close();
        Application.Current.Exit();
    }

    public void Dispose()
    {
        _reminders.Dispose();
        _rollover.Dispose();
        _toasts.Unregister();
        _tray.Dispose();
        if (ReferenceEquals(Current, this))
            Current = null;
    }

    private sealed class RelayActionCommand(Action action) : System.Windows.Input.ICommand
    {
        public event EventHandler? CanExecuteChanged
        {
            add { }
            remove { }
        }

        public bool CanExecute(object? parameter) => true;
        public void Execute(object? parameter) => action();
    }
}
