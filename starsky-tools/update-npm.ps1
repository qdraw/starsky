# Recursively run `npm run update:install` in all child folders containing package.json with the script

$root = Split-Path -Parent $MyInvocation.MyCommand.Definition

Get-ChildItem -Path $root -Recurse -Filter package.json | ForEach-Object {
    $packageJsonPath = $_.FullName
    $folder = Split-Path $packageJsonPath -Parent
    $package = Get-Content $packageJsonPath -Raw | ConvertFrom-Json
    if ($package.scripts -and $package.scripts.'update:install') {
        Write-Host "Running update:install in $folder"
        Push-Location $folder
        npm run update:install
        Pop-Location
    }
}
