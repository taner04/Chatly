using System.Net;
using System.Net.Http;
using System.Text;
using Chatly.Desktop.Services.Api;
using Refit;

namespace Chatly.Desktop.UnitTests.Tests.Services.Api;

public sealed class ApiRequestExecutorTests
{
    private static readonly RefitSettings Settings = new();

    [Fact]
    public async Task ExecuteAsync_Should_ReturnContent_When_RequestSucceeds()
    {
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            RequestMessage = new HttpRequestMessage(HttpMethod.Get, "https://api.test/users/me")
        };

        var result = await ApiRequestExecutor.ExecuteAsync(
            () => Task.FromResult(new ApiResponse<string>(response, "value", Settings)),
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("value");
    }

    [Fact]
    public async Task ExecuteAsync_Should_MapProblemDetails_When_ApiReturnsError()
    {
        var response = new HttpResponseMessage(HttpStatusCode.Conflict)
        {
            RequestMessage = new HttpRequestMessage(HttpMethod.Put, "https://api.test/users/me/username"),
            Content = new StringContent(
                """{"title":"Taken","detail":"Username is taken.","errorCode":"User.UsernameTaken"}""",
                Encoding.UTF8,
                "application/problem+json")
        };
        var exception = await ApiException.Create(response.RequestMessage, HttpMethod.Put, response, Settings);

        var result = await ApiRequestExecutor.ExecuteAsync(
            () => Task.FromResult(new ApiResponse<string>(response, null, Settings, exception)),
            TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("User.UsernameTaken");
        result.Error.Detail.Should().Be("Username is taken.");
    }

    [Fact]
    public async Task ExecuteAsync_Should_ReturnNetworkError_When_ServerIsUnreachable()
    {
        var result = await ApiRequestExecutor.ExecuteAsync<string>(
            () => Task.FromException<ApiResponse<string>>(new HttpRequestException("connection refused")),
            TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("network.unavailable");
    }

    [Fact]
    public async Task ExecuteAsync_Should_Rethrow_When_CallerCancels()
    {
        using var cancellation = new CancellationTokenSource();
        await cancellation.CancelAsync();

        var act = () => ApiRequestExecutor.ExecuteAsync<string>(
            () => Task.FromException<ApiResponse<string>>(new OperationCanceledException(cancellation.Token)),
            cancellation.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
    }
}
