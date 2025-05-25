$ErrorActionPreference = 'Stop'

$version = 'v1.0.0'

$configJson = Get-Content 'publish-config.json' -Raw | ConvertFrom-Json
$srcPath = [IO.Path]::Combine('src', 'Hitorus.Api')
$outputPath = [IO.Path]::Combine($configJson.ReleasePath, $version, 'Hitorus.Api')
Remove-Item -Path $outputPath -Recurse -Force
dotnet publish $srcPath --output $outputPath