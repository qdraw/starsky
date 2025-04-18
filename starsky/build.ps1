[CmdletBinding()]
Param(
    [Parameter(Position=0,Mandatory=$false,ValueFromRemainingArguments=$true)]
    [string[]]$BuildArguments
)

Write-Output "PowerShell $($PSVersionTable.PSEdition) version $($PSVersionTable.PSVersion)"

Write-Output "BuildArguments: $BuildArguments"

Set-StrictMode -Version 2.0; $ErrorActionPreference = "Stop"; $ConfirmPreference = "None"; trap { Write-Error $_ -ErrorAction Continue; exit 1 }
$PSScriptRoot = Split-Path $MyInvocation.MyCommand.Path -Parent

###########################################################################
# CONFIGURATION
###########################################################################

$BuildProjectFile = "$PSScriptRoot\build\_build.csproj"
$TempDirectory = "$PSScriptRoot\\.nuke\temp"

$DotNetGlobalFile = "$PSScriptRoot\\global.json"
$DotNetInstallUrl = "https://dot.net/v1/dotnet-install.ps1"
$DotNetChannel = "Current"

$env:DOTNET_SKIP_FIRST_TIME_EXPERIENCE = 1
$env:DOTNET_CLI_TELEMETRY_OPTOUT = 1
$env:DOTNET_MULTILEVEL_LOOKUP = 0
$env:DOTNET_NOLOGO = 1
$env:NUKE_TELEMETRY_OPTOUT = 1


$nvmRcFile = "$PSScriptRoot\\.nvmrc"


###########################################################################
# EXECUTION
###########################################################################

function ExecSafe([scriptblock] $cmd) {
    & $cmd
    if ($LASTEXITCODE) { exit $LASTEXITCODE }
}

function Test-Administrator  
{  
    [OutputType([bool])]
    param()
    process {
        [Security.Principal.WindowsPrincipal]$user = [Security.Principal.WindowsIdentity]::GetCurrent();
        return $user.IsInRole([Security.Principal.WindowsBuiltinRole]::Administrator);
    }
}

function Test-Switch-Nvm-Path  
{ 
    if (Test-Path -Path $nvmRcFile) {
        $nvmVersion = Get-Content $nvmRcFile

        $nvmCurrentCommand = "nvm current";
        if ($nvmVersion -ne (Invoke-Expression -Command $nvmCurrentCommand)) {
            write-host "next switch to "$nvmVersion
            write-host "There might be an admin window"

            $nvmInstallVersionCommand = "nvm install "+$nvmVersion
            Invoke-Expression -Command $nvmInstallVersionCommand
            $nvmSwitchVersionCommand = "nvm use "+$nvmVersion
            Invoke-Expression -Command $nvmSwitchVersionCommand
        }
        else {
            write-host "already the right version " $nvmVersion
        }
    }   
}

function Restart-Command-Not-In-Path  
{ 
    param (
        [string]$command
    )
    
    if ($null -eq (Get-Command $command -ErrorAction SilentlyContinue)) {
        write-host ""
        write-host "Please restart the current powershell window and run the ./build.ps1 again"
        write-host "Set-Location -Path \"$PSScriptRoot\"; .\build.ps1"
        write-host ""
        exit 1
    }
}

function Add-NuGetSource {
    param (
        [string]$dotnetPath,
        [string]$sourceUrl,
        [string]$sourceName
    )

    # Run the command to list NuGet sources and check if the target source is already added
    $result = & $dotnetPath nuget list source | Select-String -Pattern $sourceUrl

    # If the target source is not found, add it
    if (-not $result) {
        Write-Host "Adding NuGet source '$sourceName' with URL '$sourceUrl'..."
        & $dotnetPath nuget add source --name $sourceName $sourceUrl
        Write-Host "NuGet source added successfully."
    } else {
        Write-Host "NuGet source '$sourceName' already exists."
    }
}

write-host "ci: " $env:CI "tfbuild: "  $env:TF_BUILD  " install check: " $env:FORCE_INSTALL_CHECK

#$env:CI = 'true'
#$env:FORCE_INSTALL_CHECK = 'true'

