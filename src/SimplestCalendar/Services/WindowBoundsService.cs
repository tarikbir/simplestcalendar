using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using SimplestCalendar.Core.Models;
using Windows.Graphics;

namespace SimplestCalendar.Services;

public static class WindowBoundsService
{
    public static void Apply(Window window, WindowBounds bounds)
    {
        ApplyMinimumSize(window);
        bounds.ClampToMinimum();
        window.AppWindow.Resize(new SizeInt32((int)bounds.Width, (int)bounds.Height));
        if (bounds.X is double x && bounds.Y is double y)
        {
            window.AppWindow.Move(new PointInt32((int)x, (int)y));
        }

        if (string.Equals(bounds.State, "Maximized", StringComparison.OrdinalIgnoreCase))
            window.AppWindow.SetPresenter(AppWindowPresenterKind.Overlapped);
    }

    public static void ApplyMinimumSize(Window window)
    {
        if (window.AppWindow.Presenter is OverlappedPresenter presenter)
        {
            presenter.PreferredMinimumWidth = (int)WindowBounds.MinWidth;
            presenter.PreferredMinimumHeight = (int)WindowBounds.MinHeight;
        }
    }

    public static void Capture(Window window, WindowBounds bounds)
    {
        var size = window.AppWindow.Size;
        var pos = window.AppWindow.Position;
        bounds.Width = Math.Max(WindowBounds.MinWidth, size.Width);
        bounds.Height = Math.Max(WindowBounds.MinHeight, size.Height);
        bounds.X = pos.X;
        bounds.Y = pos.Y;
        bounds.State = "Normal";
        bounds.ClampToMinimum();
    }

    public static async Task PersistMainWindowAsync(Window window)
    {
        var settings = AppServices.Current.Settings;
        Capture(window, settings.Settings.Window);
        await settings.SaveAsync();
    }
}
