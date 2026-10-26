namespace SimplestCalendar.Core.Models;

public sealed class AppSettings
{
    public DayOfWeek FirstDayOfWeek { get; set; } = DayOfWeek.Monday;
    public string MonthTitleFormat { get; set; } = "MMMM";
    public bool LaunchAtStartup { get; set; }
    /// <summary>
    /// When true, the window close button quits the app. When false (default), close hides to the tray.
    /// </summary>
    public bool CloseAppOnExit { get; set; }
    public AppTheme Theme { get; set; } = AppTheme.Dark;
    public WindowBounds Window { get; set; } = new();
}

public enum AppTheme
{
    Dark = 0
}

public sealed class WindowBounds
{
    public double Width { get; set; } = 1100;
    public double Height { get; set; } = 720;
    public double? X { get; set; }
    public double? Y { get; set; }
    public string State { get; set; } = "Normal";

    public const double MinWidth = 400;
    public const double MinHeight = 525;

    public void ClampToMinimum()
    {
        if (Width < MinWidth) Width = MinWidth;
        if (Height < MinHeight) Height = MinHeight;
    }
}
