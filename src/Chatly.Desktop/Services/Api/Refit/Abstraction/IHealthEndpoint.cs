using Refit;

namespace Chatly.Desktop.Services.Api.Refit.Abstraction;

public interface IHealthEndpoint
{
    [Get("/api/health/status")]
    Task<IApiResponse> CheckStatusAsync(CancellationToken cancellationToken);
}