using System.Net;
using Chatly.Contracts.Common;
using Chatly.Desktop.Services.Api;
using Chatly.Desktop.UnitTests.Infrastructure;
using Refit;

namespace Chatly.Desktop.UnitTests.Tests.Services.Api;

public sealed class ApiRequestExecutorTests : TestBase
{
    [Fact]
    public async Task ExecuteAsync_Should_ReturnContent_When_RequestSucceeds()
    {
        var result = await ApiRequestExecutor.ExecuteAsync(
            () => Task.FromResult(ApiResponses.Ok("value")),
            CurrentCancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("value");
    }

    [Fact]
    public async Task ExecuteAsync_Should_MapProblemDetails_When_ApiReturnsError()
    {
        var response = await ApiResponses.ProblemAsync<string>(
            HttpStatusCode.Conflict,
            new ApiProblemDetails { Title = "Taken", Detail = "Username is taken.", ErrorCode = "User.UsernameTaken" });

        var result = await ApiRequestExecutor.ExecuteAsync(
            () => Task.FromResult(response),
            CurrentCancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("User.UsernameTaken");
        result.Error.Detail.Should().Be("Username is taken.");
    }

    [Fact]
    public async Task ExecuteAsync_Should_ReturnNetworkError_When_ServerIsUnreachable()
    {
        var result = await ApiRequestExecutor.ExecuteAsync(
            () => Task.FromException<ApiResponse<string>>(new HttpRequestException("connection refused")),
            CurrentCancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("network.unavailable");
    }

    [Fact]
    public async Task ExecuteAsync_Should_Rethrow_When_CallerCancels()
    {
        using var cancellation = new CancellationTokenSource();
        await cancellation.CancelAsync();
        var token = cancellation.Token;

        var act = () => ApiRequestExecutor.ExecuteAsync(
            () => Task.FromException<ApiResponse<string>>(new OperationCanceledException(token)),
            token);

        await act.Should().ThrowAsync<OperationCanceledException>();
    }
}