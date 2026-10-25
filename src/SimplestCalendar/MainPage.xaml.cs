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

    private void SettingsButton_Click(object sender, RoutedEventArgs e) =>
        AppHost.Current?.ShowSettings();
}
