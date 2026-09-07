namespace Chatly.Desktop.Models;

public sealed partial class User : ObservableObject
{
    public Guid Id { get; init; }

    public string Email { get; init; } = string.Empty;

    [ObservableProperty] public partial string? Username { get; set; }

    [ObservableProperty] public partial string? ProfilePictureUrl { get; set; }

    public bool OnboardingCompleted { get; init; }

    [ObservableProperty] public partial bool IsOnline { get; set; }
}