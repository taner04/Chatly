using System.IO;
using Refit;

namespace Chatly.Desktop.Services.Api.Multipart;

internal sealed class OwnedStreamParts(
    IReadOnlyList<StreamPart> parts,
    IReadOnlyList<Stream> streams) : IAsyncDisposable
{
    internal IReadOnlyList<StreamPart> Parts { get; } = parts;

    internal StreamPart Single => Parts.Count == 1
        ? Parts[0]
        : throw new InvalidOperationException("Exactly one multipart stream was expected.");

    public async ValueTask DisposeAsync()
    {
        foreach (var stream in streams)
        {
            await stream.DisposeAsync();
        }
    }
}