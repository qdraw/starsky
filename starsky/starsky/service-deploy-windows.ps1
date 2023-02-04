#Requires -RunAsAdministrator

param(
    [Parameter(Mandatory=$false)][switch]$help,
    [Parameter(Mandatory=$false)][string]$port='5000',
    [Parameter(Mandatory=$false)][switch]$anyWhere,
    [Parameter(Mandatory=$false)][string]$outPut,
    [Parameter(Mandatory=$false)][string]$serviceName='starsky',
    [Parameter(Mandatory=$false)][string]$exeName='starsky.exe'
)

# for powershell 5
switch ([System.Environment]::OSVersion.Platform)
{
    'Win32NT' { 
        New-Variable -Option Constant -Name IsWindows -Value $True -ErrorAction SilentlyContinue
        New-Variable -Option Constant -Name IsLinux  -Value $false -ErrorAction SilentlyContinue
        New-Variable -Option Constant -Name IsMacOs  -Value $false -ErrorAction SilentlyContinue
     }
}

if($IsWindows -eq $False) {
  Write-host "This script is currently for windows only"
  exit 0
}

if ($help -eq $True) {
    write-host "help"

    exit 0
}

write-host "-port"$port "-anywhere"$anyWhere "-serviceName"$serviceName

If($outPut -eq "") {
    $scriptRootPath = (Split-Path $MyInvocation.MyCommand.Path -Parent)
    $outPut = $scriptRootPath
}
$exePath = Join-Path $outPut $exeName


if ((Test-Path -Path $exePath) -eq $false) {
    write-host "Path doesn't exist." $exePath
    write-host "end script due fail"
    exit 1
} 

write-host $exePath

$currentService = Get-Service -Name $serviceName -ErrorAction SilentlyContinue

If([string]::IsNullOrWhitespace($currentService) -eq $false) {
    write-host "first delete service with the same name: "$currentService

    $filter = "Name='" + $serviceName + "'"
    $service = Get-WmiObject -Class Win32_Service -Filter $filter
    $service.delete()
    #  for powershell 6+
    # Remove-Service -Name ServiceName
    # or sc.exe delete ServiceName
}

$binaryPathName = '"'+ $exePath + '"'

write-host  "test"
write-host $binaryPathName

$params = @{
  Name = $serviceName
  BinaryPathName = $exePath
  DependsOn = "NetLogon"
  DisplayName = "Test Service"
  StartupType = "Automatic"
  Description = "This is a test service."
}
New-Service @params