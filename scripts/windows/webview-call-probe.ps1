$ErrorActionPreference = 'Stop'

$RootDir = (Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
$Project = Join-Path $RootDir 'tests/Chatly.WebViewCallProbe.IntegrationTests/Chatly.WebViewCallProbe.IntegrationTests.csproj'
$Configuration = if ($env:CONFIGURATION) { $env:CONFIGURATION } else { 'Debug' }

dotnet test --project $Project --configuration $Configuration --explicit only @args
exit $LASTEXITCODE
