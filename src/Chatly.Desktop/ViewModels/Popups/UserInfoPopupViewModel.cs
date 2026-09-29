using System.ComponentModel;
using System.Text.RegularExpressions;
using Chatly.Contracts.Common.Policies;
using Chatly.Contracts.Features.Users.Endpoints.GetCurrentUser;
using Chatly.Contracts.Features.Users.Endpoints.UpdateProfilePicture;
using Chatly.Contracts.Features.Users.Endpoints.UpdateUsername;
using Chatly.Desktop.Abstraction.Storage;
using Chatly.Desktop.Mappers;
using Chatly.Desktop.Services.Api.Clients;
using Chatly.Desktop.Services.Api.Results;

namespace Chatly.Desktop.ViewModels.Popups;

[TransientService]
public sealed partial class UserInfoPopupViewModel : ProfilePicturePopupViewModel
{
    private readonly UserSessionContext _sessionContext;
    private readonly IToastService _toastService;
    private readonly UserApiClient _userApiClient;

    public UserInfoPopupViewModel(
        UserSessionContext sessionContext,
        IToastService toastService,
        UserApiClient userApiClient,
        IFilePicker filePicker)
        : this(
            sessionContext,
            toastService,
            userApiClient,
            filePicker,
            sessionContext.CurrentUser
            ?? throw new InvalidOperationException("A signed-in user is required."))
    {
    }

    private UserInfoPopupViewModel(
        UserSessionContext sessionContext,
        IToastService toastService,
        UserApiClient userApiClient,
        IFilePicker filePicker,
        User user)
        : base(filePicker, user.ProfilePictureUrl)
    {
        _sessionContext = sessionContext;
        _toastService = toastService;
        _userApiClient = userApiClient;
        User = user;
        OriginalUsername = user.Username ?? string.Empty;
        Username = OriginalUsername;
        User.PropertyChanged += OnUserPropertyChanged;
    }

    public override string Title => "Your profile";

    public override bool IsDismissible => true;

    public int UsernameMaxLength => UsernamePolicy.MaxLength;

    public User User { get; }

    public string OriginalUsername { get; }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    public partial string Username { get; set; }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    public partial bool IsSaving { get; private set; }

    public override void CloseOverlay()
    {
        SaveCommand.Cancel();
        User.PropertyChanged -= OnUserPropertyChanged;
        base.CloseOverlay();
    }

    [RelayCommand(CanExecute = nameof(CanSave))]
    private async Task Save(CancellationToken cancellationToken)
    {
        IsSaving = true;
        var changed = false;

        try
        {
            if (!string.Equals(Username, OriginalUsername, StringComparison.Ordinal))
            {
                var usernameResult = await _userApiClient.UpdateUsernameAsync(
                    new UpdateUsernameRequest(Username),
                    cancellationToken);

                if (!ApplyResult(usernameResult))
                {
                    return;
                }

                changed = true;
            }

            if (ProfilePicture is not null)
            {
                await using var content = await ProfilePicture.OpenReadAsync();
                var pictureResult = await _userApiClient.UpdateProfilePictureAsync(
                    new UpdateProfilePictureRequest(
                        content,
                        ProfilePicture.Name,
                        GetContentType(ProfilePicture)),
                    cancellationToken);

                if (!ApplyResult(pictureResult))
                {
                    return;
                }

                changed = true;
            }
            else if (IsProfilePictureRemoved && !string.IsNullOrWhiteSpace(User.ProfilePictureUrl))
            {
                var pictureResult = await _userApiClient.UpdateProfilePictureAsync(
                    new UpdateProfilePictureRequest(),
                    cancellationToken);

                if (!ApplyResult(pictureResult))
                {
                    return;
                }

                changed = true;
            }

            if (changed)
            {
                _toastService.ShowSuccess("Profile updated.");
            }

            CloseOverlay();
        }
        finally
        {
            IsSaving = false;
        }
    }

    [RelayCommand]
    private void Cancel()
    {
        CloseOverlay();
    }

    private bool ApplyResult(WebClientResult<CurrentUserResponse> result)
    {
        if (result.IsFailure)
        {
            _toastService.ShowError(result.Error);
            return false;
        }

        _sessionContext.SetAuthenticated(UserMapper.Map(result.Value));
        return true;
    }

    private bool CanSave()
    {
        var usernameChanged = !string.Equals(Username, OriginalUsername, StringComparison.Ordinal);
        var pictureChanged = ProfilePicture is not null ||
                             (IsProfilePictureRemoved && !string.IsNullOrWhiteSpace(User.ProfilePictureUrl));

        return !IsSaving &&
               IsUsernameValid() &&
               (usernameChanged || pictureChanged);
    }

    private bool IsUsernameValid() =>
        !string.IsNullOrWhiteSpace(Username) &&
        Username.Length <= UsernamePolicy.MaxLength &&
        UsernamePolicyPatternRegex().IsMatch(Username);

    protected override void OnProfilePictureSelectionChanged()
    {
        SaveCommand.NotifyCanExecuteChanged();
    }

    private void OnUserPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(User.ProfilePictureUrl))
        {
            SetExistingProfilePictureUrl(User.ProfilePictureUrl);
        }
    }

    [GeneratedRegex(UsernamePolicy.Pattern)]
    private static partial Regex UsernamePolicyPatternRegex();
}