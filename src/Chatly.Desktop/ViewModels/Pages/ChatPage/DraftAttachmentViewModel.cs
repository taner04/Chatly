using System.IO;
using Avalonia.Platform.Storage;
using Chatly.Contracts.Common.Policies;
using Chatly.Desktop.Utilities;

namespace Chatly.Desktop.ViewModels.Pages.ChatPage;

public sealed class DraftAttachmentViewModel
{
    internal DraftAttachmentViewModel(IStorageFile file, long size)
    {
        File = file;
        FileName = file.Name;
        Size = size;
        SizeText = FileSizeFormatter.Format(size);
        IsImage = IsImageFile(file.Name);
        PreviewUrl = IsImage ? file.Path.AbsoluteUri : null;
    }

    internal IStorageFile File { get; }

    public string FileName { get; }

    public long Size { get; }

    public string SizeText { get; }

    public bool IsImage { get; }

    public bool IsFile => !IsImage;

    public string? PreviewUrl { get; }

    private static bool IsImageFile(string fileName)
    {
        var contentType = ImageContentTypePolicy.FromFileExtension(Path.GetExtension(fileName));
        return contentType is not null && ImageContentTypePolicy.ImageContentTypes.Contains(contentType);
    }
}