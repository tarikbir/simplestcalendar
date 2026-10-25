using System.Text.Json;
using System.Text.Json.Serialization;
using SimplestCalendar.Core.Models;

namespace SimplestCalendar.Core.Services;

public interface IEntryStore
{
    IReadOnlyList<Entry> Entries { get; }
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

    public async Task LoadAsync(CancellationToken cancellationToken = default)
    {
        if (!File.Exists(_filePath))
        {
            _entries = [];
            return;
        }

        await using var stream = File.OpenRead(_filePath);
        var doc = await JsonSerializer.DeserializeAsync<EntriesDocument>(stream, Options, cancellationToken);
        _entries = doc?.Entries?.ToList() ?? [];
    }

    public async Task SaveAsync(CancellationToken cancellationToken = default)
    {
        var dir = Path.GetDirectoryName(_filePath)!;
        Directory.CreateDirectory(dir);
        var temp = _filePath + ".tmp";
        var doc = new EntriesDocument { Version = 1, Entries = _entries };
        await using (var stream = File.Create(temp))
        {
            await JsonSerializer.SerializeAsync(stream, doc, Options, cancellationToken);
        }

        File.Copy(temp, _filePath, overwrite: true);
        File.Delete(temp);
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

    private sealed class EntriesDocument
    {
        public int Version { get; set; } = 1;
        public List<Entry> Entries { get; set; } = [];
    }
}
