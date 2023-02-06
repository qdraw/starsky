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
    write-host "-port 5000"
    write-host "-anyWhere"
    write-host "-output folder_path"
    write-host "-serviceName name_service"
    write-host "-exeName starsky.exe"
    exit 0
}

write-host "-port"$port "-anywhere"$anyWhere "-serviceName"$serviceName

If($outPut -eq "") {
    $scriptRootPath = (Split-Path $MyInvocation.MyCommand.Path -Parent)
    $outPut = $scriptRootPath
}
$exePath = Join-Path $outPut $exeName


if ((Test-Path -Path $exePath) -eq $false) {
    write-host "-output path doesn't exist." $exePath
    write-host "end script due fail"
    exit 1
} 

write-host $exePath

Invoke-Expression -Command "taskkill /F /IM mmc.exe"

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

$appSettingsPath = Join-Path -Path $outPut -ChildPath "appsettings.json"

$jsonAppSettings = (Get-Content -Raw $appSettingsPath | ConvertFrom-Json )

write-host $jsonAppSettings

function ReinstallService ($localServiceName, $binaryPath, $description, $login, $password, $startUpType, $displayName)
{
    Write-Host "Trying to create service: $localServiceName - $binaryPath"

    #Check Parameters
    if ((Test-Path $binaryPath)-eq $false)
    {
        Write-Host "BinaryPath to service not found: $binaryPath"
        Write-Host "Service was NOT installed."
        return
    }

    if (("Automatic", "Manual", "Disabled") -notcontains $startUpType)
    {
        Write-Host "Value for startUpType parameter should be (Automatic or Manual or Disabled) and it was $startUpType"
        Write-Host "Service was NOT installed."
        return
    }

    # Verify if the service already exists, and if yes remove it first
    if (Get-Service $localServiceName -ErrorAction SilentlyContinue)
    {
        # using WMI to remove Windows service because PowerShell does not have CmdLet for this
        $serviceToRemove = Get-WmiObject -Class Win32_Service -Filter "name='$localServiceName'"

        $serviceToRemove.delete()
        Write-Host "Service removed: $localServiceName"
    }

    # if password is empty, create a dummy one to allow have credentias for system accounts: 
    #NT AUTHORITY\LOCAL SERVICE
    #NT AUTHORITY\NETWORK SERVICE
    if ($password -eq "") 
    {
        #$secpassword = (new-object System.Security.SecureString)
        # Bug detected by @GaTechThomas
        $secpasswd = (new-object System.Security.SecureString)
    }
    else
    {
        $secpasswd = ConvertTo-SecureString $password -AsPlainText -Force
    }
    $mycreds = New-Object System.Management.Automation.PSCredential ($login, $secpasswd)

    # Creating Windows Service using all provided parameters
    Write-Host "Installing service: $localServiceName"
    New-Service -name $localServiceName -binaryPathName $binaryPath -Description $description -displayName $displayName -startupType $startUpType -credential $mycreds

    Write-Host "Installation completed: $localServiceName"

    # Trying to start new service
    Write-Host "Trying to start new service: $localServiceName"
    $serviceToStart = Get-WmiObject -Class Win32_Service -Filter "name='$localServiceName'"
    $serviceToStart.startservice()
    Write-Host "Service started: $localServiceName"

    #SmokeTest
    Write-Host "Waiting 5 seconds to give time service to start..."
    Start-Sleep -s 5
    $SmokeTestService = Get-Service -Name $localServiceName
    if ($SmokeTestService.Status -ne "Running")
    {
        Write-Host "Smoke test: FAILED. (SERVICE FAILED TO START)"
        Throw "Smoke test: FAILED. (SERVICE FAILED TO START)"
    }
    else
    {
        Write-Host "Smoke test: OK."
    }
  # https://stackoverflow.com/questions/14708825/how-to-create-a-windows-service-in-powershell-for-network-service-account
}

ReinstallService $serviceName $exePath "Windows service" "NT AUTHORITY\NETWORK SERVICE" "" "Automatic" "Starsky Web App"