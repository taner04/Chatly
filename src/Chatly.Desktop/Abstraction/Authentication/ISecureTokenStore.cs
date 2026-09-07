namespace Chatly.Desktop.Abstraction.Authentication;

public interface ISecureTokenStore
{
    Task SaveRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken);
    Task<string?> TryReadRefreshTokenAsync(CancellationToken cancellationToken);
    Task DeleteRefreshTokenAsync(CancellationToken cancellationToken);
}