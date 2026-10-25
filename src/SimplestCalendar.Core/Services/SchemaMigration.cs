using System.Text.Json.Nodes;

namespace SimplestCalendar.Core.Services;

/// <summary>
/// Ordered schema upgrades. To introduce schema 2: bump <see cref="DataSchema.Current"/>
/// and register a migrator with <c>FromVersion = 1</c> that mutates the <see cref="JsonObject"/>.
/// </summary>
public static class SchemaMigration
{
    private static readonly IReadOnlyList<ISchemaMigrator> Migrators = [];

    public static JsonObject MigrateToCurrent(JsonObject root, int fromVersion)
    {
        var version = fromVersion;
        while (version < DataSchema.Current)
        {
            var step = Migrators.FirstOrDefault(m => m.FromVersion == version)
                ?? throw new InvalidOperationException(
                    $"No schema migrator registered for version {version} → {version + 1}.");
            step.Migrate(root);
            version++;
            root["version"] = version;
        }

        root["version"] = DataSchema.Current;
        return root;
    }
}

public interface ISchemaMigrator
{
    int FromVersion { get; }
    void Migrate(JsonObject root);
}
