using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using SimplestCalendar.Core.Models;

namespace SimplestCalendar.Core.Services;

public interface IEntryStore
{
    IReadOnlyList<Entry> Entries { get; }
    string? LoadWarning { get; }
    int LoadedSchemaVersion { get; }
    Task LoadAsync(CancellationToken cancellationToken = default);
    Task SaveAsync(CancellationToken cancellationToken = default);
    void ReplaceAll(IEnumerable<Entry> entries);
    void Upsert(Entry entry);
    void Remove(Guid id);
    int WipeBefore(DateOnly monthStart);
}

public sealed class JsonEntryStore : IEntryStore
{
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly string _filePath;
    private List<Entry> _entries = [];

    public JsonEntryStore(string dataDirectory)
    {
        Directory.CreateDirectory(dataDirectory);
        _filePath = Path.Combine(dataDirectory, "entries.json");
    }

    public IReadOnlyList<Entry> Entries => _entries;
    public string? LoadWarning { get; private set; }
    public int LoadedSchemaVersion { get; private set; } = DataSchema.Current;

    public async Task LoadAsync(CancellationToken cancellationToken = default)
    {
        LoadWarning = null;
        LoadedSchemaVersion = DataSchema.Current;

        if (!File.Exists(_filePath))
        {
            _entries = [];
            return;
        }

        string text;
        try
        {
            text = await File.ReadAllTextAsync(_filePath, cancellationToken);
        }
        catch (Exception ex)
        {
            RefuseWithBackup(versionHint: 0, $"Could not read entries.json: {ex.Message}");
            _entries = [];
            return;
        }

        JsonNode? node;
        try
        {
            node = JsonNode.Parse(text);
        }
        catch (JsonException)
        {
            RefuseWithBackup(versionHint: 0, "entries.json is corrupt. A backup was kept; starting with an empty calendar.");
            _entries = [];
            return;
        }

        if (node is not JsonObject root)
        {
            RefuseWithBackup(versionHint: 0, "entries.json has an unexpected shape. A backup was kept; starting with an empty calendar.");
            _entries = [];
            return;
        }

        var version = ReadVersion(root);
        if (version < 1)
            version = 1;
        if (version > DataSchema.Current)
        {
            RefuseWithBackup(version, $"entries.json schema v{version} is newer than this app supports (v{DataSchema.Current}). Original file left intact; a backup copy was made. Calendar started empty.");
            _entries = [];
            return;
        }

        if (version < DataSchema.Current)
        {
            try
            {
                SchemaMigration.MigrateToCurrent(root, version);
                version = DataSchema.Current;
                await WriteRootAsync(root, cancellationToken);
            }
            catch (Exception ex)
            {
                RefuseWithBackup(version, $"Could not migrate entries.json: {ex.Message}");
                _entries = [];
                return;
            }
        }

        try
        {
            var doc = root.Deserialize<EntriesDocument>(Options);
            _entries = doc?.Entries?.ToList() ?? [];
            LoadedSchemaVersion = DataSchema.Current;
        }
        catch (JsonException)
        {
            RefuseWithBackup(version, "entries.json could not be parsed after migration. A backup was kept; starting empty.");
            _entries = [];
        }
    }

    public async Task SaveAsync(CancellationToken cancellationToken = default)
    {
        var doc = new EntriesDocument
        {
            Version = DataSchema.Current,
            AppVersion = DataSchema.AppVersion,
            Entries = _entries
        };
        await AtomicWriteAsync(doc, cancellationToken);
        LoadedSchemaVersion = DataSchema.Current;
    }

    public void ReplaceAll(IEnumerable<Entry> entries) =>
        _entries = entries.ToList();

    public void Upsert(Entry entry)
    {
        var index = _entries.FindIndex(e => e.Id == entry.Id);
        if (index >= 0)
            _entries[index] = entry;
        else
            _entries.Add(entry);
    }

    public void Remove(Guid id) =>
        _entries.RemoveAll(e => e.Id == id);

    public int WipeBefore(DateOnly monthStart)
    {
        var before = _entries.Count;
        _entries = _entries.Where(e => e.Date >= monthStart).ToList();
        return before - _entries.Count;
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

    private async Task AtomicWriteAsync(EntriesDocument doc, CancellationToken cancellationToken)
    {
        var dir = Path.GetDirectoryName(_filePath)!;
        Directory.CreateDirectory(dir);
        var temp = _filePath + ".tmp";
        await using (var stream = File.Create(temp))
        {
            await JsonSerializer.SerializeAsync(stream, doc, Options, cancellationToken);
        }

        File.Copy(temp, _filePath, overwrite: true);
        File.Delete(temp);
    }

    private sealed class EntriesDocument
    {
        public int Version { get; set; } = DataSchema.Current;
        public string? AppVersion { get; set; }
        public List<Entry> Entries { get; set; } = [];
    }
}
