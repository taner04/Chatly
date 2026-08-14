using System.Net;
using Chatly.WebApi.Common.Shared.Exceptions;

namespace Chatly.WebApi.Common.Shared.Guards.Exceptions;

public sealed class GuardException : ChatlyException
{
    private GuardException(string title, string message, string errorCode)
        : base(title, message, errorCode, HttpStatusCode.BadRequest)
    {
    }

    public static void Throw(string ownerName, string propertyName, string rule, string message)
    {
        throw new GuardException(
            $"Invalid {propertyName}",
            message,
            $"{ownerName}.{propertyName}.{rule}");
    }
}
