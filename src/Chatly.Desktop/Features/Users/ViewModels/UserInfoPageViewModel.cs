using Chatly.Desktop.Shared.ViewModels.Base;

namespace Chatly.Desktop.Features.Users.ViewModels;

public sealed partial class UserInfoPageViewModel(
    UserSessionContext userContext) : ViewModelBase
{
    public UserSessionContext UserContext { get; } = userContext;
}
