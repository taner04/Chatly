using System.IO;
using Refit;

namespace Chatly.Desktop.Services.Api.Multipart;

internal sealed class OwnedStreamParts(
    IReadOnlyList<StreamPart> parts,
    IReadOnlyList<Stream> streams) : IAsyncDisposable
{
    internal IReadOnlyList<StreamPart> Parts { get; } = parts;

    public async ValueTask DisposeAsync()
    {
        foreach (var stream in streams)
        {
            await stream.DisposeAsync();
        }
    }
}