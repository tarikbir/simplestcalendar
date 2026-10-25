using SimplestCalendar.Core.Models;
using SimplestCalendar.Core.Services;

namespace SimplestCalendar.Core.Tests;

public class EntryWorkflowTests
{
    [Fact]
    public async Task Create_update_delete_segments_independently()
    {
        var dir = Path.Combine(Path.GetTempPath(), "sc-wf-" + Guid.NewGuid().ToString("N"));
        try
        {
            var store = new JsonEntryStore(dir);
            var draft = new EntryDraft(
                new DateOnly(2026, 10, 25),
                "Class",
                new TimeOnly(23, 0),
                180,
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
            var second = loaded.Entries.First(e => e.SplitIndex == 1);
            var editDraft = new EntryDraft(
                first.Date,
                "Class updated",
                first.StartTime,
                first.DurationMinutes,
                first.IsStarred,
                first.ReminderEnabled,
                first.ReminderLeadMinutes);
            EntryWorkflow.Save(loaded, editDraft, first, out _);
            await loaded.SaveAsync();

            // Editing first segment only replaces that row; sibling remains with old note.
            Assert.Equal(2, loaded.Entries.Count);
            Assert.Contains(loaded.Entries, e => e.Note == "Class updated");
            Assert.Contains(loaded.Entries, e => e.Id == second.Id && e.Note == "Class");

            EntryWorkflow.Delete(loaded, loaded.Entries.First(e => e.Note == "Class updated"));
            Assert.Single(loaded.Entries);
            Assert.Equal("Class", loaded.Entries[0].Note);
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
            new DateOnly(2026, 10, 1), "x", null, 120, false, false, 5)));
        Assert.Null(EntryWorkflow.Validate(new EntryDraft(
            new DateOnly(2026, 10, 1), "x", new TimeOnly(10, 0), 120, false, false, 5)));
        Assert.NotNull(EntryWorkflow.Validate(new EntryDraft(
            new DateOnly(2026, 10, 1), "x", new TimeOnly(10, 0), 0, false, false, 5)));
    }
}
