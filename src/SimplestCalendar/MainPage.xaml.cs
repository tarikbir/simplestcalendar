using Microsoft.UI.Dispatching;
using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using SimplestCalendar.Services;
using SimplestCalendar.ViewModels;

namespace SimplestCalendar;

public sealed partial class MainPage : Page
{
    public MonthViewModel ViewModel { get; }

    private readonly DispatcherQueueTimer _gridResizeTimer;
    private readonly TappedEventHandler _dayCellTappedHandler;
    private double _pendingGridHeight;
    private bool _updatingGridHeight;

    public MainPage()
    {
        ViewModel = new MonthViewModel(AppServices.Current);
        _dayCellTappedHandler = DayCell_Tapped;
        InitializeComponent();

        _gridResizeTimer = DispatcherQueue.CreateTimer();
        _gridResizeTimer.Interval = TimeSpan.FromMilliseconds(32);
        _gridResizeTimer.IsRepeating = false;
        _gridResizeTimer.Tick += (_, _) => ApplyMonthGridItemHeight(_pendingGridHeight);
    }

    private void MonthHost_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        if (e.NewSize.Height <= 0)
            return;

        // Host height is the star-row allocation (shrinks with the window).
        // Debounce MinItemHeight updates to avoid layout reentrancy crashes while dragging.
        _pendingGridHeight = e.NewSize.Height;
        _gridResizeTimer.Stop();
        _gridResizeTimer.Start();
    }

    private void ApplyMonthGridItemHeight(double availableHeight)
    {
        if (_updatingGridHeight || MonthGridLayout is null || availableHeight <= 0)
            return;

        var rows = Math.Max(1, (int)Math.Ceiling(ViewModel.Days.Count / 7.0));
        var spacing = MonthGridLayout.MinRowSpacing * Math.Max(0, rows - 1);
        var target = (availableHeight - spacing) / rows;
        var next = Math.Clamp(target, 72, 400);

        if (Math.Abs(MonthGridLayout.MinItemHeight - next) < 0.5)
            return;

        _updatingGridHeight = true;
        try
        {
            MonthGridLayout.MinItemHeight = next;
        }
        finally
        {
            _updatingGridHeight = false;
        }
    }

    private void DayCell_Loaded(object sender, RoutedEventArgs e)
    {
        // ScrollViewer marks tapped handled in the entry area; still receive the click.
        // Remove+Add keeps ItemsRepeater recycling from stacking duplicate handlers.
        if (sender is not UIElement el)
            return;
        el.RemoveHandler(UIElement.TappedEvent, _dayCellTappedHandler);
        el.AddHandler(UIElement.TappedEvent, _dayCellTappedHandler, handledEventsToo: true);
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

    private async void DayCell_Tapped(object sender, TappedRoutedEventArgs e)
    {
        if (sender is not FrameworkElement { Tag: DayCellViewModel vm })
            return;
        if (!vm.IsInViewedMonth)
            return;

        // Entry lines own their gestures (edit / delete); empty cell opens add.
        if (IsUnderEntryLine(e.OriginalSource as DependencyObject))
            return;

        e.Handled = true;
        var saved = await EntryDialogService.ShowEditorAsync(XamlRoot, vm.Date);
        if (saved)
            ViewModel.Refresh();
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

    private async void EntryLine_RightTapped(object sender, RightTappedRoutedEventArgs e)
    {
        if (sender is not FrameworkElement { Tag: DayEntrySummaryLine line })
            return;

        e.Handled = true;
        var saved = await EntryDialogService.ShowEditorAsync(XamlRoot, line.Entry.Date, line.Entry);
        if (saved)
            ViewModel.Refresh();
    }

    private static bool IsUnderEntryLine(DependencyObject? source)
    {
        while (source is not null)
        {
            if (source is FrameworkElement { Tag: DayEntrySummaryLine })
                return true;
            source = VisualTreeHelper.GetParent(source);
        }

        return false;
    }

    private void SettingsButton_Click(object sender, RoutedEventArgs e) =>
        AppHost.Current?.ShowSettings();
}
