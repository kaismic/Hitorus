# ------------- NOTE -------------
# This script must be placed in the same directory with Hitorus.Api and Hitorus.Web

$WEB_PORT = 5214
$API_PORT = 7076

Set-Location $PSScriptRoot

# This is needed if the powershell version is < 6. See https://github.com/PowerShell/PowerShell/issues/2736
# reference: https://github.com/PowerShell/PowerShell/issues/2736#issue-190538839
# Formats JSON in a nicer format than the built-in ConvertTo-Json does.
function Format-Json(
    [Parameter(Mandatory, ValueFromPipeline)][String] $json,
    [Parameter()][int] $Indentation = 2
    ) {
    $i = 0;
    ($json -Split '\n' |
        ForEach-Object {
        if ($_ -match '[\}\]]') {
            # This line contains  ] or }, decrement the indentation level
            $i--
        }
        $line = (' ' * $i * $Indentation) + $_.TrimStart().Replace(':  ', ': ')
        if ($_ -match '[\{\[]') {
            # This line contains [ or {, increment the indentation level
            $i++
        }
        $line
    }) -Join "`n"
}

$webAppSettingsPath = [IO.Path]::Combine('Hitorus.Web', 'wwwroot', 'appsettings.json')
$webAppSettingsJson = Get-Content $webAppSettingsPath -Raw | ConvertFrom-Json
$webApiUri = New-Object System.Uri($webAppSettingsJson.ApiUrl)
if ($webApiUri.Port -ne $API_PORT) {
    $webAppSettingsJson.ApiUrl = "https://localhost:$($API_PORT)/api/"
    $webAppSettingsJson | ConvertTo-Json -Depth 2 | Format-Json | Out-File $webAppSettingsPath
}
$webApiUri = $null

$apiAppSettingsPath = [IO.Path]::Combine('Hitorus.Api', 'appsettings.json')
$apiAppSettingsJson = Get-Content $apiAppSettingsPath -Raw | ConvertFrom-Json
$apiHostUri = New-Object System.Uri($apiAppSettingsJson.Kestrel.Endpoints.Https.Url)
if ($apiHostUri.Port -ne $API_PORT) {
    $apiAppSettingsJson.Kestrel.Endpoints.Https.Url = "https://localhost:$($API_PORT)/"
    $apiAppSettingsJson | ConvertTo-Json -Depth 4 | Format-Json | Out-File $apiAppSettingsPath
}
$apiHostUri = $null

# Install dotnet-serve if not exists
$json = dotnet tool list -g dotnet-serve --format json | ConvertFrom-Json
if ($json.data.Count -eq 0) {
    dotnet tool install -g dotnet-serve --version 1.10.175
}

# Run API
Set-Location ([IO.Path]::Combine($PSScriptRoot, 'Hitorus.Api'))
Start-Process powershell {dotnet 'Hitorus.Api.dll'}
# Run Web App
Set-Location ([IO.Path]::Combine($PSScriptRoot, 'Hitorus.Web'))
dotnet serve -d 'wwwroot' -o -p $WEB_PORT -q