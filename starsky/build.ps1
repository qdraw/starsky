[CmdletBinding()]
Param(
    [string]$Script = "build.cake",
    [string]$Target,
    [Parameter(Position=0,Mandatory=$false,ValueFromRemainingArguments=$true)]
    [string[]]$ScriptArgs
)

Push-Location $PSScriptRoot

$env:DOTNET_SKIP_FIRST_TIME_EXPERIENCE = 1
$env:DOTNET_CLI_TELEMETRY_OPTOUT = 1
# When DOTNET_MULTILEVEL_LOOKUP is set to 0, .NET ignores any globally installed .NET version.
$env:DOTNET_MULTILEVEL_LOOKUP = 1

try { 
    [System.Environment]::SetEnvironmentVariable('DOTNET_CLI_TELEMETRY_OPTOUT','true')
    [System.Environment]::SetEnvironmentVariable('DOTNET_SKIP_FIRST_TIME_EXPERIENCE','1') 
}
catch {}


# Restore Cake tool
& dotnet tool restore

# Build Cake arguments
$cakeArguments = @("$Script");
if ($Target) { $cakeArguments += "--target=$Target" }
$cakeArguments += $ScriptArgs

& dotnet tool run dotnet-cake -- $cakeArguments --verbosity=Normal

Pop-Location

exit $LASTEXITCODE
