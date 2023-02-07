#Requires -RunAsAdministrator

Push-Location (Join-Path -Path $PSScriptRoot -ChildPath "starsky")
    Invoke-Expression -Command "./cleanup-build-tools.ps1" -ErrorAction Stop
Pop-Location
