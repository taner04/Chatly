$ErrorActionPreference = 'Stop'

$RootDir = (Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
$Configuration = if ($env:CONFIGURATION) { $env:CONFIGURATION } else { 'Debug' }

$SourceProjects = Get-ChildItem -Path (Join-Path $RootDir 'src') -Filter '*.csproj' -Recurse -File
$TestProjects = Get-ChildItem -Path (Join-Path $RootDir 'tests') -Filter '*.csproj' -Recurse -File

foreach ($project in @($SourceProjects) + @($TestProjects)) {
    & dotnet build $project.FullName --configuration $Configuration

    if ($LASTEXITCODE -ne 0) {
        exit $LASTEXITCODE
    }
}

foreach ($project in $TestProjects) {
    $projectDirectory = $project.DirectoryName
    $hasSource = Get-ChildItem -Path $projectDirectory -Filter '*.cs' -Recurse -File |
        Where-Object { $_.FullName -notmatch '[\\/](bin|obj)[\\/]' } |
        Select-Object -First 1

    if (-not $hasSource) {
        continue
    }

    if ($env:UNIT_TESTS_ONLY -eq 'true' -and $project.Name -like '*IntegrationTests.csproj') {
        continue
    }

    & dotnet test $project.FullName `
        --configuration $Configuration `
        --no-build `
        @args

    if ($LASTEXITCODE -ne 0) {
        exit $LASTEXITCODE
    }
}
