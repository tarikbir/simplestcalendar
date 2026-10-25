using System.Collections.ObjectModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using SimplestCalendar.ViewModels;

namespace SimplestCalendar.Views;

public sealed partial class DayEntriesListView : UserControl
{
    public ObservableCollection<DayEntryListItem> Items { get; }

    public event EventHandler<DayEntryListItem>? EditRequested;
    public event EventHandler<DayEntryListItem>? DeleteRequested;

    public DayEntriesListView(IEnumerable<DayEntryListItem> items)
    {
        Items = new ObservableCollection<DayEntryListItem>(items);
        InitializeComponent();
    }

    private void EntriesList_ItemClick(object sender, ItemClickEventArgs e)
    {
        if (e.ClickedItem is DayEntryListItem item)
            EditRequested?.Invoke(this, item);
    }

    private void DeleteButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement { Tag: DayEntryListItem item })
            DeleteRequested?.Invoke(this, item);
    }

    private void EntriesList_PointerPressed(object sender, PointerRoutedEventArgs e)
    {
        var point = e.GetCurrentPoint(EntriesList);
        if (!point.Properties.IsMiddleButtonPressed)
            return;

        var item = FindListItem(e.OriginalSource as DependencyObject);
        if (item is null)
            return;

        EntriesList.SelectedItem = item;
        DeleteRequested?.Invoke(this, item);
        e.Handled = true;
    }

    private static DayEntryListItem? FindListItem(DependencyObject? source)
    {
        while (source is not null)
        {
            if (source is ListViewItem { Content: DayEntryListItem item })
                return item;
            source = VisualTreeHelper.GetParent(source);
        }

        return null;
    }
}
