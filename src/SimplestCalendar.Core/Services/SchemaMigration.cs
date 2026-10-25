using System.Text.Json.Nodes;

namespace SimplestCalendar.Core.Services;

/// <summary>
/// Ordered schema upgrades. To introduce schema N+1: bump <see cref="DataSchema.Current"/>
/// and register a migrator with <c>FromVersion = N</c> that mutates the <see cref="JsonObject"/>.
/// </summary>
public static class SchemaMigration
{
    private static readonly IReadOnlyList<ISchemaMigrator> Migrators =
    [
        new V1ToV2DurationMigrator()
    ];

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

/// <summary>Converts whole-hour <c>durationHours</c> to <c>durationMinutes</c>.</summary>
public sealed class V1ToV2DurationMigrator : ISchemaMigrator
{
    public int FromVersion => 1;

    public void Migrate(JsonObject root)
    {
        if (root["entries"] is not JsonArray entries)
            return;

        foreach (var node in entries)
        {
            if (node is not JsonObject entry)
                continue;

            if (entry["durationHours"] is JsonValue hoursNode && hoursNode.TryGetValue<int>(out var hours))
            {
                entry["durationMinutes"] = hours * 60;
                entry.Remove("durationHours");
            }
            else
            {
                entry.Remove("durationHours");
            }
        }
    }
}
