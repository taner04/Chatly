using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Chatly.Desktop.Abstractions.Authentication;

namespace Chatly.Desktop.Services.Authentication.Storage;

internal sealed class WindowsSecureTokenStore : ISecureTokenStore
{
    private static readonly string DirectoryPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "Chatly");

    private static readonly string TokenPath = Path.Combine(
        DirectoryPath,
        "refresh-token.dat");

    public async Task SaveRefreshTokenAsync(
        string refreshToken,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(refreshToken);
        cancellationToken.ThrowIfCancellationRequested();

        Directory.CreateDirectory(DirectoryPath);

        var plaintext = Encoding.UTF8.GetBytes(refreshToken);

        try
        {
#pragma warning disable CA1416
            var encrypted = ProtectedData.Protect(
                plaintext,
                null,
                DataProtectionScope.CurrentUser);
#pragma warning restore CA1416

            await File.WriteAllBytesAsync(
                TokenPath,
                encrypted,
                cancellationToken);
        }
        finally
        {
            CryptographicOperations.ZeroMemory(plaintext);
        }
    }

    public Task DeleteRefreshTokenAsync(
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (File.Exists(TokenPath))
        {
            File.Delete(TokenPath);
        }

        return Task.CompletedTask;
    }

    public async Task<string?> TryReadRefreshTokenAsync(
        CancellationToken cancellationToken)
    {
        if (!File.Exists(TokenPath))
        {
            return null;
        }

        try
        {
            var encrypted = await File.ReadAllBytesAsync(
                TokenPath,
                cancellationToken);

            cancellationToken.ThrowIfCancellationRequested();

#pragma warning disable CA1416
            var plaintext = ProtectedData.Unprotect(
                encrypted,
                null,
                DataProtectionScope.CurrentUser);
#pragma warning restore CA1416

            try
            {
                var token = Encoding.UTF8.GetString(plaintext);
                return string.IsNullOrWhiteSpace(token) ? null : token;
            }
            finally
            {
                CryptographicOperations.ZeroMemory(plaintext);
            }
        }
        catch (CryptographicException)
        {
            return null;
        }
        catch (IOException)
        {
            return null;
        }
    }
}