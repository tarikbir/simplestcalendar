using Microsoft.Win32;

namespace SimplestCalendar.Services;

/// <summary>
/// Windows 11 hides new tray icons by default; promote ours so close-to-tray is usable.
/// </summary>
internal static class TrayIconPromotion
{
    private const string NotifyIconSettingsKey = @"Control Panel\NotifyIconSettings";

    public static void PromoteCurrentApp()
    {
        try
        {
            var exe = Environment.ProcessPath;
            if (string.IsNullOrWhiteSpace(exe))
                return;

            using var root = Registry.CurrentUser.OpenSubKey(NotifyIconSettingsKey, writable: true);
            if (root is null)
                return;

            foreach (var name in root.GetSubKeyNames())
            {
                using var sub = root.OpenSubKey(name, writable: true);
                if (sub is null)
                    continue;

                var path = sub.GetValue("ExecutablePath") as string;
                if (path is null || !path.Equals(exe, StringComparison.OrdinalIgnoreCase))
                    continue;

                sub.SetValue("IsPromoted", 1, RegistryValueKind.DWord);
            }
        }
        catch
        {
            // Best-effort; overflow (^) still works if promotion fails.
        }
    }
}
