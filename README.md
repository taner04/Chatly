<div align="center">
  <img src="docs/assets/logo-transparent.png" alt="Chatly Logo" width="300" height="300" />
</div>

# Chatly

## 🎯 Features

- **💬 Real-Time Messaging** — Persistent one-to-one chats powered by SignalR
- **⌨️ Live Activity** — Online presence, typing indicators, unread counts, and read state
- **👥 Friend Management** — Search users, send requests, accept or decline requests, and remove friends
- **🔐 Secure Authentication** — Auth0 OIDC authentication with PKCE and secure refresh-token storage
- **🖥️ Native Desktop Client** — Avalonia application for Windows and macOS
- **🎨 Personalization** — Light, dark, and system themes with multiple accent colors
- **🔔 Notifications** — In-app toast notifications with optional notification sounds
- **🖼️ User Profiles** — Custom usernames and profile pictures stored in Azure Blob Storage
- **📦 RESTful API** — Feature-driven ASP.NET Core Minimal APIs with CQRS patterns
- **💾 Data Persistence** — PostgreSQL database with Entity Framework Core
- **☁️ Cloud-Native** — Built with .NET Aspire for local orchestration and deployment
- **📖 Interactive API Docs** — Scalar UI for API exploration
- **📊 Observability** — OpenTelemetry, health checks, service discovery, and HTTP resilience

---

## 📋 Requirements

- **.NET 10 SDK** — [Download](https://dotnet.microsoft.com/download/dotnet/10.0)
- **Docker Desktop** — [Download](https://www.docker.com/products/docker-desktop)
- **Auth0 Account** — [Sign up free](https://auth0.com/signup)
- **Windows or macOS** — Required for the desktop client

---

## 🚀 Getting Started

### 1. Clone Repository

```bash
git clone https://github.com/taner04/Chatly
cd Chatly
```

### 2. Configure Auth0

1. Go to the [Auth0 Dashboard](https://manage.auth0.com/)
2. Create a **Native Application**
3. Create an **API** in Auth0 and set its **Identifier** (Audience)
4. Enable refresh tokens and offline access for the application
5. Copy the **Domain**, **Client ID**, and **API Audience**
6. Set the following URLs in the Auth0 application settings:
   - **Allowed Callback URLs**: `http://127.0.0.1:7890/callback/`
   - **Allowed Logout URLs**: `http://127.0.0.1:7890/callback/`

### 3. Configure Web API

Create `src/Chatly.WebApi/appsettings.json` with the following template:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "Auth0Option": {
    "Domain": "your-auth0-domain",
    "Audience": "your-auth0-api-audience",
    "ClientId": "your-auth0-client-id",
    "UsePersistentStorage": false
  }
}
```

### 4. Configure Desktop Client

Create `src/Chatly.Desktop/appsettings.json` with the following template:

```json
{
  "Auth0Option": {
    "Domain": "your-auth0-domain",
    "ClientId": "your-auth0-client-id",
    "Audience": "your-auth0-api-audience",
    "ConnectionName": "Username-Password-Authentication",
    "Scope": "openid profile email offline_access",
    "RedirectUri": "http://127.0.0.1:7890/callback/"
  },
  "DesktopProfileOption": {
    "Name": "default"
  },
  "WebApiClientOption": {
    "BaseAddress": "https://localhost:7123/",
    "TimeoutInSeconds": 30,
    "HubAddress": "https://localhost:7123/hubs/notification"
  }
}
```

### 5. Run Backend

```bash
dotnet run --project ./tools/Chatly.AppHost
```

.NET Aspire starts PostgreSQL, Azure Storage, database migrations, and the Web API. The Aspire dashboard displays the active API endpoints.

In development, Scalar API documentation is available from the `chatly-api` resource at `/scalar/v1`.

### 6. Run Desktop Client

In another terminal, run:

```bash
dotnet run --project ./src/Chatly.Desktop
```

The desktop client waits for the API, database, and blob storage to become ready before opening the Auth0 sign-in flow.

---

## 📁 Project Structure

```
Chatly/
├── docs/                         # Documentation and logo assets
├── scripts/                      # Development helper scripts
├── src/
│   ├── Chatly.Contracts/         # REST and SignalR contracts
│   ├── Chatly.Desktop/           # Avalonia desktop client
│   ├── Chatly.Shared/            # Shared options and utilities
│   ├── Chatly.SourceGenerator/   # Dependency injection source generators
│   └── Chatly.WebApi/            # ASP.NET Core API and persistence
└── tools/
    ├── Chatly.AppHost/           # .NET Aspire orchestration
    ├── Chatly.MigrationService/  # Database migrations and development seeding
    └── Chatly.ServiceDefaults/   # Telemetry, health, discovery, and resilience
```

## 📄 License

MIT License — see [LICENSE](LICENSE)
