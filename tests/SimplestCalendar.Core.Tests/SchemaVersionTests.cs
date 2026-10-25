using System.Text.Json;
using SimplestCalendar.Core.Models;
using SimplestCalendar.Core.Services;

namespace SimplestCalendar.Core.Tests;

public class SchemaVersionTests
{
    public SchemaVersionTests()
    {
        DataSchema.AppVersion = "1.0.0-test";
    }

    [Fact]
    public async Task Save_writes_schema_and_app_version()
    {
        var dir = NewDir();
        try
        {
            var store = new JsonEntryStore(dir);
            store.Upsert(new Entry { Date = new DateOnly(2026, 10, 25), Note = "hi" });
            await store.SaveAsync();

            await using var stream = File.OpenRead(Path.Combine(dir, "entries.json"));
            using var doc = await JsonDocument.ParseAsync(stream);
            Assert.Equal(DataSchema.Current, doc.RootElement.GetProperty("version").GetInt32());
            Assert.Equal("1.0.0-test", doc.RootElement.GetProperty("appVersion").GetString());
            Assert.Null(store.LoadWarning);
        }
        finally
        {
            Cleanup(dir);
        }
    }

    [Fact]
    public async Task Missing_version_loads_as_v1_and_migrates_to_current()
    {
        var dir = NewDir();
        try
        {
            await File.WriteAllTextAsync(Path.Combine(dir, "entries.json"), """
                { "entries": [ { "id": "11111111-1111-1111-1111-111111111111", "date": "2026-10-25", "note": "legacy", "isStarred": false, "reminderEnabled": false, "reminderLeadMinutes": 5, "createdAt": "2026-10-25T12:00:00", "updatedAt": "2026-10-25T12:00:00" } ] }
                """);

            var store = new JsonEntryStore(dir);
            await store.LoadAsync();
            Assert.Single(store.Entries);
            Assert.Equal("legacy", store.Entries[0].Note);
            Assert.Null(store.LoadWarning);
            Assert.Equal(DataSchema.Current, store.LoadedSchemaVersion);
        }
        finally
        {
            Cleanup(dir);
        }
    }

    [Fact]
    public async Task V1_duration_hours_migrates_to_minutes()
    {
        var dir = NewDir();
        try
        {
            await File.WriteAllTextAsync(Path.Combine(dir, "entries.json"), """
                {
                  "version": 1,
                  "entries": [
                    {
                      "id": "11111111-1111-1111-1111-111111111111",
                      "date": "2026-10-25",
                      "note": "timed",
                      "startTime": "18:00",
                      "durationHours": 2,
                      "endTime": "20:00",
                      "isStarred": false,
                      "reminderEnabled": false,
                      "reminderLeadMinutes": 5,
                      "createdAt": "2026-10-25T12:00:00",
                      "updatedAt": "2026-10-25T12:00:00"
                    }
                  ]
                }
                """);

            var store = new JsonEntryStore(dir);
            await store.LoadAsync();
            Assert.Single(store.Entries);
            Assert.Equal(120, store.Entries[0].DurationMinutes);
            Assert.Null(store.LoadWarning);
            Assert.Equal(2, store.LoadedSchemaVersion);
        }
        finally
        {
            Cleanup(dir);
        }
    }

    [Fact]
    public async Task Future_version_backs_up_and_refuses()
    {
        var dir = NewDir();
        try
        {
            var path = Path.Combine(dir, "entries.json");
            var original = """{ "version": 999, "entries": [ { "id": "11111111-1111-1111-1111-111111111111", "date": "2026-10-25", "note": "future", "isStarred": false, "reminderEnabled": false, "reminderLeadMinutes": 5, "createdAt": "2026-10-25T12:00:00", "updatedAt": "2026-10-25T12:00:00" } ] }""";
            await File.WriteAllTextAsync(path, original);

            var store = new JsonEntryStore(dir);
            await store.LoadAsync();

            Assert.Empty(store.Entries);
            Assert.NotNull(store.LoadWarning);
            Assert.Contains("999", store.LoadWarning);
            Assert.Equal(original.Replace("\r\n", "\n").Trim(), (await File.ReadAllTextAsync(path)).Replace("\r\n", "\n").Trim());
            Assert.NotEmpty(Directory.GetFiles(dir, "entries.json.bak-v999-*"));
        }
        finally
        {
            Cleanup(dir);
        }
    }

    [Fact]
    public async Task Corrupt_file_backs_up_and_starts_empty()
    {
        var dir = NewDir();
        try
        {
            var path = Path.Combine(dir, "entries.json");
            await File.WriteAllTextAsync(path, "{ not-json");

            var store = new JsonEntryStore(dir);
            await store.LoadAsync();

            Assert.Empty(store.Entries);
            Assert.NotNull(store.LoadWarning);
            Assert.Equal("{ not-json", await File.ReadAllTextAsync(path));
            Assert.NotEmpty(Directory.GetFiles(dir, "entries.json.bak-v0-*"));
        }
        finally
        {
            Cleanup(dir);
        }
    }

    [Fact]
    public async Task Settings_future_version_refuses_with_defaults()
    {
        var dir = NewDir();
        try
        {
            var path = Path.Combine(dir, "settings.json");
            await File.WriteAllTextAsync(path, """{ "version": 42, "firstDayOfWeek": "Sunday", "monthTitleFormat": "MMMM", "launchAtStartup": true, "theme": "Dark", "window": { "width": 1100, "height": 720, "state": "Normal" } }""");

            var store = new JsonSettingsStore(dir);
            await store.LoadAsync();

            Assert.Equal(DayOfWeek.Monday, store.Settings.FirstDayOfWeek);
            Assert.False(store.Settings.LaunchAtStartup);
            Assert.NotNull(store.LoadWarning);
            Assert.NotEmpty(Directory.GetFiles(dir, "settings.json.bak-v42-*"));
        }
        finally
        {
            Cleanup(dir);
        }
    }

    private static string NewDir()
    {
        var dir = Path.Combine(Path.GetTempPath(), "sc-schema-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        return dir;
    }

    private static void Cleanup(string dir)
    {
        if (Directory.Exists(dir))
            Directory.Delete(dir, true);
    }
}
