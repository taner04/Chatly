namespace Chatly.Desktop.Services.Api.Results;

internal readonly record struct WebClientError
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

    internal static WebClientError FromProblemDetails(ApiProblemDetails problemDetails) =>
        new(
            problemDetails.ErrorCode ?? "api.error",
            problemDetails.Detail ?? "The API request was not successful.");

    internal static WebClientError CustomError(string errorCode, string detail) => new(errorCode, detail);
}