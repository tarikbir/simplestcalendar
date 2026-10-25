namespace SimplestCalendar.Core.Services;

/// <summary>
/// Portable JSON schema version. Bump when on-disk shape changes; add a migrator step.
/// </summary>
public static class DataSchema
{
    public const int Current = 1;

    /// <summary>
    /// Set once at process start from the UI assembly informational version.
    /// </summary>
    public static string AppVersion { get; set; } = "0.0.0";
}
