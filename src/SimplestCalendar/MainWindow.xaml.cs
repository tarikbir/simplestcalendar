using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using SimplestCalendar.Services;

namespace SimplestCalendar;

public sealed partial class MainWindow : Window
{
    public Frame ContentFrame => RootFrame;
    private bool _loadWarningShown;

    public MainWindow()
    {
        InitializeComponent();

        ExtendsContentIntoTitleBar = true;
        SetTitleBar(AppTitleBar);

        AppWindow.SetIcon("Assets/AppIcon.ico");
        AppWindow.Resize(new Windows.Graphics.SizeInt32(1100, 720));

        RootFrame.Navigate(typeof(MainPage));
        Activated += MainWindow_Activated;
    }

    private async void MainWindow_Activated(object sender, WindowActivatedEventArgs args)
    {
        if (_loadWarningShown || args.WindowActivationState == WindowActivationState.Deactivated)
            return;
        if (string.IsNullOrWhiteSpace(AppServices.Current.DataLoadWarning))
            return;

        _loadWarningShown = true;
        var root = Content?.XamlRoot ?? RootFrame.XamlRoot;
        if (root is null)
        {
            _loadWarningShown = false;
            return;
        }

        var dialog = new ContentDialog
        {
            Title = "Data could not be loaded",
            Content = AppServices.Current.DataLoadWarning,
            CloseButtonText = "OK",
            XamlRoot = root
        };
        await dialog.ShowAsync();
    }
}
