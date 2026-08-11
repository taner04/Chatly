using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Chatly.Desktop.ViewModels;

namespace Chatly.Desktop.Views;

public partial class SplashScreen : Window
{
    public SplashScreen(SplashScreenViewModel viewModel)
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
