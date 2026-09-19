using System.IO;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using Chatly.Contracts.Common.Policies;
using Chatly.Desktop.Abstraction.Storage;

namespace Chatly.Desktop.ViewModels.Popups;

public abstract partial class ProfilePicturePopupViewModel(
    IFilePicker filePicker,
    string? existingProfilePictureUrl = null)
    : PopupOverlayViewModel
{
    private string? _existingProfilePictureUrl = existingProfilePictureUrl;

    [ObservableProperty] public partial IStorageFile? ProfilePicture { get; private set; }

    [ObservableProperty] public partial Bitmap? ProfilePicturePreview { get; private set; }

    [ObservableProperty] public partial bool IsProfilePictureRemoved { get; private set; }

    public bool HasProfilePicturePreview => ProfilePicturePreview is not null;

    public bool HasProfilePicture => HasProfilePicturePreview ||
                                     (!IsProfilePictureRemoved &&
                                      !string.IsNullOrWhiteSpace(_existingProfilePictureUrl));

    public string? VisibleProfilePictureUrl => IsProfilePictureRemoved
        ? null
        : _existingProfilePictureUrl;

    public override void CloseOverlay()
    {
        ProfilePicturePreview?.Dispose();
        ProfilePicturePreview = null;
        base.CloseOverlay();
    }

    [RelayCommand]
    private void RemoveProfilePicture()
    {
        ProfilePicture = null;
        ProfilePicturePreview?.Dispose();
        ProfilePicturePreview = null;
        IsProfilePictureRemoved = true;
    }

    [RelayCommand]
    private async Task SelectProfilePicture()
    {
        var file = await filePicker.PickProfilePictureAsync();
        if (file is null)
        {
            return;
        }

        ProfilePicture = file;
        await using var stream = await ProfilePicture.OpenReadAsync();

        ProfilePicturePreview?.Dispose();
        ProfilePicturePreview = new Bitmap(stream);
        IsProfilePictureRemoved = false;
    }

    partial void OnProfilePicturePreviewChanged(Bitmap? value)
    {
        OnPropertyChanged(nameof(HasProfilePicturePreview));
        OnPropertyChanged(nameof(HasProfilePicture));
    }

    partial void OnProfilePictureChanged(IStorageFile? value)
    {
        OnProfilePictureSelectionChanged();
    }

    partial void OnIsProfilePictureRemovedChanged(bool value)
    {
        OnPropertyChanged(nameof(VisibleProfilePictureUrl));
        OnPropertyChanged(nameof(HasProfilePicture));
        OnProfilePictureSelectionChanged();
    }

    protected virtual void OnProfilePictureSelectionChanged()
    {
    }

    protected void SetExistingProfilePictureUrl(string? profilePictureUrl)
    {
        if (_existingProfilePictureUrl == profilePictureUrl)
        {
            return;
        }

        _existingProfilePictureUrl = profilePictureUrl;
        OnPropertyChanged(nameof(VisibleProfilePictureUrl));
        OnPropertyChanged(nameof(HasProfilePicture));
    }

    protected static string GetContentType(IStorageFile file)
    {
        var contentType = ImageContentTypePolicy.FromFileExtension(Path.GetExtension(file.Name));
        return contentType is not null && ImageContentTypePolicy.ProfilePictureContentTypes.Contains(contentType)
            ? contentType
            : "application/octet-stream";
    }
}