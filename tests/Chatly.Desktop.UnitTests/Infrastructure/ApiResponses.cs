using System.Net;
using System.Net.Http.Json;
using Chatly.Contracts.Common;
using Refit;

namespace Chatly.Desktop.UnitTests.Infrastructure;

internal static class ApiResponses
{
    private static readonly RefitSettings Settings = new();

    internal static ApiResponse<T> Ok<T>(T? content) => new(CreateResponse(HttpStatusCode.OK), content, Settings);

    internal static async Task<ApiResponse<T>> ProblemAsync<T>(HttpStatusCode statusCode, ApiProblemDetails problem)
    {
        var response = Problem(statusCode, problem);
        var request = response.RequestMessage!;
        var exception = await ApiException.Create(request, request.Method, response, Settings);
        return new ApiResponse<T>(response, default, Settings, exception);
    }

    internal static HttpResponseMessage Problem(HttpStatusCode statusCode, ApiProblemDetails problem)
    {
        var response = CreateResponse(statusCode);
        response.Content = JsonContent.Create(problem);
        return response;
    }

    private static HttpResponseMessage CreateResponse(HttpStatusCode statusCode) =>
        new(statusCode)
        {
            RequestMessage = new HttpRequestMessage(HttpMethod.Get, "https://api.test/")
        };
}