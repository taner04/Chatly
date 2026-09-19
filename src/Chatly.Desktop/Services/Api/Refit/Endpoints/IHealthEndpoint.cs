using Refit;

namespace Chatly.Desktop.Services.Api.Refit.Endpoints;

public interface IHealthEndpoint
{
    [Get(ApiRoutes.Health.Status)]
    Task<IApiResponse> CheckStatusAsync(CancellationToken cancellationToken);
}