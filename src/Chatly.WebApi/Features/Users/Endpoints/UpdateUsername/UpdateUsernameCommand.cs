using Chatly.Contracts.Users.Results;
using Mediator;

namespace Chatly.WebApi.Features.Users.Endpoints.UpdateUsername;

public record UpdateUsernameCommand(string NewUsername) : ICommand<CurrentUserResponse>;