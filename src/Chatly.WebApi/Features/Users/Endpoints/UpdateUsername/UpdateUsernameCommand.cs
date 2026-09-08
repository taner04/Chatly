namespace Chatly.WebApi.Features.Users.Endpoints.UpdateUsername;

internal sealed record UpdateUsernameCommand(string NewUsername) : ICommand<CurrentUserResponse>;
