namespace Chatly.Desktop.Models;

public sealed partial class User : ObservableObject
{
    public Guid Id { get; init; }

    public string Email { get; set; } = string.Empty;

    [ObservableProperty] public partial string? Username { get; set; }

    [ObservableProperty] public partial string? ProfilePictureUrl { get; set; }

    public bool OnboardingCompleted { get; set; }

    [ObservableProperty] public partial bool IsOnline { get; set; }
}