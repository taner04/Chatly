using System.Net;

namespace Chatly.WebApi.Common.Shared.Exceptions;

internal abstract class ChatlyException(string title, string message, string errorCode, HttpStatusCode statusCode)
    : Exception(message)
{
    public string Title { get; init; } = title;
    public string ErrorCode { get; init; } = errorCode;
    public HttpStatusCode StatusCode { get; init; } = statusCode;
}
