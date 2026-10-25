using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using SimplestCalendar.Core.Models;
using SimplestCalendar.Core.Services;
using SimplestCalendar.Core.Sorting;
using SimplestCalendar.ViewModels;
using SimplestCalendar.Views;

namespace SimplestCalendar.Services;

public static class EntryDialogService
{
    public static async Task<bool> ShowEditorAsync(XamlRoot xamlRoot, DateOnly date, Entry? editing = null)
    {
        var vm = new EntryEditorViewModel(date, editing);
        var editor = new EntryEditorView(vm);

        var dialog = new ContentDialog
        {
            Title = vm.DialogTitle,
            Content = editor,
            PrimaryButtonText = "Save",
            CloseButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Primary,
            XamlRoot = xamlRoot,
            RequestedTheme = ElementTheme.Dark
        };

        dialog.PrimaryButtonClick += async (_, args) =>
        {
            var deferral = args.GetDeferral();
            try
            {
                if (!vm.TryBuildDraft(out var draft))
                {
                    args.Cancel = true;
                    return;
                }

                var store = AppServices.Current.Entries;
                EntryWorkflow.Save(store, draft, editing, out var overlap);
                await store.SaveAsync();

                if (overlap)
                {
                    var warn = new ContentDialog
                    {
                        Title = "Overlap",
                        Content = "This timed entry overlaps another on the same day. It was saved anyway.",
                        CloseButtonText = "OK",
                        XamlRoot = xamlRoot,
                        RequestedTheme = ElementTheme.Dark
                    };
                    await warn.ShowAsync();
                }
            }
            finally
            {
                deferral.Complete();
            }
        };

        var result = await dialog.ShowAsync();
        return result == ContentDialogResult.Primary;
    }

    public static async Task ShowDayListAsync(XamlRoot xamlRoot, DateOnly date, Action refresh)
    {
        var store = AppServices.Current.Entries;
        var items = EntrySort.Order(store.Entries.Where(e => e.Date == date))
            .Select(e => new DayEntryListItem(e))
            .ToList();
        if (items.Count == 0)
            return;

        var list = new DayEntriesListView(items);
        var dialog = new ContentDialog
        {
            Title = $"Entries — {date:dddd, MMM d}",
            Content = list,
            CloseButtonText = "Close",
            XamlRoot = xamlRoot,
            RequestedTheme = ElementTheme.Dark
        };

        list.EditRequested += async (_, item) =>
        {
            dialog.Hide();
            var saved = await ShowEditorAsync(xamlRoot, item.Entry.Date, item.Entry);
            if (saved)
                refresh();
        };

        list.DeleteRequested += async (_, item) =>
        {
            EntryWorkflow.Delete(store, item.Entry);
            await store.SaveAsync();
            items.Remove(item);
            list.Items.Remove(item);
            refresh();
            if (list.Items.Count == 0)
                dialog.Hide();
        };

        await dialog.ShowAsync();
    }
}
