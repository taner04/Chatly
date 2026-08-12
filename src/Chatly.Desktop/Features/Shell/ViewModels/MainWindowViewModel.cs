using System.Collections.Generic;
using Chatly.Desktop.Features.Authentication.Services;
using Chatly.Desktop.Features.Home.Views;
using Chatly.Desktop.Features.Users;
using Chatly.Desktop.Shared.ViewModels.Base;
using CommunityToolkit.Mvvm.ComponentModel;
using FluentIcons.Common;

namespace Chatly.Desktop.Features.Shell.ViewModels;

public sealed partial class MainWindowViewModel(UserSessionContext userContext) : ViewModelBase
{
    public UserSessionContext UserContext { get; } = userContext;

    public List<NavigationItemViewModel> NavigationItems { get; set; } =
    [
        new("Home", Symbol.Home, typeof(HomePage)) { IsSelected = true }
    ];

    [ObservableProperty] public partial NavigationItemViewModel CurrentNavigationItem { get; set; }
}