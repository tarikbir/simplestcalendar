using SimplestCalendar.Core.Calendar;
using SimplestCalendar.Core.Models;
using SimplestCalendar.Core.Services;
using SimplestCalendar.Core.Sorting;

namespace SimplestCalendar.Core.Tests;

/// <summary>
/// End-to-end domain smoke covering quickstart checklist items that do not need UI.
/// </summary>
public class SmokeWorkflowTests
{
    [Fact]
    public async Task Whiteboard_day_flow_create_overlap_split_edit_delete_wipe()
    {
        var dir = Path.Combine(Path.GetTempPath(), "sc-smoke-" + Guid.NewGuid().ToString("N"));
        try
        {
            var store = new JsonEntryStore(dir);
            var today = new DateOnly(2026, 10, 25);

            // Untimed note
            EntryWorkflow.Save(store, new EntryDraft(today, "Buy chalk", null, null, false, false, 5), null, out _);

            // Timed with overlap warning
            EntryWorkflow.Save(
                store,
                new EntryDraft(today, "Class A", new TimeOnly(10, 0), 2, true, true, 5),
                null,
                out var overlap1);
            Assert.False(overlap1);
            EntryWorkflow.Save(
                store,
                new EntryDraft(today, "Class B", new TimeOnly(11, 0), 2, false, false, 5),
                null,
                out var overlap2);
            Assert.True(overlap2);

            // Midnight split
            var split = EntryWorkflow.Save(
                store,
                new EntryDraft(today, "Night work", new TimeOnly(23, 0), 3, false, true, 5),
                null,
                out _);
            Assert.Equal(2, split.Count);
            Assert.True(split[0].ReminderEnabled);
            Assert.False(split[1].ReminderEnabled);

            await store.SaveAsync();
            Assert.True(File.Exists(Path.Combine(dir, "entries.json")));

            var ordered = EntrySort.OrderToList(store.Entries.Where(e => e.Date == today));
            Assert.Contains(ordered, e => e.IsStarred && e.Note == "Class A");
            Assert.Equal("Buy chalk", ordered.First(e => e.IsUntimed && !e.IsStarred).Note);

            // Edit + delete
            var toEdit = store.Entries.First(e => e.Note == "Class B");
            EntryWorkflow.Save(
                store,
                new EntryDraft(today, "Class B edited", new TimeOnly(14, 0), null, false, false, 5),
                toEdit,
                out _);
            Assert.Contains(store.Entries, e => e.Note == "Class B edited");

            EntryWorkflow.Delete(store, store.Entries.First(e => e.Note == "Night work"));
            Assert.DoesNotContain(store.Entries, e => e.Note == "Night work");

            // Wipe past month
            store.Upsert(new Entry { Date = new DateOnly(2026, 9, 1), Note = "old" });
            Assert.Equal(1, store.WipeBefore(new DateOnly(2026, 10, 1)));
            Assert.DoesNotContain(store.Entries, e => e.Note == "old");

            // Navigation + title
            Assert.Equal("October", MonthTitleFormatter.Format(new DateOnly(2026, 10, 1), "MMMM"));
            Assert.False(MonthNavigation.CanGoNext(new DateOnly(2026, 10, 1).AddMonths(11), today));
            Assert.Equal(42, MonthGridBuilder.Build(new DateOnly(2026, 10, 1), DayOfWeek.Monday, today).Count);

            // Reminder skip/fire
            var scheduler = new ReminderScheduler();
            var processStart = new DateTime(2026, 10, 25, 12, 0, 0);
            var live = new Entry
            {
                Id = Guid.NewGuid(),
                Date = today,
                Note = "soon",
                StartTime = new TimeOnly(12, 10),
                ReminderEnabled = true,
                ReminderLeadMinutes = 5
            };
            Assert.Single(scheduler.CollectDue([live], new DateTime(2026, 10, 25, 12, 6, 0), processStart));
        }
        finally
        {
            if (Directory.Exists(dir))
                Directory.Delete(dir, true);
        }
    }
}
