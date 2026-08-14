using System;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using Chatly.Contracts.Users.Requests;
using Chatly.Desktop.Models;
using Chatly.Desktop.Services.Api;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Chatly.Desktop.ViewModels.Overlays;

public sealed partial class OnboardingOverlayViewModel(
    UserSessionContext userContext,
    UserWebService userWebService) : PopupOverlayViewModel
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

    [RelayCommand(CanExecute = nameof(CanExecuteCompleteOnboarding))]
    private async Task CompleteOnboarding()
    {
        if (string.IsNullOrWhiteSpace(Username))
        {
            return;
        }

        if (ProfilePicture is null)
        {
            var onboardingResult = await userWebService.CompleteOnboardingAsync(
                new CompleteOnboardingRequest(Username));

            if (onboardingResult.IsSuccess)
            {
                userContext.SetAuthenticated(onboardingResult.Value);
                CloseOverlay();
            }

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

        var result = await userWebService.CompleteOnboardingAsync(
            new CompleteOnboardingRequest(Username, content, ProfilePicture.Name, contentType));

        if (result.IsSuccess)
        {
            userContext.SetAuthenticated(result.Value);
            CloseOverlay();
        }
    }

    private bool CanExecuteCompleteOnboarding() => !string.IsNullOrWhiteSpace(Username);

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
                new("Image Files")
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
