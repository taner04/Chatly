using Chatly.Contracts.Common.Policies;
using Chatly.Contracts.Features.Messages.Models;
using Chatly.Desktop.Abstraction.Storage;
using Chatly.Desktop.Services.Storage;
using Chatly.Desktop.Utilities;

namespace Chatly.Desktop.ViewModels.Pages.ChatPage.Messages;

public sealed partial class MessageAttachmentViewModel
{
    private readonly FileDownloadClient _fileDownloadClient;
    private readonly IFilePicker _filePicker;
    private readonly IToastService _toastService;

    internal MessageAttachmentViewModel(
        MessageAttachmentContract attachment,
        IFilePicker filePicker,
        FileDownloadClient fileDownloadClient,
        IToastService toastService)
    {
        AttachmentId = attachment.AttachmentId;
        FileName = attachment.FileName;
        ContentType = attachment.ContentType;
        Url = attachment.Url;
        IsImage = ImageContentTypePolicy.ImageContentTypes.Contains(attachment.ContentType);
        SizeText = FileSizeFormatter.Format(attachment.Size);
        _filePicker = filePicker;
        _fileDownloadClient = fileDownloadClient;
        _toastService = toastService;
    }

    public Guid AttachmentId { get; }

    public string FileName { get; }

    public string ContentType { get; }

    public string Url { get; }

    public bool IsImage { get; }

    public bool IsFile => !IsImage;

    public string SizeText { get; }

    [RelayCommand]
    private async Task DownloadAsync(CancellationToken cancellationToken)
    {
        var destination = await _filePicker.PickSaveFileAsync(FileName);
        if (destination is null)
        {
            return;
        }

        try
        {
            await using var output = await destination.OpenWriteAsync();
            output.SetLength(0);
            await _fileDownloadClient.DownloadAsync(Url, output, cancellationToken);
            _toastService.ShowSuccess($"Downloaded {FileName}.");
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
        }
        catch (Exception)
        {
            _toastService.AddNotification($"Could not download {FileName}.");
        }
    }
}
