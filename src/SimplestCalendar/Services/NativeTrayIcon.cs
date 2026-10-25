namespace SimplestCalendar.Services;

/// <summary>
/// Tray icon via WinForms NotifyIcon on a dedicated STA thread (WinUI's loop alone is not enough).
/// </summary>
internal sealed class NativeTrayIcon : IDisposable
{
    private readonly Thread _thread;
    private System.Windows.Forms.NotifyIcon? _notify;
    private System.Drawing.Icon? _ownedIcon;
    private System.Windows.Forms.ApplicationContext? _context;
    private readonly ManualResetEventSlim _ready = new(false);
    private Exception? _startError;
    private bool _disposed;

    public event Action? ShowRequested;
    public event Action? SettingsRequested;
    public event Action? QuitRequested;

    public bool IsCreated { get; private set; }

    public NativeTrayIcon(string tooltip, string? iconPath)
    {
        _thread = new Thread(() => RunTrayThread(tooltip, iconPath))
        {
            IsBackground = true,
            Name = "SimplestCalendar.Tray"
        };
        _thread.SetApartmentState(ApartmentState.STA);
        _thread.Start();

        if (!_ready.Wait(TimeSpan.FromSeconds(5)))
            throw new TimeoutException("Tray icon thread did not start in time.");

        if (_startError is not null)
            throw _startError;

        if (!IsCreated)
            throw new InvalidOperationException("Tray icon failed to become visible.");
    }

    private void RunTrayThread(string tooltip, string? iconPath)
    {
        try
        {
            System.Windows.Forms.Application.SetHighDpiMode(System.Windows.Forms.HighDpiMode.PerMonitorV2);

            System.Drawing.Icon icon;
            if (!string.IsNullOrWhiteSpace(iconPath) && File.Exists(iconPath))
            {
                _ownedIcon = new System.Drawing.Icon(iconPath);
                icon = _ownedIcon;
            }
            else
            {
                icon = System.Drawing.SystemIcons.Application;
            }

            var menu = new System.Windows.Forms.ContextMenuStrip();
            menu.Items.Add("Show", null, (_, _) => ShowRequested?.Invoke());
            menu.Items.Add("Settings", null, (_, _) => SettingsRequested?.Invoke());
            menu.Items.Add(new System.Windows.Forms.ToolStripSeparator());
            menu.Items.Add("Quit", null, (_, _) => QuitRequested?.Invoke());

            _notify = new System.Windows.Forms.NotifyIcon
            {
                Icon = icon,
                Text = tooltip.Length > 63 ? tooltip[..63] : tooltip,
                ContextMenuStrip = menu,
                Visible = true
            };
            _notify.MouseClick += (_, e) =>
            {
                if (e.Button == System.Windows.Forms.MouseButtons.Left)
                    ShowRequested?.Invoke();
            };

            IsCreated = _notify.Visible;
            TrayIconPromotion.PromoteCurrentApp();
            _context = new System.Windows.Forms.ApplicationContext();
            _ready.Set();
            System.Windows.Forms.Application.Run(_context);
        }
        catch (Exception ex)
        {
            _startError = ex;
            _ready.Set();
        }
        finally
        {
            if (_notify is not null)
            {
                _notify.Visible = false;
                _notify.Dispose();
                _notify = null;
            }

            _ownedIcon?.Dispose();
            _ownedIcon = null;
        }
    }

    public void ShowRunningInTrayTip()
    {
        try
        {
            _notify?.ShowBalloonTip(
                4000,
                "Simplest Calendar",
                "Still running in the notification area. Click the icon to open again.",
                System.Windows.Forms.ToolTipIcon.Info);
        }
        catch
        {
            // ignore
        }
    }

    public void Dispose()
    {
        if (_disposed)
            return;
        _disposed = true;

        try
        {
            _context?.ExitThread();
        }
        catch
        {
            // ignore
        }

        if (!_thread.Join(500))
        {
            // Best-effort; process exit will clean up.
        }

        _ready.Dispose();
    }
}
