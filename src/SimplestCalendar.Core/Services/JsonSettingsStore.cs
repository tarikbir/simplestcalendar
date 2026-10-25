using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using SimplestCalendar.Core.Models;

namespace SimplestCalendar.Core.Services;

public interface ISettingsStore
{
    AppSettings Settings { get; }
    string? LoadWarning { get; }
    int LoadedSchemaVersion { get; }
    Task LoadAsync(CancellationToken cancellationToken = default);
    Task SaveAsync(CancellationToken cancellationToken = default);
    void Update(AppSettings settings);
}

public sealed class JsonSettingsStore : ISettingsStore
{
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly string _filePath;
    private AppSettings _settings = new();

    public JsonSettingsStore(string dataDirectory)
    {
        Directory.CreateDirectory(dataDirectory);
        _filePath = Path.Combine(dataDirectory, "settings.json");
    }

    public AppSettings Settings => _settings;
    public string? LoadWarning { get; private set; }
    public int LoadedSchemaVersion { get; private set; } = DataSchema.Current;

    public async Task LoadAsync(CancellationToken cancellationToken = default)
    {
        LoadWarning = null;
        LoadedSchemaVersion = DataSchema.Current;

        if (!File.Exists(_filePath))
        {
            _settings = new AppSettings();
            return;
        }

        string text;
        try
        {
            text = await File.ReadAllTextAsync(_filePath, cancellationToken);
        }
        catch (Exception ex)
        {
            RefuseWithBackup(0, $"Could not read settings.json: {ex.Message}");
            _settings = new AppSettings();
            return;
        }

        JsonNode? node;
        try
        {
            node = JsonNode.Parse(text);
        }
        catch (JsonException)
        {
            RefuseWithBackup(0, "settings.json is corrupt. A backup was kept; using defaults.");
            _settings = new AppSettings();
            return;
        }

        if (node is not JsonObject root)
        {
            RefuseWithBackup(0, "settings.json has an unexpected shape. A backup was kept; using defaults.");
            _settings = new AppSettings();
            return;
        }

        var version = ReadVersion(root);
        if (version < 1)
            version = 1;
        if (version > DataSchema.Current)
        {
            RefuseWithBackup(version, $"settings.json schema v{version} is newer than this app supports (v{DataSchema.Current}). Original left intact; using defaults.");
            _settings = new AppSettings();
            return;
        }

        if (version < DataSchema.Current)
        {
            try
            {
                SchemaMigration.MigrateToCurrent(root, version);
                await WriteRootAsync(root, cancellationToken);
            }
            catch (Exception ex)
            {
                RefuseWithBackup(version, $"Could not migrate settings.json: {ex.Message}");
                _settings = new AppSettings();
                return;
            }
        }

        try
        {
            var doc = root.Deserialize<SettingsDocument>(Options);
            _settings = doc?.ToSettings() ?? new AppSettings();
            _settings.Window.ClampToMinimum();
            LoadedSchemaVersion = DataSchema.Current;
        }
        catch (JsonException)
        {
            RefuseWithBackup(version, "settings.json could not be parsed. A backup was kept; using defaults.");
            _settings = new AppSettings();
        }
    }

    public async Task SaveAsync(CancellationToken cancellationToken = default)
    {
        _settings.Window.ClampToMinimum();
        var doc = SettingsDocument.From(_settings);
        var temp = _filePath + ".tmp";
        await using (var stream = File.Create(temp))
        {
            await JsonSerializer.SerializeAsync(stream, doc, Options, cancellationToken);
        }

        File.Copy(temp, _filePath, overwrite: true);
        File.Delete(temp);
        LoadedSchemaVersion = DataSchema.Current;
    }

    public void Update(AppSettings settings)
    {
        _settings = settings;
        _settings.Window.ClampToMinimum();
    }

    private void RefuseWithBackup(int versionHint, string warning)
    {
        DataFileBackup.Create(_filePath, versionHint);
        LoadWarning = warning;
        LoadedSchemaVersion = 0;
    }

    private static int ReadVersion(JsonObject root)
    {
        if (root.TryGetPropertyValue("version", out var v) && v is JsonValue jv && jv.TryGetValue<int>(out var n))
            return n;
        return 1;
    }

    private async Task WriteRootAsync(JsonObject root, CancellationToken cancellationToken)
    {
        root["version"] = DataSchema.Current;
        root["appVersion"] = DataSchema.AppVersion;
        var temp = _filePath + ".tmp";
        await File.WriteAllTextAsync(temp, root.ToJsonString(Options), cancellationToken);
        File.Copy(temp, _filePath, overwrite: true);
        File.Delete(temp);
    }

    private sealed class SettingsDocument
    {
        public int Version { get; set; } = DataSchema.Current;
        public string? AppVersion { get; set; }
        public string FirstDayOfWeek { get; set; } = "Monday";
        public string MonthTitleFormat { get; set; } = "MMMM";
        public bool LaunchAtStartup { get; set; }
        public bool CloseAppOnExit { get; set; }
        public string Theme { get; set; } = "Dark";
        public WindowBounds Window { get; set; } = new();

        public static SettingsDocument From(AppSettings s) => new()
        {
            Version = DataSchema.Current,
            AppVersion = DataSchema.AppVersion,
            FirstDayOfWeek = s.FirstDayOfWeek.ToString(),
            MonthTitleFormat = s.MonthTitleFormat,
            LaunchAtStartup = s.LaunchAtStartup,
            CloseAppOnExit = s.CloseAppOnExit,
            Theme = s.Theme.ToString(),
            Window = s.Window
        };

        public AppSettings ToSettings()
        {
            if (!Enum.TryParse<DayOfWeek>(FirstDayOfWeek, true, out var dow))
                dow = DayOfWeek.Monday;
            if (!Enum.TryParse<AppTheme>(Theme, true, out var theme))
                theme = AppTheme.Dark;
            return new AppSettings
            {
                FirstDayOfWeek = dow,
                MonthTitleFormat = string.IsNullOrWhiteSpace(MonthTitleFormat) ? "MMMM" : MonthTitleFormat,
                LaunchAtStartup = LaunchAtStartup,
                CloseAppOnExit = CloseAppOnExit,
                Theme = theme,
                Window = Window ?? new WindowBounds()
            };
        }
    }
}