if (( ($env:CI -ne $true) -and ($env:TF_BUILD -ne $true)) -or ($env:FORCE_INSTALL_CHECK -eq $true)) {

    $jsonDotNetGlobalFile = Get-Content $DotNetGlobalFile | Out-String | ConvertFrom-Json
    $shouldBeNetVersion = $jsonDotNetGlobalFile.sdk.version

    write-host shouldBeNetVersion: $shouldBeNetVersion

    # check if dotnet is installed
    if ($null -ne (Get-Command "dotnet" -ErrorAction SilentlyContinue) -and `
         $(dotnet --version) -and $LASTEXITCODE -eq 0) {

        write-host "right version is installed"
    }
    else {
        write-host "wrong version is installed"
        
        if ($null -ne (Get-Command "choco" -ErrorAction SilentlyContinue)) {
            write-host "next: install via choco"
            
            $firstCharOfVersion = $shouldBeNetVersion.SubString(0,1)
            $showCommand = 'choco install dotnet-' + $firstCharOfVersion + '.0-sdk --version ' + $shouldBeNetVersion + ' -y --no-progress' 
            # to uninstall choco uninstall dotnet-6.0-sdk
            write-host "next run: " $showCommand
            
            $resultInstall = Invoke-Expression -Command $showCommand -ErrorAction SilentlyContinue
            if ($LASTEXITCODE -eq 0) {
                write-host "version found" 
            }
            else {
                write-host "version not found so skip"
                Write-host $resultInstall
            }

            Restart-Command-Not-In-Path -Command "dotnet"
        }

        if (($null -ne (Get-Command "winget" -ErrorAction SilentlyContinue)) -and (-not (Get-Command "choco" -ErrorAction SilentlyContinue))) {
        
            write-host "next: install via winget"

            # just to get by those messages
            Invoke-Expression "winget search dotnet --accept-source-agreements" -ErrorAction SilentlyContinue | Out-Null

            $firstCharOfVersion = $shouldBeNetVersion.SubString(0,1)
            $showCommand = 'winget show dotnet-sdk-' + $firstCharOfVersion + ' -v ' + $shouldBeNetVersion + ' --disable-interactivity' 
            write-host "next run: " $showCommand
            $resultInstall = Invoke-Expression -Command $showCommand -ErrorAction SilentlyContinue
            if ($LASTEXITCODE -eq 0) {
                write-host "version found - next install" 
                write-host "you will be asked for an admin"
                $installCommand = 'winget install dotnet-sdk-' + $firstCharOfVersion + ' -v ' + $shouldBeNetVersion + ' --disable-interactivity --accept-source-agreements --accept-package-agreements' 
                Invoke-Expression -Command $installCommand 
            }
            else {
                write-host "version not found so skip"
                Write-host $resultInstall
            }
        }
    }

    write-host "next: check right version of nodejs"

    if (($null -ne (Get-Command "choco" -ErrorAction SilentlyContinue)) -and ($null -eq (Get-Command "nvm" -ErrorAction SilentlyContinue)) ) {
        # https://chocolatey.org/install
        write-host "choco exists"

        if(-not (Test-Administrator))
        {
            Write-Error "hit Winget - This script must be executed as Administrator.";
            exit 1;
        }

        write-host "next install node version manager - choco exists"

        Invoke-Expression -Command "choco install nvm -y"

        Restart-Command-Not-In-Path -Command "nvm"
         
        Test-Switch-Nvm-Path

    }
    
    if (($null -ne (Get-Command "winget" -ErrorAction SilentlyContinue)) -and (-not (Get-Command "choco" -ErrorAction SilentlyContinue))) {

        if ($null -eq (Get-Command "nvm" -ErrorAction SilentlyContinue)) {

            if(-not (Test-Administrator))
            {
                Write-Error "hit Winget - This script must be executed as Administrator.";
                exit 1;
            }

            write-host "update package list winget"
            Invoke-Expression -Command "winget source update --verbose-logs"

            write-host "next install node version manager - winget exists"
            write-host "you will asked for password"

            try {
               Invoke-Expression -Command "winget install -e --id CoreyButler.NVMforWindows --disable-interactivity"
            } catch {
                write-host $_
                write-host "try other way"
                Invoke-Expression -Command "winget install -e --id CoreyButler.NVMforWindows"
            }

            write-host "install of nvm done"
        }

        Restart-Command-Not-In-Path -Command "nvm"

        Test-Switch-Nvm-Path
        
    }

}

# If dotnet CLI is installed globally and it matches requested version, use for execution
if ($null -ne (Get-Command "dotnet" -ErrorAction SilentlyContinue) -and `
     $(dotnet --version) -and $LASTEXITCODE -eq 0) {
    $env:DOTNET_EXE = (Get-Command "dotnet").Path
}
else {
    # Download install script
    write-host "download dotnet ps1 CI script"
    $DotNetInstallFile = "$TempDirectory\dotnet-install.ps1"
    New-Item -ItemType Directory -Path $TempDirectory -Force | Out-Null
    [Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12
    (New-Object System.Net.WebClient).DownloadFile($DotNetInstallUrl, $DotNetInstallFile)

    # If global.json exists, load expected version
    if (Test-Path $DotNetGlobalFile) {
        $DotNetGlobal = $(Get-Content $DotNetGlobalFile | Out-String | ConvertFrom-Json)
        if ($DotNetGlobal.PSObject.Properties["sdk"] -and $DotNetGlobal.sdk.PSObject.Properties["version"]) {
            $DotNetVersion = $DotNetGlobal.sdk.version
        }
    }
    
    # Install by channel or version
    $DotNetDirectory = "$TempDirectory\dotnet-win"

    Add-NuGetSource -dotnetPath "$DotNetDirectory\dotnet.exe" -sourceUrl "https://api.nuget.org/v3/index.json" -sourceName "nuget.org"

    if (!(Test-Path variable:DotNetVersion)) {
        ExecSafe { & powershell $DotNetInstallFile -InstallDir $DotNetDirectory -Channel $DotNetChannel -NoPath }
    } else {
        ExecSafe { & powershell $DotNetInstallFile -InstallDir $DotNetDirectory -Version $DotNetVersion -NoPath }
    }
    $env:DOTNET_EXE = "$DotNetDirectory\dotnet.exe"
}

Write-Output "Microsoft (R) .NET SDK version $(& $env:DOTNET_EXE --version)"

ExecSafe { & $env:DOTNET_EXE build $BuildProjectFile /nodeReuse:false /p:UseSharedCompilation=false -nologo -clp:NoSummary --verbosity quiet }
ExecSafe { & $env:DOTNET_EXE run --project $BuildProjectFile --no-build -- --no-logo $BuildArguments }
