namespace SimplestCalendar.Core.Services;

public static class DataFileBackup
{
    /// <summary>
    /// Copies <paramref name="filePath"/> beside itself as
    /// <c>{name}.bak-v{version}-{yyyyMMddHHmmss}</c>. Returns the backup path, or null if source missing.
    /// </summary>
    public static string? Create(string filePath, int version)
    {
        if (!File.Exists(filePath))
            return null;

        var dir = Path.GetDirectoryName(filePath)!;
        var name = Path.GetFileName(filePath);
        var stamp = DateTime.Now.ToString("yyyyMMddHHmmss");
        var backupName = $"{name}.bak-v{version}-{stamp}";
        var backupPath = Path.Combine(dir, backupName);

        var n = 0;
        while (File.Exists(backupPath))
        {
            n++;
            backupPath = Path.Combine(dir, $"{backupName}-{n}");
        }

        File.Copy(filePath, backupPath, overwrite: false);
        return backupPath;
    }
}
