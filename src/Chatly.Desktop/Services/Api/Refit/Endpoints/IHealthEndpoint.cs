using Refit;

namespace Chatly.Desktop.Services.Api.Refit.Abstraction;

public interface IHealthEndpoint
{
    [Get(ApiRoutes.Health.Status)]
    Task<IApiResponse> CheckStatusAsync(CancellationToken cancellationToken);
}