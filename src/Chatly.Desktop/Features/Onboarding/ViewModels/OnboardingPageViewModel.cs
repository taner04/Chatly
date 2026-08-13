using Avalonia.Controls;
using Avalonia.Platform.Storage;
using Avalonia.Media.Imaging;
using Chatly.Contracts.Users.Requests;
using Chatly.Desktop.Features.Users.Api;
using Chatly.Desktop.Shared.ViewModels.Base;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Chatly.Desktop.Features.Onboarding.ViewModels;

public sealed partial class OnboardingPageViewModel(UserWebService userWebService) : ViewModelBase
{
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(CompleteOnboardingCommand))]
    public partial string Username { get; set; } = string.Empty;

    [ObservableProperty]
    public partial IStorageFile? ProfilePicture { get; set; }

    [ObservableProperty]
    public partial Bitmap? ProfilePicturePreview { get; private set; }

    [ObservableProperty]
    public partial bool HasProfilePicturePreview { get; private set; }

    [RelayCommand(CanExecute = nameof(CanExcecuteCompleteOnboarding))]
    private async Task CompleteOnboarding()
    {
        if (!string.IsNullOrWhiteSpace(Username))
        {
            if (ProfilePicture is null)
            {
                await userWebService.CompleteOnboardingAsync(new CompleteOnboardingRequest(Username));
                return;
            }

            await using var content = await ProfilePicture.OpenReadAsync();
            var contentType = Path.GetExtension(ProfilePicture.Name).ToLowerInvariant() switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".webp" => "image/webp",
                _ => "application/octet-stream"
            };

            await userWebService.CompleteOnboardingAsync(new CompleteOnboardingRequest(Username, content, ProfilePicture.Name, contentType));
        }
    }

    private bool CanExcecuteCompleteOnboarding() => !string.IsNullOrWhiteSpace(Username);

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

        var file = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Select Profile Picture",
            AllowMultiple = false,
            FileTypeFilter =
            [
                new("Image Files")
                {
                    Patterns = ["*.jpg", "*.jpeg", "*.png", "*.webp"]
                }
            ]
        });

        if (file.Count == 1)
        {
            ProfilePicture = file[0];

            await using var stream = await ProfilePicture.OpenReadAsync();
            ProfilePicturePreview?.Dispose();
            ProfilePicturePreview = new Bitmap(stream);
            HasProfilePicturePreview = true;
        }
    }
}
