using Avalonia.Controls;
using Chatly.Desktop.Abstractions;
using Chatly.Desktop.ViewModels;

namespace Chatly.Desktop.Views;

public partial class LoginPageView : UserControl, INavigavablePage
{
    private bool _started;

    public LoginPageView(LoginPageViewModel viewModel)
    {        
        ViewModel = viewModel;
        DataContext = ViewModel;
        
        InitializeComponent();

        
        AttachedToVisualTree += async (_, _) =>
        {
            if (_started)
            {
                return;
            }

            _started = true;
            await ViewModel.StartAuthenticationAsync();
        };
    }
    
    public LoginPageViewModel ViewModel {get;}
}
