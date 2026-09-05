using System.Net.Http;
using Chatly.Contracts.Results;
using Chatly.Desktop.Services.Api.Results;
using Refit;

namespace Chatly.Desktop.Services.Api;

internal static class ApiRequestExecutor
{
    internal static async Task<WebClientResult> ExecuteAsync(
        Func<Task<IApiResponse>> apiCall,
        CancellationToken cancellationToken)
    {
        try
        {
            using var response = await apiCall();

            if (response.IsSuccessful)
            {
                return WebClientResult.Success();
            }

            return await CreateErrorAsync(response.Error);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (HttpRequestException exception)
        {
            return WebClientError.CustomError(
                "network.unavailable", exception.Message);
        }
        catch (Exception exception)
        {
            return WebClientError.CustomError(
                "client.unexpected", exception.Message);
        }
    }

    internal static async Task<WebClientResult<T>> ExecuteAsync<T>(
        Func<Task<ApiResponse<T>>> apiCall,
        CancellationToken cancellationToken)
    {
        try
        {
            using var response = await apiCall();

            if (response is { IsSuccessful: true, Content: not null })
            {
                return response.Content;
            }

            return await CreateErrorAsync(response.Error);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (HttpRequestException exception)
        {
            return WebClientError.CustomError(
                "network.unavailable", exception.Message);
        }
        catch (Exception exception)
        {
            return WebClientError.CustomError(
                "client.unexpected", exception.Message);
        }
    }

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