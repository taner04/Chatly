using System.Diagnostics;
using Chatly.WebApi.Common.Shared.Exceptions;
using FluentValidation;

namespace Chatly.WebApi.Common.Composition.Configs;

public static class ProblemDetailsConfig
{
    public static void Config(
        ProblemDetailsOptions options)
    {
        options.CustomizeProblemDetails = ctx =>
        {
            var httpContext = ctx.HttpContext;

            var problemDetails = ctx.Exception switch
            {
                ValidationException validation => new ApiProblemDetails
                {
                    Status = StatusCodes.Status400BadRequest,
                    Title = "Validation failed",
                    ErrorCode = "Validation.Failed",
                    Errors = validation.Errors
                        .GroupBy(e => e.PropertyName, StringComparer.OrdinalIgnoreCase)
                        .ToDictionary(
                            g => g.Key,
                            g => g.Select(e => e.ErrorMessage).ToArray(),
                            StringComparer.OrdinalIgnoreCase
                        )
                },

                ChatlyException modelsException => new ApiProblemDetails
                {
                    Status = (int)modelsException.StatusCode,
                    Title = modelsException.Title,
                    Detail = modelsException.Message,
                    ErrorCode = modelsException.ErrorCode
                },

                UnauthorizedAccessException => new ApiProblemDetails
                {
                    Status = StatusCodes.Status401Unauthorized,
                    Title = "Unauthorized",
                    Detail = "You are not authorized to access this resource.",
                    ErrorCode = "Authorization.Unauthorized"
                },

                _ => new ApiProblemDetails
                {
                    Status = StatusCodes.Status500InternalServerError,
                    Title = "Internal Server Error",
                    Detail = "An unexpected error has occurred.",
                    ErrorCode = "Server.UnexpectedError"
                }
            };

            httpContext.Response.StatusCode = problemDetails.Status ?? StatusCodes.Status500InternalServerError;

            problemDetails.Instance = httpContext.Request.Path;
            problemDetails.Type = $"https://chatly.com/{GetRoutePattern(httpContext)}";

            problemDetails.Extensions["method"] = httpContext.Request.Method;
            problemDetails.Extensions["traceId"] = Activity.Current?.Id ?? httpContext.TraceIdentifier;

            if (problemDetails.Errors.Count > 0)
            {
                problemDetails.Extensions["errors"] = problemDetails.Errors;
            }

            ctx.ProblemDetails = problemDetails;
        };
    }

    private static string GetRoutePattern(
        HttpContext http)
    {
        var endpoint = http.GetEndpoint();
        return endpoint?
                   .Metadata
                   .GetMetadata<RouteEndpoint>()?
                   .RoutePattern.RawText
               ?? http.Request.Path;
    }
}
