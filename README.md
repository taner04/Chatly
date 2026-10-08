<div align="center">
  <img src="docs/assets/logo-transparent.png" alt="Chatly Logo" width="160" height="160" />

  <h1>Chatly</h1>

  <p>Private one-to-one chats and voice calls — a native desktop app for Windows and macOS.</p>

  <p>
    <a href="https://github.com/taner04/Chatly/actions/workflows/ci.yml"><img src="https://github.com/taner04/Chatly/actions/workflows/ci.yml/badge.svg" alt="CI" /></a>
    <img src="https://img.shields.io/badge/.NET-10-512BD4" alt=".NET 10" />
    <img src="https://img.shields.io/badge/platform-Windows%20%7C%20macOS-lightgrey" alt="Windows | macOS" />
    <a href="LICENSE"><img src="https://img.shields.io/badge/license-MIT-green" alt="MIT License" /></a>
  </p>

  <p>
    <a href="#features">Features</a> ·
    <a href="#getting-started">Getting Started</a> ·
    <a href="#project-structure">Project Structure</a> ·
    <a href="#license">License</a>
  </p>

  <img src="docs/assets/screenshots/chat.png" alt="Chatly chat view" width="900" />
</div>

## About

Chatly is a messenger for people who talk to the same few friends every day. Add a friend, open the chat, and write, share files, react or call — everything updates live on every device you are signed in on.

