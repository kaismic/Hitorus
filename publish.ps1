$version = 'v1.0.0'
$projectNames = @('Hitorus.Api', 'Hitorus.Web')

Set-Location $PSScriptRoot
$configJson = Get-Content 'publish-config.json' -Raw | ConvertFrom-Json
$srcPaths = $projectNames | ForEach-Object { [IO.Path]::Combine('src', $_) }
$outputPaths = $projectNames | ForEach-Object { [IO.Path]::Combine($configJson.ReleasePath, $version, $_) }

$ErrorActionPreference = 'Stop'
Write-Host 'Enter a option/options from the following (separated by space):'
Write-Host '0 - all, 1 - api, 2 - web, 3 - scripts, 4 - dotnet-serve'
$userInput = Read-Host
$options = $userInput.Split(' ')

if ($options.Contains('0') -or $options.Contains('1')) {
    $srcPath = $srcPaths[0]
    $outputPath = $outputPaths[0]
    Remove-Item -Path $outputPath -Recurse -Force
    dotnet publish $srcPath --output $outputPath
    # TODO change .csproj <Version> attribute
}
if ($options.Contains('0') -or $options.Contains('2')) {
    $srcPath = $srcPaths[1]
    $outputPath = $outputPaths[1]
    Remove-Item -Path $outputPath -Recurse -Force
    dotnet publish $srcPath --output $outputPath
    $wwwroot = [IO.Path]::Combine($outputPath, 'wwwroot')
    # remove the compressed .br .gz files because they cannot be modified
    # and browser will use them instead of the actual appsettings.json file
    # when updating api url
    Remove-Item -Path ([IO.Path]::Combine($wwwroot, 'appsettings.json.br'))
    Remove-Item -Path ([IO.Path]::Combine($wwwroot, 'appsettings.json.gz'))
    # TODO change .csproj <Version> attribute
}
if ($options.Contains('0') -or $options.Contains('3')) {
    $scriptSrcPath = [IO.Path]::Combine('run-scripts', '*')
    $scriptOutputPath = [IO.Path]::Combine($configJson.ReleasePath, $version)
    Copy-item -Force $scriptSrcPath -Destination $scriptOutputPath
}
if ($options.Contains('0') -or $options.Contains('4')) {
    $srcPath = [IO.Path]::Combine('dotnet-serve', 'src', 'dotnet-serve')
    $outputPath = [IO.Path]::Combine($configJson.ReleasePath, $version, 'dotnet-serve')
    Remove-Item -Path $outputPath -Recurse -Force
    dotnet publish $srcPath --output $outputPath
}