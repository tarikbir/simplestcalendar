using Microsoft.Win32;

namespace SimplestCalendar.Core.Services;

/// <summary>
/// Unpackaged startup registration via HKCU Run (no package identity required).
/// </summary>
public static class StartupRegistration
{
    public const string RunValueName = "SimplestCalendar";
    private const string RunKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Run";

    public static bool IsEnabled()
    {
        using var key = Registry.CurrentUser.OpenSubKey(RunKeyPath, writable: false);
        var value = key?.GetValue(RunValueName) as string;
        return !string.IsNullOrWhiteSpace(value);
    }

    public static void SetEnabled(bool enabled, string executablePath)
    {
        using var key = Registry.CurrentUser.OpenSubKey(RunKeyPath, writable: true)
            ?? Registry.CurrentUser.CreateSubKey(RunKeyPath);

        if (enabled)
            key!.SetValue(RunValueName, $"\"{executablePath}\"");
        else if (key!.GetValue(RunValueName) is not null)
            key.DeleteValue(RunValueName, throwOnMissingValue: false);
    }
}
