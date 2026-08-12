using System.Collections.Generic;
using Chatly.Contracts.Results;

namespace Chatly.Desktop.Common.Api.Results;

public readonly record struct WebClientError
{
    private WebClientError(
        string errorCode,
        string title,
        string detail,
        Dictionary<string, string[]>? errors)
    {
        ErrorCode = errorCode;
        Title = title;
        Detail = detail;
        Errors = errors ?? [];
    }

    public string ErrorCode { get; }
    public string Title { get; }
    public string Detail { get; }
    public Dictionary<string, string[]> Errors { get; }

    internal static WebClientError FromProblemDetails(ApiProblemDetails problemDetails) =>
        new(
            problemDetails.ErrorCode ?? "api.error",
            problemDetails.Title ?? "API request failed",
            problemDetails.Detail ?? "The API request was not successful.",
            problemDetails.Errors);

    internal static WebClientError CustomError(
        string errorCode,
        string title,
        string detail) =>
        new(errorCode, title, detail, null);
}