using Avalonia.Controls;
using Avalonia.Input;
using Chatly.Desktop.Features.Startup.ViewModels;

namespace Chatly.Desktop.Features.Startup.Views;

public partial class SplashScreenWindow : Window
{
    public SplashScreenWindow(SplashScreenViewModel viewModel)
    {
        ViewModel = viewModel;
        DataContext = this;

        InitializeComponent();
    }

    public SplashScreenViewModel ViewModel { get; }

    private void SplashSurface_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (!CancelButton.IsPointerOver &&
            e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            BeginMoveDrag(e);
        }
    }
}