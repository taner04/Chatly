using Chatly.Desktop.Models;

namespace Chatly.Desktop.ViewModels.Pages;

public sealed class UserInfoPageViewModel(
    UserSessionContext userContext) : ViewModelBase
{
    public UserSessionContext UserContext { get; } = userContext;
}