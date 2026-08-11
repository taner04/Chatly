using CommunityToolkit.Mvvm.ComponentModel;

namespace Chatly.Desktop.Infrastructure.Auth;

public sealed partial class UserContext : ObservableObject
{ 
    public bool IsAuthenticated => !string.IsNullOrEmpty(AccessToken);
    public string AccessToken { get; set; } = null!;
}
