#Requires -RunAsAdministrator

param(
    [Parameter(Mandatory=$false)][switch]$help,
    [Parameter(Mandatory=$false)][string]$port='4000',
    [Parameter(Mandatory=$false)][switch]$anyWhere,
    [Parameter(Mandatory=$false)][string]$outPut,
    [Parameter(Mandatory=$false)][string]$serviceName='starsky',
    [Parameter(Mandatory=$false)][string]$exeName='starsky.exe',
    [Parameter(Mandatory=$false)][switch]$noTelemetry=$false,
    [Parameter(Mandatory=$false)][switch]$remove=$false
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

if ($help -eq $True) {
    Write-Host "help"
    Write-Host "-port 4000"
    Write-Host "-anyWhere"
    Write-Host "-output folder_path"
    Write-Host "-serviceName name_service"
    Write-Host "-exeName starsky.exe"
    Write-Host "-remove (to remove the service, keep files on disk)"
    exit 0
}

Write-Host "-port"$port "-anywhere"$anyWhere "-serviceName"$serviceName "-noTelemetry"$noTelemetry

if($IsWindows -eq $False) {
  Write-Host "This script is currently for windows only"
  exit 0
}

if($outPut -eq "") {
    $scriptRootPath = (Split-Path $MyInvocation.MyCommand.Path -Parent)
    $outPut = $scriptRootPath
}

$exePath = Join-Path $outPut $exeName

if ((Test-Path -Path $exePath) -eq $false) {
    Write-Host "-output path doesn't exist." $exePath
    Write-Host "end script due fail"
    exit 1
}

Write-Host "next close windows of mmc.exe"
Invoke-Expression -Command "taskkill /F /IM mmc.exe"


function ReinstallService ($localServiceName, $binaryPath, $cmdArgs, $description, $login, $password, $startUpType, $displayName, $localRemove)
{
    Write-Output "Trying to create service: $localServiceName - $binaryPath"

    #Check Parameters
    if ((Test-Path $binaryPath)-eq $false)
    {
        Write-Output "BinaryPath to service not found: $binaryPath"
        Write-Output "Service was NOT installed."
        return
    }

    if (("Automatic", "Manual", "Disabled") -notcontains $startUpType)
    {
        Write-Output "Value for startUpType parameter should be (Automatic or Manual or Disabled) and it was $startUpType"
        Write-Output "Service was NOT installed."
        return
    }

    # Verify if the service already exists, and if yes remove it first
    if (Get-Service $localServiceName -ErrorAction SilentlyContinue)
    {
        # using WMI to remove Windows service because PowerShell does not have CmdLet for this
        $serviceToRemove = Get-CimInstance -ClassName Win32_Service -Filter "name='$localServiceName'"
        $id = $serviceToRemove | Select-Object -ExpandProperty ProcessId

        Stop-Process -ID $id -Force -ErrorAction SilentlyContinue

        Write-Output "next delete:"

        $serviceToRemove.Delete()
        #  for powershell 6+
        # Remove-Service -Name ServiceName
        # or sc.exe delete ServiceName
        Write-Output "Service removed: $localServiceName"

        if ($localRemove) {
            Write-Output "remove flag used so done now"
            return;
        }
    }

    # if password is empty, create a dummy one to allow have credentias for system accounts:
    #NT AUTHORITY\LOCAL SERVICE
    #NT AUTHORITY\NETWORK SERVICE
    if ($password -eq "")
    {
        #$secpassword = (new-object System.Security.SecureString)
        # Bug detected by @GaTechThomas
        $secpasswd = (New-Object System.Security.SecureString)
    }
    else
    {
        $secpasswd = ConvertTo-SecureString $password -AsPlainText -Force
    }
    $mycreds = New-Object System.Management.Automation.PSCredential ($login, $secpasswd)

    # Creating Windows Service using all provided parameters
    Write-Output "Installing service: $localServiceName"

    $binaryPathName = "$binaryPath $cmdArgs"
    New-Service -Name $localServiceName -binaryPathName $binaryPathName -Description $description -displayName $displayName `
        -startupType $startUpType -Credential $mycreds

    Write-Output "Installation completed: $localServiceName"

    # Trying to start new service
    Write-Output "Trying to start new service: $localServiceName"
    $serviceToStart = Get-CimInstance -ClassName Win32_Service -Filter "name='$localServiceName'"
    $serviceToStart.StartService()
    Write-Output "Service started: $localServiceName"

    # SmokeTest
    Write-Output "Waiting 5 seconds to give time service to start..."
    Start-Sleep -s 5
    $SmokeTestService = Get-Service -Name $localServiceName
    if ($SmokeTestService.Status -ne "Running")
    {
        Write-Output "Smoke test: FAILED. (SERVICE FAILED TO START)"
        throw "Smoke test: FAILED. (SERVICE FAILED TO START)"
    }
    else
    {
        Write-Output "Smoke test: OK."
    }
    # https://stackoverflow.com/questions/14708825/how-to-create-a-windows-service-in-powershell-for-network-service-account
}

$cmdArgsAdd = '--urls "http://localhost:' + $port + '"'

if ($anyWhere -eq $true) {
    $cmdArgsAdd = '--urls "http://*:' + $port + '"'
}

if($noTelemetry -eq $true) {
     $cmdArgsAdd += " --app:enablePackageTelemetry=False"
}

Write-Output "args: "$cmdArgsAdd

ReinstallService $serviceName $exePath $cmdArgsAdd "Windows service" "NT AUTHORITY\NETWORK SERVICE" "" "Automatic" "Starsky Web App" $remove

Write-Output "done"
exit 0