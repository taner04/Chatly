namespace Chatly.Contracts.Users.Requests;

public sealed record CompleteOnboardingRequest(
    string NewUsername,
    Stream? Content = null,
    string? FileName = null,
    string? ContentType = null);