using System.Text.Json;
using System.Text.Json.Serialization;
using SimplestCalendar.Core.Models;

namespace SimplestCalendar.Core.Services;

public interface ISettingsStore
{
    AppSettings Settings { get; }
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

    public async Task LoadAsync(CancellationToken cancellationToken = default)
    {
        if (!File.Exists(_filePath))
        {
            _settings = new AppSettings();
            return;
        }

        await using var stream = File.OpenRead(_filePath);
        var doc = await JsonSerializer.DeserializeAsync<SettingsDocument>(stream, Options, cancellationToken);
        _settings = doc?.ToSettings() ?? new AppSettings();
        _settings.Window.ClampToMinimum();
    }

    public async Task SaveAsync(CancellationToken cancellationToken = default)
    {
        _settings.Window.ClampToMinimum();
        var temp = _filePath + ".tmp";
        var doc = SettingsDocument.From(_settings);
        await using (var stream = File.Create(temp))
        {
            await JsonSerializer.SerializeAsync(stream, doc, Options, cancellationToken);
        }

        File.Copy(temp, _filePath, overwrite: true);
        File.Delete(temp);
    }

    public void Update(AppSettings settings)
    {
        _settings = settings;
        _settings.Window.ClampToMinimum();
    }

    private sealed class SettingsDocument
    {
        public int Version { get; set; } = 1;
        public string FirstDayOfWeek { get; set; } = "Monday";
        public string MonthTitleFormat { get; set; } = "MMMM";
        public bool LaunchAtStartup { get; set; }
        public string Theme { get; set; } = "Dark";
        public WindowBounds Window { get; set; } = new();

        public static SettingsDocument From(AppSettings s) => new()
        {
            Version = 1,
            FirstDayOfWeek = s.FirstDayOfWeek.ToString(),
            MonthTitleFormat = s.MonthTitleFormat,
            LaunchAtStartup = s.LaunchAtStartup,
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
                Theme = theme,
                Window = Window ?? new WindowBounds()
            };
        }
    }
}
