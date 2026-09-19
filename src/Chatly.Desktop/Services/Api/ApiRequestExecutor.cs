using System.Net.Http;
using Chatly.Desktop.Services.Api.Results;
using Refit;

namespace Chatly.Desktop.Services.Api;

internal static class ApiRequestExecutor
{
    internal static async Task<WebClientResult> ExecuteAsync(
        Func<Task<IApiResponse>> apiCall,
        CancellationToken cancellationToken)
    {
        return await ExecuteCoreAsync(
            apiCall,
            response => response.IsSuccessful ? WebClientResult.Success() : null,
            error => error,
            cancellationToken);
    }

    internal static async Task<WebClientResult<T>> ExecuteAsync<T>(
        Func<Task<ApiResponse<T>>> apiCall,
        CancellationToken cancellationToken)
    {
        return await ExecuteCoreAsync(
            async () => await apiCall(),
            ProjectContent<T>,
            error => error,
            cancellationToken);
    }

    private static async Task<TResult> ExecuteCoreAsync<TResult>(
        Func<Task<IApiResponse>> apiCall,
        Func<IApiResponse, TResult?> projectSuccess,
        Func<WebClientError, TResult> projectError,
        CancellationToken cancellationToken)
        where TResult : WebClientResult
    {
        try
        {
            using var response = await apiCall();

            if (projectSuccess(response) is { } result)
            {
                return result;
            }

            return projectError(await CreateErrorAsync(response.Error));
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (HttpRequestException exception)
        {
            return projectError(WebClientError.CustomError(
                "network.unavailable", exception.Message));
        }
        catch (Exception exception)
        {
            return projectError(WebClientError.CustomError(
                "client.unexpected", exception.Message));
        }
    }

    private static WebClientResult<T>? ProjectContent<T>(IApiResponse response) =>
        response is ApiResponse<T> { IsSuccessful: true, Content: not null } apiResponse
            ? apiResponse.Content
            : null;

    private static async Task<WebClientError> CreateErrorAsync(ApiExceptionBase? exception)
    {
        if (exception is null)
        {
            return WebClientError.CustomError(
                "response.invalid",
                "The API returned neither content nor error details.");
        }

        try
        {
            var problemDetails = exception is ApiException apiException
                ? await apiException.GetContentAsAsync<ApiProblemDetails>()
                : null;

            if (problemDetails is not null)
            {
                return WebClientError.FromProblemDetails(problemDetails);
            }
        }
        catch
        {
            // Ignore any exceptions while trying to read the problem details
        }

        return WebClientError.CustomError(
            "api.error", exception.Message);
    }
}