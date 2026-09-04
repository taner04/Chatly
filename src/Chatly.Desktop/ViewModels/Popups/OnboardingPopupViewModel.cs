using Chatly.Contracts.Endpoints.Users.Requests;
using Chatly.Desktop.Abstraction.Toasts;
using Chatly.Desktop.Extensions;
using Chatly.Desktop.Mappers;
using Chatly.Desktop.Services.Api;
using CommunityToolkit.Mvvm.Input;

namespace Chatly.Desktop.ViewModels.Popups;

[TransientService]
public sealed partial class OnboardingPopupViewModel(
    IToastService toastService,
    UserSessionContext userContext,
    UserWebService userWebService) : ProfilePicturePopupViewModel
{
    public override string Title => "Complete your profile";

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(CompleteOnboardingCommand))]
    public partial string Username { get; set; } = string.Empty;

    [RelayCommand(CanExecute = nameof(CanExecuteCompleteOnboarding))]
    private async Task CompleteOnboarding()
    {
        if (string.IsNullOrWhiteSpace(Username))
        {
            return;
        }

        if (ProfilePicture is null)
        {
            var onboardingResult =
                await userWebService.CompleteOnboardingAsync(new CompleteOnboardingRequest(Username));

            if (onboardingResult.IsFailure)
            {
                toastService.ShowError(onboardingResult.Error);
                return;
            }

            userContext.SetAuthenticated(UserMapper.Map(onboardingResult.Value));
            CloseOverlay();

            return;
        }

        await using var content = await ProfilePicture.OpenReadAsync();
        var contentType = GetContentType(ProfilePicture);

        var result =
            await userWebService.CompleteOnboardingAsync(new CompleteOnboardingRequest(Username, content,
                ProfilePicture.Name, contentType));

        if (result.IsFailure)
        {
            toastService.ShowError(result.Error);
        }
        else
        {
            userContext.SetAuthenticated(UserMapper.Map(result.Value));
            CloseOverlay();
        }
    }

    private bool CanExecuteCompleteOnboarding()
    {
        return !string.IsNullOrWhiteSpace(Username);
    }
}
