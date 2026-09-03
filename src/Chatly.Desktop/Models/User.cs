namespace Chatly.Desktop.Models;

public sealed partial class User : ObservableObject
{
    public Guid Id { get; init; }

    public string Email { get; init; } = string.Empty;

    public string? Username { get; init; }

    public string? ProfilePictureUrl { get; init; }

    public bool OnboardingCompleted { get; init; }

    [ObservableProperty] public partial bool IsOnline { get; set; }
}