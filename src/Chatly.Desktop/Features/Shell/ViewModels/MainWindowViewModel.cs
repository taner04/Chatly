using System.Collections.Generic;
using System.Diagnostics.Contracts;
using Chatly.Desktop.Features.Home.Views;
using Chatly.Desktop.Features.Popup.ViewModels;
using Chatly.Desktop.Features.Users;
using Chatly.Desktop.Shared.ViewModels.Base;
using CommunityToolkit.Mvvm.ComponentModel;
using FluentIcons.Common;

namespace Chatly.Desktop.Features.Shell.ViewModels;

public sealed partial class MainWindowViewModel(
    UserSessionContext userContext,
    PopupOverlayViewModel popup) : ViewModelBase
{
    public UserSessionContext UserContext { get; } = userContext;

    public PopupOverlayViewModel Popup { get; } = popup;

    public List<NavigationItemViewModel> NavigationItems { get; set; } =
    [
        new("Home", Symbol.Home, typeof(HomePage)) { IsSelected = true }
    ];

    public List<NavigationItemViewModel> FooterNavigationItems { get; set; } =
    [
        new("User", Symbol.People, typeof(UserInfoPage))
    ];

    [ObservableProperty] public partial NavigationItemViewModel CurrentNavigationItem { get; set; }
}
