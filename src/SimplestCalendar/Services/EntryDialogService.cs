using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using SimplestCalendar.Core.Models;
using SimplestCalendar.Core.Services;
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

        dialog.Opened += (_, _) => editor.FocusNote();

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
}
