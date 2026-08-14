using System.Collections.Generic;
using Chatly.Desktop.Models;
using Chatly.Desktop.ViewModels.Overlays;
using Chatly.Desktop.Views.Pages;
using CommunityToolkit.Mvvm.ComponentModel;
using FluentIcons.Common;

namespace Chatly.Desktop.ViewModels.Windows;

public sealed partial class MainWindowViewModel(
    UserSessionContext userContext,
    PopupOverlayHostViewModel popup) : ViewModelBase
{
    public UserSessionContext UserContext { get; } = userContext;

    public PopupOverlayHostViewModel Popup { get; } = popup;

    public List<NavigationItemViewModel> NavigationItems { get; } =
    [
        new("Home", Symbol.Home, typeof(HomePage)) { IsSelected = true }
    ];

    public List<NavigationItemViewModel> FooterNavigationItems { get; } =
    [
        new("User", Symbol.People, typeof(UserInfoPage))
    ];

    [ObservableProperty] public partial NavigationItemViewModel CurrentNavigationItem { get; set; }
}
