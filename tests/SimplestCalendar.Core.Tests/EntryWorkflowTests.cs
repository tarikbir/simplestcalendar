using SimplestCalendar.Core.Models;
using SimplestCalendar.Core.Services;

namespace SimplestCalendar.Core.Tests;

public class EntryWorkflowTests
{
    [Fact]
    public async Task Create_update_delete_and_split_persist()
    {
        var dir = Path.Combine(Path.GetTempPath(), "sc-wf-" + Guid.NewGuid().ToString("N"));
        try
        {
            var store = new JsonEntryStore(dir);
            var draft = new EntryDraft(
                new DateOnly(2026, 10, 25),
                "Class",
                new TimeOnly(23, 0),
                3,
                true,
                true,
                5);

            var created = EntryWorkflow.Save(store, draft, null, out var overlap);
            Assert.False(overlap);
            Assert.Equal(2, created.Count);
            await store.SaveAsync();

            var loaded = new JsonEntryStore(dir);
            await loaded.LoadAsync();
            Assert.Equal(2, loaded.Entries.Count);

            var first = loaded.Entries.First(e => e.SplitIndex == 0);
            var editDraft = draft with { Note = "Class updated", DurationHours = 2 };
            EntryWorkflow.Save(loaded, editDraft, first, out _);
            await loaded.SaveAsync();

            // 23:00 + 2h still crosses midnight → still 2 segments, old group replaced
            Assert.Equal(2, loaded.Entries.Count);
            Assert.All(loaded.Entries, e => Assert.Equal("Class updated", e.Note));

            EntryWorkflow.Delete(loaded, loaded.Entries[0]);
            Assert.Empty(loaded.Entries);
        }
        finally
        {
            if (Directory.Exists(dir))
                Directory.Delete(dir, true);
        }
    }

    [Fact]
    public void Validate_requires_note_and_start_for_duration()
    {
        Assert.NotNull(EntryWorkflow.Validate(new EntryDraft(
            new DateOnly(2026, 10, 1), "", null, null, false, false, 5)));
        Assert.NotNull(EntryWorkflow.Validate(new EntryDraft(
            new DateOnly(2026, 10, 1), "x", null, 2, false, false, 5)));
        Assert.Null(EntryWorkflow.Validate(new EntryDraft(
            new DateOnly(2026, 10, 1), "x", new TimeOnly(10, 0), 2, false, false, 5)));
    }
}
