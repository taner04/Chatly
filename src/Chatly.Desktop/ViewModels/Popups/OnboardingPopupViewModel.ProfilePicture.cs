using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.Input;

namespace Chatly.Desktop.ViewModels.Popups;

public sealed partial class OnboardingPopupViewModel
{
    [ObservableProperty] public partial IStorageFile? ProfilePicture { get; set; }

    [ObservableProperty] public partial Bitmap? ProfilePicturePreview { get; private set; }

    [ObservableProperty] public partial bool HasProfilePicturePreview { get; private set; }

    [RelayCommand]
    private void RemoveProfilePicture()
    {
        ProfilePicture = null;
        ProfilePicturePreview?.Dispose();
        ProfilePicturePreview = null;
        HasProfilePicturePreview = false;
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
        HasProfilePicturePreview = true;
    }
}