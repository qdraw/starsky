[CmdletBinding()]
Param(
    [string]$Script = "build.cake",
    [string]$Target,
    [Parameter(Position=0,Mandatory=$false,ValueFromRemainingArguments=$true)]
    [string[]]$ScriptArgs
)

$env:DOTNET_CLI_TELEMETRY_OPTOUT = "true"
$env:DOTNET_SKIP_FIRST_TIME_EXPERIENCE = "1"

# Restore Cake tool
& dotnet tool restore

# Build Cake arguments
$cakeArguments = @("$Script");
if ($Target) { $cakeArguments += "--target=$Target" }
$cakeArguments += $ScriptArgs

& dotnet tool run dotnet-cake -- $cakeArguments --verbosity=Normal
exit $LASTEXITCODE
