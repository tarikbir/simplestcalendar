using SimplestCalendar.Core.Models;
using SimplestCalendar.Core.Services;

namespace SimplestCalendar.Core.Tests;

public class JsonStoreTests
{
    [Fact]
    public async Task Entry_store_round_trip_and_wipe()
    {
        var dir = Path.Combine(Path.GetTempPath(), "sc-test-" + Guid.NewGuid().ToString("N"));
        try
        {
            var store = new JsonEntryStore(dir);
            store.Upsert(new Entry { Date = new DateOnly(2026, 9, 1), Note = "old" });
            store.Upsert(new Entry { Date = new DateOnly(2026, 10, 5), Note = "keep" });
            await store.SaveAsync();

            var loaded = new JsonEntryStore(dir);
            await loaded.LoadAsync();
            Assert.Equal(2, loaded.Entries.Count);

            var removed = loaded.WipeBefore(new DateOnly(2026, 10, 1));
            Assert.Equal(1, removed);
            await loaded.SaveAsync();
            Assert.Single(loaded.Entries);
            Assert.Equal("keep", loaded.Entries[0].Note);
        }
        finally
        {
            if (Directory.Exists(dir))
                Directory.Delete(dir, true);
        }
    }

    [Fact]
    public async Task Settings_defaults_and_round_trip()
    {
        var dir = Path.Combine(Path.GetTempPath(), "sc-set-" + Guid.NewGuid().ToString("N"));
        try
        {
            var store = new JsonSettingsStore(dir);
            await store.LoadAsync();
            Assert.Equal(DayOfWeek.Monday, store.Settings.FirstDayOfWeek);
            Assert.False(store.Settings.LaunchAtStartup);

            store.Settings.FirstDayOfWeek = DayOfWeek.Sunday;
            await store.SaveAsync();

            var loaded = new JsonSettingsStore(dir);
            await loaded.LoadAsync();
            Assert.Equal(DayOfWeek.Sunday, loaded.Settings.FirstDayOfWeek);
        }
        finally
        {
            if (Directory.Exists(dir))
                Directory.Delete(dir, true);
        }
    }
}
