#!/usr/bin/env bash

set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)"
FORCE=false

for argument in "$@"; do
    case "$argument" in
        --force) FORCE=true ;;
        *)
            printf 'Unknown argument: %s\nUsage: %s [--force]\n' "$argument" "$0" >&2
            exit 1
            ;;
    esac
done

random_secret() {
    head -c 32 /dev/urandom | od -An -tx1 | tr -d ' \n'
}

LIVEKIT_API_SECRET="$(random_secret)"
KEYCLOAK_API_CLIENT_SECRET="$(random_secret)"

write_settings() {
    local path="$ROOT_DIR/$1"

    if [[ -f "$path" && "$FORCE" != true ]]; then
        printf 'Skipped  %s (already exists, use --force to overwrite)\n' "$1"
        cat > /dev/null
        return
    fi

    cat > "$path"
    printf 'Created  %s\n' "$1"
}

write_settings "tools/Chatly.AppHost/appsettings.json" <<JSON
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
  "LiveKitOption": {
    "Image": "livekit/livekit-server",
    "Tag": "v1.13.7",
    "BindAddress": "0.0.0.0",
    "NodeIp": "127.0.0.1",
    "HttpPort": 7880,
    "RtcTcpPort": 7881,
    "RtcUdpPort": 7882,
    "ApiKey": "chatly-dev"
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
    "keycloak-api-client-secret": "$KEYCLOAK_API_CLIENT_SECRET",
    "keycloak-seed-user-password": "dev",
    "livekit-api-secret": "$LIVEKIT_API_SECRET"
  },
  "EmailOption": {
    "SenderName": "Chatly",
    "SenderEmail": "noreply@chatly.local",
    "Username": "",
    "Password": "",
    "UseSsl": false
  }
}
JSON

write_settings "src/Chatly.WebApi/appsettings.json" <<'JSON'
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
  },
  "EmailOption": {
    "Host": "smtp.example.com",
    "Port": 587,
    "SenderName": "Chatly",
    "SenderEmail": "noreply@example.com",
    "Username": "smtp-user",
    "Password": "smtp-password",
    "UseSsl": true
  }
}
JSON

write_settings "src/Chatly.Desktop/appsettings.json" <<'JSON'
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
JSON

printf '\nStart the backend with: dotnet run --project ./tools/Chatly.AppHost\n'
printf 'Sign in with taner@byom.de or test@byom.de, password: dev\n'
