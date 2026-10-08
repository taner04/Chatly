param(
    [switch]$Force
)

$ErrorActionPreference = 'Stop'

$RootDir = (Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
$Utf8WithoutBom = New-Object System.Text.UTF8Encoding $false

function New-RandomSecret {
    $bytes = New-Object byte[] 32
    $generator = [System.Security.Cryptography.RandomNumberGenerator]::Create()
    try {
        $generator.GetBytes($bytes)
    }
    finally {
        $generator.Dispose()
    }

    return -join ($bytes | ForEach-Object { $_.ToString('x2') })
}

function Write-Settings {
    param(
        [Parameter(Mandatory)]
        [string]$RelativePath,

        [Parameter(Mandatory)]
        [string]$Content
    )

    $path = Join-Path $RootDir $RelativePath

    if ((Test-Path $path) -and -not $Force) {
        Write-Host "Skipped  $RelativePath (already exists, use -Force to overwrite)"
        return
    }

    [System.IO.File]::WriteAllText($path, $Content, $Utf8WithoutBom)
    Write-Host "Created  $RelativePath"
}

$LiveKitApiSecret = New-RandomSecret
$KeycloakApiClientSecret = New-RandomSecret

Write-Settings 'tools/Chatly.AppHost/appsettings.json' @"
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
    "keycloak-api-client-secret": "$KeycloakApiClientSecret",
    "keycloak-seed-user-password": "dev",
    "livekit-api-secret": "$LiveKitApiSecret"
  },
  "EmailOption": {
    "SenderName": "Chatly",
    "SenderEmail": "noreply@chatly.local",
    "Username": "",
    "Password": "",
    "UseSsl": false
  }
}
"@

Write-Settings 'src/Chatly.WebApi/appsettings.json' @'
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
'@

Write-Settings 'src/Chatly.Desktop/appsettings.json' @'
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
'@

Write-Host ''
Write-Host 'Start the backend with: dotnet run --project ./tools/Chatly.AppHost'
Write-Host 'Sign in with taner@byom.de or test@byom.de, password: dev'
