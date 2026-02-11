# Recursively run `npm run update:install` in all child folders containing package.json with the script

$root = Split-Path -Parent $MyInvocation.MyCommand.Definition
$maxDepth = 2 # Change this value to set the max depth

Get-ChildItem -Path $root -Recurse -Filter package.json | Where-Object {
    ($_.FullName -replace [regex]::Escape($root), '') -split '[\\/]' | Where-Object { $_ -ne '' } | Measure-Object | Select-Object -ExpandProperty Count
} | ForEach-Object {
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
