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
        public RouteHandlerBuilder ProducesStandardErrors() => AddStandardErrors(builder, []);

        public RouteHandlerBuilder ProducesStandardErrors(int statusCode) =>
            AddStandardErrors(builder, [statusCode]);

        public RouteHandlerBuilder ProducesStandardErrors(int firstStatusCode, int secondStatusCode) =>
            AddStandardErrors(builder, [firstStatusCode, secondStatusCode]);

        public RouteHandlerBuilder ProducesStandardErrors(
            int firstStatusCode,
            int secondStatusCode,
            int thirdStatusCode) =>
            AddStandardErrors(builder, [firstStatusCode, secondStatusCode, thirdStatusCode]);
    }

    private static RouteHandlerBuilder AddStandardErrors(
        RouteHandlerBuilder builder,
        ReadOnlySpan<int> additionalStatusCodes)
    {
        var allCodes = DefaultProblemCodes
            .Concat(additionalStatusCodes.ToArray())
            .Distinct();

        foreach (var code in allCodes)
        {
            builder.Produces<ApiProblemDetails>(code);
        }

        return builder;
    }
}