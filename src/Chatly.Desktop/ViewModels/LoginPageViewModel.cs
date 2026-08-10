using System;
using System.Threading.Tasks;
using Chatly.Desktop.Infrastructure.Auth;

namespace Chatly.Desktop.ViewModels;

public sealed class LoginPageViewModel : ViewModelBase
{
	private readonly AuthenticationService _authenticationService;

	private string _statusMessage = "Your browser will open for secure sign-in.";
	private bool _isLoading;

	public LoginPageViewModel(AuthenticationService authenticationService)
	{
		_authenticationService = authenticationService;
	}

	public string StatusMessage
	{
		get => _statusMessage;
		private set => SetProperty(ref _statusMessage, value);
	}

	public bool IsLoading
	{
		get => _isLoading;
		private set => SetProperty(ref _isLoading, value);
	}

	public async Task StartAuthenticationAsync()
	{
		if (IsLoading)
		{
			return;
		}

		IsLoading = true;
		StatusMessage = "Opening your browser...";

		try
		{
			var loginResult = await _authenticationService.LoginAsync();

			if (loginResult.IsError)
			{
				StatusMessage = $"Login failed: {loginResult.Error}";
				return;
			}

			StatusMessage = $"Signed in as {loginResult.User.FindFirst(c => c.Type == "name")?.Value ?? "unknown user"}.";
		}
		catch (Exception exception)
		{
			StatusMessage = $"Login failed: {exception.Message}";
		}
		finally
		{
			IsLoading = false;
		}
	}

}