> [!WARNING]
> Chatly is under active development. It is primarily developed and tested on macOS; **Windows support is not fully tested yet**, so expect rough edges there, especially with voice calls. If you run into a problem on Windows, please [open an issue](https://github.com/taner04/Chatly/issues/new) or contribute a fix via pull request. See [Project Status](#project-status).

## Table of Contents

- [Features](#features)
- [Screenshots](#screenshots)
- [Getting Started](#getting-started)
  - [Prerequisites](#prerequisites)
  - [Installation](#installation)
  - [Configuration](#configuration)
- [Built With](#built-with)
- [Project Status](#project-status)
- [Project Structure](#project-structure)
- [License](#license)

## Features

- **Real-time chat** — messages appear instantly, with typing indicators, unread counts and read state
- **Files and images** — attach files or drag them into the window
- **Reactions** — react to any message with emoji
- **Voice calls** — one-to-one calls with echo cancellation, noise suppression and automatic gain control, plus mute and microphone/speaker selection
- **Multiple devices** — accept a call on one device and your other devices stop ringing
- **Device management** — see every device you are signed in on and sign out a single device or all others; a signed-out device closes immediately
- **Friends** — search by username, send and answer friend requests, see who is online
- **Personalization** — light, dark or system theme, accent colors, username and profile picture
- **Sounds and notifications** — a ringtone while a call is ringing, and a notification sound for new messages and friend requests that you can turn on or off in the settings
- **Secure sign-in** — Keycloak with a Chatly-styled login, refresh tokens stored in the operating system's secure storage

## Screenshots

<table>
  <tr>
    <td width="50%"><img src="docs/assets/screenshots/call-outgoing.png" alt="Outgoing call" /></td>
    <td width="50%"><img src="docs/assets/screenshots/call-incoming.png" alt="Incoming call" /></td>
  </tr>
  <tr>
    <td align="center">Calling a friend</td>
    <td align="center">Incoming call</td>
  </tr>
  <tr>
    <td width="50%"><img src="docs/assets/screenshots/call-active.png" alt="Active call" /></td>
    <td width="50%"><img src="docs/assets/screenshots/friends.png" alt="Friends" /></td>
  </tr>
  <tr>
    <td align="center">In a call</td>
    <td align="center">Friends and online status</td>
  </tr>
  <tr>
    <td colspan="2" align="center"><img src="docs/assets/screenshots/settings.png" alt="Settings" width="50%" /></td>
  </tr>
  <tr>
    <td colspan="2" align="center">Appearance and call settings</td>
  </tr>
</table>

## Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop)
- Windows or macOS for the desktop client

### Installation

1. Clone the repository:

   ```bash
   git clone https://github.com/taner04/Chatly
   cd Chatly
   ```

2. Create the three `appsettings.json` files with development defaults:

   ```bash
   ./scripts/unix/init.sh          # Linux/macOS
   ./scripts/windows/init.ps1      # Windows
   ```

   Existing files are kept; pass `--force` (or `-Force` on Windows) to overwrite them. The LiveKit secret and the Keycloak API client secret are generated randomly. The files and their values are described in [Configuration](#configuration).

3. Start PostgreSQL, Azure Storage, Papercut SMTP, the LiveKit media server, Keycloak, the database migrations and the Web API:

   ```bash
   dotnet run --project ./tools/Chatly.AppHost
   ```

   The Aspire dashboard lists the running resources. In development, the Scalar API documentation is available from the `chatly-api` resource at `/scalar/v1`.

4. In another terminal, start the desktop client:

   ```bash
   dotnet run --project ./src/Chatly.Desktop
   ```

   The client waits until the API, database and blob storage are ready, then opens the Keycloak sign-in in your browser. In development you can sign in with a seeded account, see [Keycloak](#configuration).

### Configuration

<details>
<summary><strong>Keycloak</strong></summary>

The AppHost starts Keycloak and imports the `chatly` realm from `tools/Chatly.AppHost/Realms/chatly-realm.json`. No manual setup is needed.

| Account | Where | Login | Password |
|---|---|---|---|
| Keycloak admin | `https://localhost:8180` (realm `master`) | `admin` | value of `keycloak-admin-password` |
| Seeded user | Chatly and `https://localhost:8180/realms/chatly/account` | `taner@byom.de` | value of `keycloak-seed-user-password` |
| Seeded user | Chatly and `https://localhost:8180/realms/chatly/account` | `test@byom.de` | value of `keycloak-seed-user-password` |

- Switch to the **chatly** realm in the admin console to see its clients and users.
- Users register with email and password only. Username and profile picture are set in Chatly's onboarding.
- The login and email pages use the Chatly theme from `tools/Chatly.AppHost/Themes/chatly`. Keycloak runs in development mode, so theme changes show after a browser reload.
- With `PersistData` set to `false`, Keycloak starts without a data volume and imports the realm fresh on every start. Sessions and self-registered users are lost on restart, and the desktop client has to sign in again. Set it to `true` to keep Keycloak's data; changes to the realm file then only apply after removing the volume.
- Emails such as password reset are sent to Papercut at `http://localhost:8025`.
- Signing out a device in Chatly also ends its Keycloak session. Sign-outs started in Keycloak (account console, admin console, or "Sign out from other devices" when changing the password) reach Chatly through OIDC back-channel logout.

</details>

<details>
<summary><strong>AppHost</strong> — <code>tools/Chatly.AppHost/appsettings.json</code></summary>

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Aspire.Hosting.Dcp": "Warning",
      "Aspire.Hosting.Dashboard": "Error"
    }
  },
  "PapercutOption": {
    "Image": "changemakerstudiosus/papercut-smtp",
    "Tag": "7.6",
    "HttpPort": 8025,
    "SmtpPort": 2525
  },
  "EmailOption": {
    "SenderName": "Chatly",
    "SenderEmail": "noreply@chatly.local",
    "Username": "",
    "Password": "",
    "UseSsl": false
  },
  "LiveKitOption": {
    "Image": "livekit/livekit-server",
    "Tag": "v1.13.7",
    "BindAddress": "0.0.0.0",
    "NodeIp": "127.0.0.1",
    "HttpPort": 7880,
    "RtcTcpPort": 7881,
    "RtcUdpPort": 7882,
    "ApiKey": "your-livekit-api-key"
  },
  "KeycloakOption": {
    "Registry": "docker.io",
    "Image": "keycloak/keycloak",
    "Tag": "26.4",
    "Port": 8180,
    "RealmImportPath": "Realms",
    "ThemesPath": "Themes",
    "PersistData": false
  },
  "Parameters": {
    "keycloak-admin-username": "admin",
    "keycloak-admin-password": "dev",
    "keycloak-api-client-secret": "your-keycloak-api-client-secret",
    "keycloak-seed-user-password": "dev",
    "livekit-api-secret": "your-livekit-api-secret-at-least-32-characters"
  }
}
```

The AppHost starts Papercut, the LiveKit media server and Keycloak with these values. It passes the LiveKit server URL, API key and secret to the Web API, and points the Web API's `EmailOption` at Papercut's SMTP endpoint using the sender and login values from `EmailOption`. Keycloak sends its emails to Papercut as well. The `keycloak-api-client-secret` is set on the realm's `chatly-api` client and passed to the Web API, which uses it to end Keycloak sessions. All secrets are Aspire parameters under `Parameters`, so the dashboard masks them; `livekit-api-secret` must be at least 32 characters.

</details>

<details>
<summary><strong>Web API</strong> — <code>src/Chatly.WebApi/appsettings.json</code></summary>

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "OidcOption": {
    "Authority": "https://localhost:8180/realms/chatly",
    "Audience": "chatly-api",
    "ClientId": "chatly-api-docs",
    "BackchannelLogoutAudience": "chatly-desktop",
    "UsePersistentStorage": false
  },
  "IdentityAdminOption": {
    "ClientId": "chatly-api",
    "ClientSecret": ""
  }
}
```

</details>

<details>
<summary><strong>Desktop client</strong> — <code>src/Chatly.Desktop/appsettings.json</code></summary>

```json
{
  "OidcOption": {
    "Authority": "https://localhost:8180/realms/chatly",
    "ClientId": "chatly-desktop",
    "Scope": "openid profile email",
    "RedirectUri": "http://127.0.0.1:7890/callback/"
  },
  "DesktopProfileOption": {
    "Name": "default"
  },
  "WebApiClientOption": {
    "BaseAddress": "https://localhost:7123/",
    "TimeoutInSeconds": 30
  }
}
```

</details>

## Built With

| Area | Technology |
|---|---|
| Desktop client | [Avalonia](https://avaloniaui.net/) 12, CommunityToolkit.Mvvm |
| Backend | ASP.NET Core Minimal APIs, SignalR, Mediator (CQRS) |
| Voice | [LiveKit](https://livekit.io/) media server, WebRTC in a native WebView |
| Data | PostgreSQL with Entity Framework Core, Azure Blob Storage for files |
| Sign-in | [Keycloak](https://www.keycloak.org/) (OIDC with PKCE, back-channel logout) |
| Local orchestration | .NET Aspire, OpenTelemetry, health checks |
| API docs | Scalar |

## Project Status

Chatly is a work in progress. These tasks are still open:

- [ ] Test the desktop client end to end on Windows, including voice calls (`scripts/windows/webview-call-probe.ps1`)
- [ ] Add a setting to turn the ringtone on or off
- [ ] Let users withdraw a friend request they have sent
- [ ] Let users block other users
- [ ] Add rate limiting to the API
- [ ] Let users edit and reply to messages
- [ ] Show system notifications while the app is in the background

## Project Structure

```
Chatly/
├── docs/                                          # Documentation, logo and screenshots
├── scripts/                                       # Platform-specific development helpers
│   ├── unix/                                      # Linux/macOS setup and test scripts
│   └── windows/                                   # PowerShell setup and test scripts
├── src/
│   ├── Chatly.Contracts/                          # REST and SignalR contracts
│   ├── Chatly.Desktop/                            # Avalonia desktop client and WebView call media
│   ├── Chatly.Shared/                             # Shared options and utilities
│   ├── Chatly.SourceGenerator/                    # Dependency injection and option source generators
│   └── Chatly.WebApi/                             # ASP.NET Core API, SignalR hubs, and persistence
├── tests/
│   ├── Chatly.Desktop.UnitTests/                  # Desktop services, state, view model, and sound asset tests
│   ├── Chatly.SourceGenerator.UnitTests/          # Generator output and diagnostics tests
│   ├── Chatly.WebApi.IntegrationTests/            # API and hub tests with Testcontainers (requires Docker)
│   ├── Chatly.WebApi.UnitTests/                   # Contract serialization and LiveKit unit tests
│   └── Chatly.WebViewCallProbe.IntegrationTests/  # Explicit WebView WebRTC checks per OS
└── tools/
    ├── Chatly.AppHost/                            # .NET Aspire orchestration, Keycloak realm and themes
    ├── Chatly.MigrationService/                   # Database migrations and development seeding
    ├── Chatly.ServiceDefaults/                    # Telemetry, health, discovery, and resilience
    └── Chatly.WebViewCallProbe/                   # WebView WebRTC capability check for Windows and macOS
```

## License

Chatly is licensed under the [MIT License](LICENSE).
