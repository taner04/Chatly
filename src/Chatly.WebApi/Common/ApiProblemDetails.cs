namespace Chatly.WebApi.Common;

/// <summary>
///     Represents detailed information about an API problem, compatible with RFC 7807.
/// </summary>
/// <remarks>
///     Inherits from <see cref="ProblemDetails" /> and adds application-specific fields
///     such as an error code, a collection of validation errors, and a trace identifier.
///     This type is suitable for returning structured error responses from HTTP APIs.
/// </remarks>
internal sealed class ApiProblemDetails : ProblemDetails
{
    /// <summary>
    ///     An optional application-specific error code that can be used by clients to
    ///     programmatically identify the type of error.
    /// </summary>
    public string? ErrorCode { get; init; }

    /// <summary>
    ///     Optional collection of validation or model binding errors.
    ///     The dictionary maps field names (or general keys) to an array of error messages.
    /// </summary>
    public Dictionary<string, string[]> Errors { get; init; } = [];
}
