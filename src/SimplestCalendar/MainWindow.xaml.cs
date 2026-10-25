using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace SimplestCalendar;

public sealed partial class MainWindow : Window
{
    public Frame ContentFrame => RootFrame;

    public MainWindow()
    {
        InitializeComponent();

        ExtendsContentIntoTitleBar = true;
        SetTitleBar(AppTitleBar);

        AppWindow.SetIcon("Assets/AppIcon.ico");
        AppWindow.Resize(new Windows.Graphics.SizeInt32(1100, 720));

        RootFrame.Navigate(typeof(MainPage));
    }
}
