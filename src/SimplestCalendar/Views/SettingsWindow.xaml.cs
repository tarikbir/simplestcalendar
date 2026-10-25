using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using SimplestCalendar.ViewModels;

namespace SimplestCalendar.Views;

public sealed partial class SettingsWindow : Window
{
    public SettingsViewModel ViewModel { get; }

    public SettingsWindow()
    {
        ViewModel = new SettingsViewModel();
        InitializeComponent();
        UpdatePreview();
        ViewModel.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName is nameof(SettingsViewModel.MonthTitleFormat) or nameof(SettingsViewModel.PreviewTitle))
                UpdatePreview();
        };
        AppWindow.Resize(new Windows.Graphics.SizeInt32(720, 520));
        Nav.SelectedItem = Nav.MenuItems[0];
    }

    private void UpdatePreview() => PreviewText.Text = $"Preview: {ViewModel.PreviewTitle}";

    private void Nav_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
    {
        var tag = (args.SelectedItem as NavigationViewItem)?.Tag as string;
        CalendarPanel.Visibility = tag == "calendar" ? Visibility.Visible : Visibility.Collapsed;
        GeneralPanel.Visibility = tag == "general" ? Visibility.Visible : Visibility.Collapsed;
        AppearancePanel.Visibility = tag == "appearance" ? Visibility.Visible : Visibility.Collapsed;
        AboutPanel.Visibility = tag == "about" ? Visibility.Visible : Visibility.Collapsed;
    }

    private async void OpenDataFolder_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            Directory.CreateDirectory(ViewModel.DataPath);
            await Windows.System.Launcher.LaunchFolderPathAsync(ViewModel.DataPath);
        }
        catch
        {
            ViewModel.StatusMessage = "Could not open data folder.";
        }
    }
}
