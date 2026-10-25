namespace SimplestCalendar.Core.Services;

public static class PortableDataPaths
{
    /// <summary>
    /// Data lives beside the executable for portable installs (decisions.md §4).
    /// </summary>
    public static string GetDataDirectory(string? baseDirectory = null)
    {
        var root = baseDirectory ?? AppContext.BaseDirectory;
        return Path.GetFullPath(Path.Combine(root, "data"));
    }
}
