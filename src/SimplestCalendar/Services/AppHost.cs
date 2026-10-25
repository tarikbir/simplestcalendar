using System.Runtime.InteropServices;
using Microsoft.UI.Xaml;
using Microsoft.UI.Windowing;
using SimplestCalendar.Views;
using WinRT.Interop;

namespace SimplestCalendar.Services;

/// <summary>
/// Owns main window lifetime, tray, reminders, and month rollover for a single process.
/// </summary>
public sealed class AppHost : IDisposable
{
    public static AppHost? Current { get; private set; }

    private readonly MainWindow _mainWindow;
    private readonly NativeTrayIcon? _tray;
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

        try
        {
            _toasts.Initialize();
        }
        catch
        {
            // Toast shell is optional for unpackaged runs.
        }

        _toasts.EntryOpenedFromToast += id =>
            _mainWindow.DispatcherQueue.TryEnqueue(() => ShowCompactForEntry(id));

        _reminders = new ReminderHost(_toasts);
        _reminders.Tick();
        _rollover = new MonthRolloverHost();
        _rollover.MonthChanged += () =>
        {
            _mainWindow.DispatcherQueue.TryEnqueue(RefreshMainPage);
        };

        _tray = TryCreateTray();
        _mainWindow.AppWindow.Closing += AppWindow_Closing;
    }

    private NativeTrayIcon? TryCreateTray()
    {
        try
        {
            var tray = new NativeTrayIcon("Simplest Calendar", ResolveAppIconPath());
            tray.ShowRequested += () => Enqueue(ShowMainWindow);
            tray.SettingsRequested += () => Enqueue(ShowSettings);
            tray.QuitRequested += () => Enqueue(Quit);
            return tray;
        }
        catch
        {
            return null;
        }
    }

    private void Enqueue(Action action) =>
        _ = _mainWindow.DispatcherQueue.TryEnqueue(() => action());

    private static string? ResolveAppIconPath()
    {
        var baseDir = AppContext.BaseDirectory;
        foreach (var relative in new[]
                 {
                     Path.Combine("Assets", "AppIcon.ico"),
                     "AppIcon.ico"
                 })
        {
            var full = Path.Combine(baseDir, relative);
            if (File.Exists(full))
                return full;
        }

        return null;
    }

    private void AppWindow_Closing(AppWindow sender, AppWindowClosingEventArgs args)
    {
        if (_quitRequested)
            return;

        // Close means exit. (Win11 often hides tray icons; close-to-tray left users stuck.)
        args.Cancel = true;
        Quit();
    }

    public void ShowMainWindow()
    {
        void Show()
        {
            var hwnd = WindowNative.GetWindowHandle(_mainWindow);
            if (hwnd != IntPtr.Zero)
            {
                _ = ShowWindow(hwnd, SwRestore);
                _ = ShowWindow(hwnd, SwShow);
            }

            _mainWindow.AppWindow.Show();
            _mainWindow.Activate();
            _mainWindow.AppWindow.MoveInZOrderAtTop();
            if (hwnd != IntPtr.Zero)
                _ = SetForegroundWindow(hwnd);
        }

        if (_mainWindow.DispatcherQueue.HasThreadAccess)
            Show();
        else
            _ = _mainWindow.DispatcherQueue.TryEnqueue(Show);
    }

    private const int SwShow = 5;
    private const int SwRestore = 9;

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool SetForegroundWindow(IntPtr hWnd);

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
        _tray?.Dispose();
        if (ReferenceEquals(Current, this))
            Current = null;
    }
}
