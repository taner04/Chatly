$ErrorActionPreference = 'Stop'

$RootDir = (Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
$Project = Join-Path $RootDir 'src/Chatly.Desktop/Chatly.Desktop.csproj'
$Configuration = if ($env:CONFIGURATION) { $env:CONFIGURATION } else { 'Debug' }
$Framework = 'net10.0'
$AppDll = Join-Path $RootDir "src/Chatly.Desktop/bin/$Configuration/$Framework/Chatly.Desktop.dll"

$ProfileOne = if ($env:CHATLY_PROFILE_ONE) { $env:CHATLY_PROFILE_ONE } else { 'primary' }
$ProfileTwo = if ($env:CHATLY_PROFILE_TWO) { $env:CHATLY_PROFILE_TWO } else { 'secondary' }
$CallbackOne = if ($env:CHATLY_CALLBACK_ONE) { $env:CHATLY_CALLBACK_ONE } else { 'http://127.0.0.1:7890/callback/' }
$CallbackTwo = if ($env:CHATLY_CALLBACK_TWO) { $env:CHATLY_CALLBACK_TWO } else { 'http://127.0.0.1:7891/callback/' }

function Start-ChatlyClient {
    param(
        [Parameter(Mandatory)]
        [string]$Profile,

        [Parameter(Mandatory)]
        [string]$Callback
    )

    $previousProfile = $env:DesktopProfileOption__Name
    $previousRedirectUri = $env:Auth0Option__RedirectUri
    $previousPrompt = $env:Auth0Option__Prompt

    try {
        $env:DesktopProfileOption__Name = $Profile
        $env:Auth0Option__RedirectUri = $Callback
        $env:Auth0Option__Prompt = 'login'

        return Start-Process dotnet -ArgumentList @($AppDll) -PassThru
    }
    finally {
        $env:DesktopProfileOption__Name = $previousProfile
        $env:Auth0Option__RedirectUri = $previousRedirectUri
        $env:Auth0Option__Prompt = $previousPrompt
    }
}

& dotnet build $Project --configuration $Configuration
if ($LASTEXITCODE -ne 0) {
    exit $LASTEXITCODE
}

$ProcessOne = Start-ChatlyClient -Profile $ProfileOne -Callback $CallbackOne
$ProcessTwo = Start-ChatlyClient -Profile $ProfileTwo -Callback $CallbackTwo

Write-Host "Chatly client $ProfileOne PID: $($ProcessOne.Id)"
Write-Host "Chatly client $ProfileTwo PID: $($ProcessTwo.Id)"
Write-Host 'Attach your debugger to either PID. Press Ctrl+C to stop both clients.'

try {
    Wait-Process -Id $ProcessOne.Id, $ProcessTwo.Id
}
finally {
    foreach ($process in @($ProcessOne, $ProcessTwo)) {
        if (-not $process.HasExited) {
            Stop-Process -Id $process.Id -ErrorAction SilentlyContinue
        }
    }
}
