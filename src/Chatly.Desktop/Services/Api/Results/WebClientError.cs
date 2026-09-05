using Chatly.Contracts.Results;

namespace Chatly.Desktop.Services.Api.Results;

public readonly record struct WebClientError
{
    private WebClientError(
        string errorCode,
        string detail)
    {
        ErrorCode = errorCode;
        Detail = detail;
    }

    public string ErrorCode { get; }
    public string Detail { get; }

    internal static WebClientError FromProblemDetails(ApiProblemDetails problemDetails)
    {
        return new WebClientError(
            problemDetails.ErrorCode ?? "api.error",
            problemDetails.Detail ?? "The API request was not successful.");
    }

    internal static WebClientError CustomError(
        string errorCode,
        string detail)
    {
        return new WebClientError(errorCode, detail);
    }
}