using System.ComponentModel;
using Chatly.Contracts.Endpoints.Users.Requests;
using Chatly.Contracts.Endpoints.Users.Results;
using Chatly.Desktop.Abstraction.Toasts;
using Chatly.Desktop.Extensions;
using Chatly.Desktop.Mappers;
using Chatly.Desktop.Services.Api;
using Chatly.Desktop.Services.Api.Results;
using CommunityToolkit.Mvvm.Input;
using UserSessionContext = Chatly.Desktop.Models.UserSession.UserSessionContext;

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
        UserApiClient userApiClient)
        : this(
            sessionContext,
            toastService,
            userApiClient,
            sessionContext.CurrentUser
            ?? throw new InvalidOperationException("A signed-in user is required."))
    {
    }

    private UserInfoPopupViewModel(
        UserSessionContext sessionContext,
        IToastService toastService,
        UserApiClient userApiClient,
        User user)
        : base(user.ProfilePictureUrl)
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
               !string.IsNullOrWhiteSpace(Username) &&
               (usernameChanged || pictureChanged);
    }

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
}