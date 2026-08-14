using System.Net;
using Chatly.WebApi.Common.Shared.Exceptions;

namespace Chatly.WebApi.Features.Users.Exceptions;

public sealed class UsernameAlreadyExistsException(string username)
    : ChatlyException(
        "Username already exists",
        $"The username '{username}' is already in use.",
        "User.Username.AlreadyExists",
        HttpStatusCode.Conflict);
