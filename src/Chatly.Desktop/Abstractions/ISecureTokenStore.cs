using System.Threading;
using System.Threading.Tasks;

namespace Chatly.Desktop.Abstractions;

public interface ISecureTokenStore
{
    Task SaveRefreshTokenAsync(
        string refreshToken,
        CancellationToken cancellationToken);

    Task<string?> TryReadRefreshTokenAsync(
        CancellationToken cancellationToken);

    Task DeleteRefreshTokenAsync(
        CancellationToken cancellationToken);
}