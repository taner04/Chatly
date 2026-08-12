using System;
using System.Threading;
using System.Threading.Tasks;
using Chatly.Desktop.Features.Authentication.Abstractions;

namespace Chatly.Desktop.Infrastructure.Authentication.Storage;

internal sealed class MacOsSecureTokenStore : ISecureTokenStore
{
    private const string Service = "com.chatly.desktop";
    private const string Account = "refresh-token";

    public Task SaveRefreshTokenAsync(
        string refreshToken,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(refreshToken);
        cancellationToken.ThrowIfCancellationRequested();


        return Task.CompletedTask;
    }

    public Task<string?> TryReadRefreshTokenAsync(
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        // SecItemCopyMatching(...)
        // errSecItemNotFound => null
        // errSecSuccess => decode returned data as UTF-8

        return Task.FromResult<string?>(null);
    }

    public Task DeleteRefreshTokenAsync(
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        // SecItemDelete(...)
        // Treat errSecItemNotFound as success

        return Task.CompletedTask;
    }
}