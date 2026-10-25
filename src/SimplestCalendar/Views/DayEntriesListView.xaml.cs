using System.Collections.ObjectModel;
using Microsoft.UI.Input;
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
        // ListView often marks pointer events handled; listen anyway for middle-click delete.
        EntriesList.AddHandler(UIElement.PointerPressedEvent, new PointerEventHandler(EntriesList_PointerPressed), true);
        EntriesList.AddHandler(UIElement.PointerReleasedEvent, new PointerEventHandler(EntriesList_PointerReleased), true);
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

    private void EntriesList_PointerPressed(object sender, PointerRoutedEventArgs e) =>
        TryMiddleDelete(e);

    private void EntriesList_PointerReleased(object sender, PointerRoutedEventArgs e) =>
        TryMiddleDelete(e);

    private void TryMiddleDelete(PointerRoutedEventArgs e)
    {
        var point = e.GetCurrentPoint(EntriesList);
        var props = point.Properties;
        var isMiddle = props.IsMiddleButtonPressed ||
                       props.PointerUpdateKind is PointerUpdateKind.MiddleButtonPressed
                           or PointerUpdateKind.MiddleButtonReleased;
        if (!isMiddle)
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
            if (source is FrameworkElement { DataContext: DayEntryListItem fromContext })
                return fromContext;
            if (source is ListViewItem { Content: DayEntryListItem fromContent })
                return fromContent;
            source = VisualTreeHelper.GetParent(source);
        }

        return null;
    }
}
