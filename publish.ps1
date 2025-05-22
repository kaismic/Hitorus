$version = 'v1.0.0'
$projectNames = @('Hitorus.Api', 'Hitorus.Web')

Set-Location $PSScriptRoot
$configJson = Get-Content 'publish-config.json' -Raw | ConvertFrom-Json
$srcPaths = $projectNames | ForEach-Object { [IO.Path]::Combine('src', $_) }
$outputPaths = $projectNames | ForEach-Object { [IO.Path]::Combine($configJson.ReleasePath, $version, $_) }

$ErrorActionPreference = 'Stop'
Write-Host 'Enter a option/options from the following (separated by space):'
Write-Host '"web", "api", "scripts"'
$userInput = Read-Host
$options = $userInput.Split(' ')

if ($options.Contains('api')) {
    dotnet publish $srcPaths[0] --output $outputPaths[0]
    $options += 0
}
if ($options.Contains('web')) {
    dotnet publish $srcPaths[1] --output $outputPaths[1]
    $options += 0
}
if ($options.Contains('scripts')) {
    $scriptSrcPath = [IO.Path]::Combine('run-scripts', '*')
    $scriptOutputPath = [IO.Path]::Combine($configJson.ReleasePath, $version)
    Copy-item -Force $scriptSrcPath -Destination $scriptOutputPath
}