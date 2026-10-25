using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using SimplestCalendar.Services;
using SimplestCalendar.ViewModels;

namespace SimplestCalendar;

public sealed partial class MainPage : Page
{
    public MonthViewModel ViewModel { get; }

    public MainPage()
    {
        ViewModel = new MonthViewModel(AppServices.Current);
        InitializeComponent();
    }

    private void MonthGrid_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        if (e.NewSize.Height <= 0 || MonthGridLayout is null)
            return;

        var rows = Math.Max(1, (int)Math.Ceiling(ViewModel.Days.Count / 7.0));
        var spacing = MonthGridLayout.MinRowSpacing * Math.Max(0, rows - 1);
        var target = (e.NewSize.Height - spacing) / rows;
        MonthGridLayout.MinItemHeight = Math.Max(72, target);
    }

    private void DayCell_PointerEntered(object sender, PointerRoutedEventArgs e)
    {
        if (sender is FrameworkElement { Tag: DayCellViewModel vm })
            vm.IsHovered = true;
    }

    private void DayCell_PointerExited(object sender, PointerRoutedEventArgs e)
    {
        if (sender is FrameworkElement { Tag: DayCellViewModel vm })
            vm.IsHovered = false;
    }

    private async void DayCell_PointerPressed(object sender, PointerRoutedEventArgs e)
    {
        if (sender is not FrameworkElement { Tag: DayCellViewModel vm })
            return;
        if (!vm.IsInViewedMonth)
            return;

        var point = e.GetCurrentPoint((UIElement)sender);
        var props = point.Properties;

        // Middle-click on empty cell / day number: do nothing (do not open add).
        if (props.IsMiddleButtonPressed ||
            props.PointerUpdateKind is PointerUpdateKind.MiddleButtonPressed
                or PointerUpdateKind.MiddleButtonReleased)
        {
            e.Handled = true;
            return;
        }

        // Right-click handled by RightTapped.
        if (props.IsRightButtonPressed ||
            props.PointerUpdateKind is PointerUpdateKind.RightButtonPressed
                or PointerUpdateKind.RightButtonReleased)
            return;

        // Left-click only opens add.
        if (!props.IsLeftButtonPressed &&
            props.PointerUpdateKind != PointerUpdateKind.LeftButtonPressed)
            return;

        e.Handled = true;
        var saved = await EntryDialogService.ShowEditorAsync(XamlRoot, vm.Date);
        if (saved)
            ViewModel.Refresh();
    }

    private async void DayCell_RightTapped(object sender, RightTappedRoutedEventArgs e)
    {
        if (sender is not FrameworkElement { Tag: DayCellViewModel vm })
            return;
        if (!vm.IsInViewedMonth)
            return;

        e.Handled = true;
        await EntryDialogService.ShowDayListAsync(XamlRoot, vm.Date, () => ViewModel.Refresh());
    }

    private void EntryLine_PointerEntered(object sender, PointerRoutedEventArgs e)
    {
        if (sender is FrameworkElement { Tag: DayEntrySummaryLine line })
            line.IsEntryHovered = true;
    }

    private void EntryLine_PointerExited(object sender, PointerRoutedEventArgs e)
    {
        if (sender is FrameworkElement { Tag: DayEntrySummaryLine line })
            line.IsEntryHovered = false;
    }

    private async void EntryLine_PointerPressed(object sender, PointerRoutedEventArgs e)
    {
        if (sender is not FrameworkElement { Tag: DayEntrySummaryLine line })
            return;

        var point = e.GetCurrentPoint((UIElement)sender);
        var props = point.Properties;
        var isMiddle = props.IsMiddleButtonPressed ||
                       props.PointerUpdateKind is PointerUpdateKind.MiddleButtonPressed
                           or PointerUpdateKind.MiddleButtonReleased;
        if (!isMiddle)
            return;

        e.Handled = true;
        await ViewModel.DeleteEntryAsync(line.Entry);
    }

    private void SettingsButton_Click(object sender, RoutedEventArgs e) =>
        AppHost.Current?.ShowSettings();
}
