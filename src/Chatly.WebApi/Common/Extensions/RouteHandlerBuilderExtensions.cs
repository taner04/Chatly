namespace Chatly.WebApi.Common.Extensions;

internal static class RouteHandlerBuilderExtensions
{
    private static readonly int[] DefaultProblemCodes =
    [
        StatusCodes.Status500InternalServerError,
        StatusCodes.Status401Unauthorized,
        StatusCodes.Status403Forbidden
    ];

    extension(RouteHandlerBuilder builder)
    {
        public RouteHandlerBuilder ProducesStandardErrors(params int[] additionalStatusCodes)
        {
            var allCodes = DefaultProblemCodes
                .Concat(additionalStatusCodes)
                .Distinct();

            foreach (var code in allCodes)
            {
                builder.Produces<ApiProblemDetails>(code);
            }

            return builder;
        }
    }
}
