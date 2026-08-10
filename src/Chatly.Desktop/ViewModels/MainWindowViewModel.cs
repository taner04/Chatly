using System.Collections.Generic;
using Chatly.Desktop.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using FluentIcons.Common;

namespace Chatly.Desktop.ViewModels;

public sealed partial class MainWindowViewModel : ViewModelBase
{
    public List<NavigationItemViewModel> NavigationItems { get; set; } =
    [
        new("Home", Symbol.Home, typeof(HomePageView)) { IsSelected = true },
        new("Login", Symbol.Key, typeof(LoginPageView))
    ];
    
    [ObservableProperty] public partial NavigationItemViewModel CurrentNavigationItem { get; set; }
}
