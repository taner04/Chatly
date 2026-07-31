using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Auth0.OidcClient;
using Chatly.Desktop.Infrastrucuture;
using Chatly.Desktop.Options;
using Microsoft.Extensions.Options;

namespace Chatly.Desktop.ViewModels;

public sealed class LoginPageViewModel : ViewModelBase
{
	private readonly Auth0Option _config;
	private readonly Dictionary<string, string> _extraParameters;

	private string _statusMessage = "Your browser will open for secure sign-in.";
	private bool _isLoading;

	public LoginPageViewModel(IOptions<Auth0Option> auth0Options)
	{
		_config = auth0Options.Value;

		_extraParameters = new Dictionary<string, string>
		{
			{ "connection", _config.ConnectionName },
			{ "audience", _config.Audience }
		};
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
			var client = new DesktopAuth0Client(new Auth0ClientOptions
			{
				Domain = _config.Domain,
				ClientId = _config.ClientId,
				Scope = _config.Scope,
				RedirectUri = _config.RedirectUri,
				Browser = new SystemBrowser()
			});

			var loginResult = await client.LoginAsync(_extraParameters);

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

	private sealed class DesktopAuth0Client(Auth0ClientOptions options)
		: Auth0ClientBase(options, "Chatly.Desktop");
}
