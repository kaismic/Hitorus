$ErrorActionPreference = 'Stop'

$version = 'v0.9.0'

# $runtimes = @('win-x64', 'win-arm64', 'linux-x64', 'linux-arm64', 'osx-x64', 'osx-arm64')
$runtimes = @('win-x64', 'linux-x64')
$configJson = Get-Content 'publish-config.json' -Raw | ConvertFrom-Json
$srcPath = [IO.Path]::Combine('src', 'Hitorus.Api')
foreach ($runtime in $runtimes) {
    $outputPath = [IO.Path]::Combine($configJson.ReleasePath, $version, "Hitorus.Api-$version-$runtime-non-r2r")
    if (Test-Path $outputPath) {
        Remove-Item -Path $outputPath -Recurse -Force
    }
    dotnet publish $srcPath --output $outputPath -r $runtime
}