using System.IO;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.Input;

namespace Chatly.Desktop.ViewModels.Popups;

public abstract partial class ProfilePicturePopupViewModel : PopupOverlayViewModel
{
    private string? _existingProfilePictureUrl;

    protected ProfilePicturePopupViewModel(string? existingProfilePictureUrl = null)
    {
        _existingProfilePictureUrl = existingProfilePictureUrl;
    }

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
    private async Task SelectProfilePicture(UserControl userControl)
    {
        var topLevel = TopLevel.GetTopLevel(userControl);
        ArgumentNullException.ThrowIfNull(topLevel);

        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Select Profile Picture",
            AllowMultiple = false,
            FileTypeFilter =
            [
                new FilePickerFileType("Image Files")
                {
                    Patterns = ["*.jpg", "*.jpeg", "*.png", "*.webp"]
                }
            ]
        });

        if (files.Count != 1)
        {
            return;
        }

        ProfilePicture = files[0];
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
        return Path.GetExtension(file.Name).ToLowerInvariant() switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".webp" => "image/webp",
            _ => "application/octet-stream"
        };
    }
}