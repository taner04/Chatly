using System.Collections.Generic;
using Chatly.Desktop.Infrastructure.Auth;
using Chatly.Desktop.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using FluentIcons.Common;

namespace Chatly.Desktop.ViewModels;

public sealed partial class MainWindowViewModel(UserContext userContext) : ViewModelBase
{
    public UserContext UserContext { get; } = userContext;

    public List<NavigationItemViewModel> NavigationItems { get; set; } =
    [
        new("Home", Symbol.Home, typeof(HomePage)) { IsSelected = true },
    ];
    
    [ObservableProperty] public partial NavigationItemViewModel CurrentNavigationItem { get; set; }
}
