using System.Collections.Generic;
using Chatly.Desktop.Views;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Chatly.Desktop.ViewModels;

public sealed partial class MainWindowViewModel : ViewModelBase
{
    public List<NavigationItemViewModel> NavigationItems { get; set; } =
    [
        new("Home", typeof(HomePageView)),
        new("Login", typeof(LoginPageView))
    ];
    
    [ObservableProperty] public partial NavigationItemViewModel CurrentNavigationItem { get; set; }
}