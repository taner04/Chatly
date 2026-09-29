using System.Runtime.CompilerServices;
using Chatly.Desktop.ViewModels.Pages;
using Chatly.Desktop.ViewModels.Pages.ChatPage;
using Chatly.Desktop.ViewModels.Pages.UserPage;
using Chatly.Desktop.ViewModels.Pages.UserPage.Popups;
using Chatly.Desktop.ViewModels.Pages.UserPage.Tabs;
using Chatly.Desktop.ViewModels.Popups;
using Chatly.Desktop.Views;
using Microsoft.Extensions.DependencyInjection;

namespace Chatly.Desktop.UnitTests.Tests.Views;

public sealed class ViewLocatorTests
{
    [Theory]
    [InlineData(typeof(UserPageViewModel))]
    [InlineData(typeof(OnlineFriendsTabPageViewModel))]
    [InlineData(typeof(AllFriendsTabPageViewModel))]
    [InlineData(typeof(PendingFriendRequestsTabPageViewModel))]
    [InlineData(typeof(ChatPageViewModel))]
    [InlineData(typeof(SettingsPageViewModel))]
    [InlineData(typeof(OnboardingPopupViewModel))]
    [InlineData(typeof(UserInfoPopupViewModel))]
    [InlineData(typeof(AddFriendPopupOverlayViewModel))]
    public void Constructor_Should_FindView_When_ViewModelHasView(Type viewModelType)
    {
        var viewLocator = new ViewLocator(new ServiceCollection().BuildServiceProvider());

        viewLocator.Match(RuntimeHelpers.GetUninitializedObject(viewModelType))
            .Should().BeTrue();
    }
}