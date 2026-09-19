using System.Text.RegularExpressions;
using Chatly.Contracts.Common.Policies;
using Chatly.Contracts.Features.Users.Endpoints.CompleteOnboarding;
using Chatly.Desktop.Abstraction.Storage;
using Chatly.Desktop.Mappers;
using Chatly.Desktop.Services.Api.Clients;
using UserSessionContext = Chatly.Desktop.Models.UserSession.UserSessionContext;

namespace Chatly.Desktop.ViewModels.Popups;

[TransientService]
public sealed partial class OnboardingPopupViewModel(
    IToastService toastService,
    UserSessionContext userContext,
    UserApiClient userApiClient,
    IFilePicker filePicker) : ProfilePicturePopupViewModel(filePicker)
{
    public override string Title => "Complete your profile";

    public int UsernameMaxLength => UsernamePolicy.MaxLength;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(CompleteOnboardingCommand))]
    public partial string Username { get; set; } = string.Empty;

    public override void CloseOverlay()
    {
        CompleteOnboardingCommand.Cancel();
        base.CloseOverlay();
    }

    [RelayCommand(CanExecute = nameof(CanExecuteCompleteOnboarding))]
    private async Task CompleteOnboarding(CancellationToken cancellationToken)
    {
        if (!IsUsernameValid())
        {
            return;
        }

        var profilePicture = ProfilePicture;
        await using var content = profilePicture is null
            ? null
            : await profilePicture.OpenReadAsync();
        var request = new CompleteOnboardingRequest(
            Username,
            content,
            profilePicture?.Name,
            profilePicture is null ? null : GetContentType(profilePicture));
        var result = await userApiClient.CompleteOnboardingAsync(request, cancellationToken);

        if (result.IsFailure)
        {
            toastService.ShowError(result.Error);
            return;
        }

        userContext.SetAuthenticated(UserMapper.Map(result.Value));
        CloseOverlay();
    }

    private bool CanExecuteCompleteOnboarding() => IsUsernameValid();

    private bool IsUsernameValid() =>
        !string.IsNullOrWhiteSpace(Username) &&
        Username.Length <= UsernamePolicy.MaxLength &&
        Regex.IsMatch(Username, UsernamePolicy.Pattern);
}