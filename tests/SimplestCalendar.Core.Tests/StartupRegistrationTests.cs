using SimplestCalendar.Core.Models;
using SimplestCalendar.Core.Services;

namespace SimplestCalendar.Core.Tests;

public class StartupRegistrationTests
{
    [Fact]
    public async Task LaunchAtStartup_persists_in_settings_json()
    {
        var dir = Path.Combine(Path.GetTempPath(), "sc-start-" + Guid.NewGuid().ToString("N"));
        try
        {
            var store = new JsonSettingsStore(dir);
            await store.LoadAsync();
            Assert.False(store.Settings.LaunchAtStartup);
            store.Settings.LaunchAtStartup = true;
            store.Settings.Theme = AppTheme.Dark;
            await store.SaveAsync();

            var loaded = new JsonSettingsStore(dir);
            await loaded.LoadAsync();
            Assert.True(loaded.Settings.LaunchAtStartup);
        }
        finally
        {
            if (Directory.Exists(dir))
                Directory.Delete(dir, true);
        }
    }
}
