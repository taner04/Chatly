using System.Net;

namespace Chatly.WebApi.Features.Users.Exceptions;

internal sealed class UsernameAlreadyExistsException(string username)
    : ChatlyException(
        "Username already exists",
        $"The username '{username}' is already in use.",
        "User.Username.AlreadyExists",
        HttpStatusCode.Conflict);
