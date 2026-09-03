using System.IO;
using System.Security.Cryptography;
using System.Text;
using Chatly.Desktop.Abstraction.Authentication;
using Chatly.Desktop.Options;
using Microsoft.Extensions.Options;

namespace Chatly.Desktop.Services.Authentication.Storage;

internal sealed class WindowsSecureTokenStore : ISecureTokenStore
{
    private readonly string _directoryPath;
    private readonly string _tokenPath;

    public WindowsSecureTokenStore(IOptions<DesktopProfileOption> profileOptions)
    {
        var profileName = profileOptions.Value.Name;
        var baseDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Chatly");
        _directoryPath = profileName == DesktopProfileOption.DefaultName
            ? baseDirectory
            : Path.Combine(baseDirectory, profileName);
        _tokenPath = Path.Combine(_directoryPath, "refresh-token.dat");
    }

    public async Task SaveRefreshTokenAsync(
        string refreshToken,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(refreshToken);
        cancellationToken.ThrowIfCancellationRequested();

        Directory.CreateDirectory(_directoryPath);

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
                _tokenPath,
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

        if (File.Exists(_tokenPath))
        {
            File.Delete(_tokenPath);
        }

        return Task.CompletedTask;
    }

    public async Task<string?> TryReadRefreshTokenAsync(
        CancellationToken cancellationToken)
    {
        if (!File.Exists(_tokenPath))
        {
            return null;
        }

        try
        {
            var encrypted = await File.ReadAllBytesAsync(
                _tokenPath,
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