using System.IO;
using Avalonia.Platform.Storage;
using Refit;

namespace Chatly.Desktop.Services.Api.Multipart;

internal static class MultipartStreamPartFactory
{
    internal static StreamPart? CreateBorrowed(
        Stream? stream,
        string? fileName,
        string? contentType,
        string parameterName)
    {
        if (stream is null)
        {
            return null;
        }

        return new StreamPart(
            stream,
            fileName ?? throw new ArgumentException(
                "A file name is required when file content is provided.",
                parameterName),
            contentType);
    }

    internal static async Task<OwnedStreamParts> OpenOwnedAsync(
        IReadOnlyCollection<IStorageFile> files,
        Func<string, string?>? getContentType = null)
    {
        var streams = new List<Stream>(files.Count);
        var parts = new List<StreamPart>(files.Count);

        try
        {
            foreach (var file in files)
            {
                var stream = await file.OpenReadAsync();
                streams.Add(stream);
                parts.Add(new StreamPart(stream, file.Name, getContentType?.Invoke(file.Name)));
            }

            return new OwnedStreamParts(parts, streams);
        }
        catch
        {
            foreach (var stream in streams)
            {
                await stream.DisposeAsync();
            }

            throw;
        }
    }
}