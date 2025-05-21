$version = 'v1.0.0'
$projectNames = @('Hitorus.Api', 'Hitorus.Web')

Set-Location $PSScriptRoot
$configJson = Get-Content 'publish-config.json' -Raw | ConvertFrom-Json
$srcPaths = $projectNames | ForEach-Object { [IO.Path]::Combine('src', $_) }
$outputPaths = $projectNames | ForEach-Object { [IO.Path]::Combine($configJson.ReleasePath, $version, $_) }

$ErrorActionPreference = 'Stop'
Write-Host 'Choose the project to build and publish'
Write-Host "$($projectNames[0]) - 1, $($projectNames[1]) - 2, Both - 3"
[int]$arg = Read-Host

if ($arg -lt 1 -or $arg -gt 3) {
    throw "Invalid input. Enter a value between 1 - 3"
}

$options = @()
if ($arg -band 1) {
    $options += 0
}
if ($arg -band 2) {
    $options += 1
    Set-Location $outputPaths[1]
    # TODO try installing dotnet-serve locally in the web app release folder
}

Set-Location $PSScriptRoot
foreach ($o in $options) {
    dotnet publish $srcPaths[$o] --output $outputPaths[$o]
    dotnet tool install --local dotnet-serve --version 1.10.175 --create-manifest-if-needed
}