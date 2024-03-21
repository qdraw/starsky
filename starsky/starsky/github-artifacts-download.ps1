#!/usr/bin/env powershell

# For insiders only - requires token
# Please use: 
# ./pm2-install-latest-release.sh 
# for public builds

# Script goal:
# Download binaries with zip folder from Github Actions
# Get pm2-new-instance.sh ready to run (but not run)

# source: /opt/starsky/starsky/github-artifacts-download.ps1

#Requires -RunAsAdministrator

param(
    [Parameter(Mandatory=$false)][switch]$help,
    [Parameter(Mandatory=$false)][string]$workFlowId="desktop-release-on-tag-net-electron.yml",
    [Parameter(Mandatory=$false)][string]$runTime='',
    [Parameter(Mandatory=$false)][string]$outPut='',
    [Parameter(Mandatory=$false)][string]$token=$env:STARSKY_GITHUB_PAT
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


if($runTime -eq '') {

    If($IsWindows -eq $true) {
        $runTime = "win-x64"
    }
    ElseIf (($IsLinux -eq $true) -or ($IsMacOs -eq $true)) {
        $uNameM = Invoke-Expression -Command "uname -m"

        If(($uNameM -eq 'x86_64') -and ($IsLinux -eq $true)) {
            $runTime = "linux-x64"
        }
        If(($uNameM -eq 'x86_64') -and ($IsMacOs -eq $true)) {
            $runTime = "osx-x64"
        }
        If(($uNameM -eq 'aarch64')) {
            $runTime = "linux-arm64"
        }
        If(($uNameM -eq 'armv7l')) {
            $runTime = "linux-arm"
        }
        If(($uNameM -eq 'arm64') -and ($IsMacOs -eq $true)) {
            $runTime = "osx-arm64" # got gatekeeper errors
        }
    }
}

write-host "runtime:"$runTime

# rename
$runTimeVersion=$runTime

$versionZipArray = @()
If($runTimeVersion.contains("desktop") -eq $false) {
    $versionZipArray = @("starsky-" + $runTime + ".zip")
}
else {
    # for desktop
    $versionZipArray = @($runTime + ".zip", $runTime + ".dmg", $runTime + ".exe")
}

$versionName = $versionZipArray[0].replace(".zip","")

if ((Test-Path -Path $outPut) -eq $false) {
    write-host "-output path doesn't exist." $exePath
    write-host "end script due fail"
    exit 1
} 

$startupCsPath = Join-Path -Path $outPut -ChildPath "Startup.cs"

if ((Test-Path -Path $startupCsPath) -eq $true) {
    write-host "FAIL: You should not run this folder from the source folder"
    write-host "copy this file to the location to run it from"
    write-host "end script due failure"
    exit 1
} 

if ([string]::IsNullOrWhitespace($token)){
    write-host "enter pat as --token and rerun"
    exit 1
} 

# Define the API URL
function Get-Workflow-Url {
    param(
        [string]$WorkflowId,
        [string]$Status,
        [string]$Branch
    )

    $ActionsWorkflowUrl = "https://api.github.com/repos/qdraw/starsky/actions/workflows/${WorkflowId}/runs?status=${Status}&per_page=1&exclude_pull_requests=true"

    if (-not [string]::IsNullOrEmpty($Branch)) {
        $ActionsWorkflowUrl += "&branch=${Branch}"
    }

    return $ActionsWorkflowUrl
}

$ActionsWorkflowUrlCompleted = Get-Workflow-Url -WorkflowId $WorkflowId -Status "completed" -Branch ""
$ActionsWorkflowUrlInProgress = Get-Workflow-Url -WorkflowId $WorkflowId -Status "in_progress" -Branch ""

# Define the API status code variable
$ApiGatewayStatusCode = (Invoke-WebRequest -Method GET -Uri $ActionsWorkflowUrlCompleted -Headers @{Authorization = "Token " + $token}).StatusCode

# Check if the API status code is 401 or 404
if ($ApiGatewayStatusCode -eq 401 -or $ApiGatewayStatusCode -eq 404) {
  Write-Output "Unauthorized or not found."
  exit 1
}

# check if is in progress
function Wait-For-WorkflowCompletion {
    param(
        [string]$StarskyGitHubPAT,
        [string]$ActionsWorkflowUrlInProgress
    )

    $MaxRetries = 5
    $RetryCount = 0

    while ($true) {
        $ResultActionsInProgressWorkflow = Invoke-RestMethod -Uri $ActionsWorkflowUrlInProgress -Headers @{
            Authorization = "Token " + $StarskyGitHubPAT
        }

        $totalCount = $ResultActionsInProgressWorkflow.total_count

        if ($totalCount -ne 0) {
            $RetryCountDisplay = $RetryCount + 1
            Write-Output "Workflow runs in progress. Retrying $RetryCountDisplay/$MaxRetries in 10 seconds..."
            Start-Sleep -Seconds 10
        }
        else {
            break
        }

        $RetryCount++
        if ($RetryCount -eq $MaxRetries) {
            Write-Output "Skip retry to get the in-progress function, continue with the latest finished build"
            break
        }
    }
}

Wait-For-WorkflowCompletion -StarskyGitHubPAT $token -ActionsWorkflowURLInProgress $ActionsWorkflowUrlInProgress


# Get the latest workflow run information
$LatestRun = (Invoke-WebRequest -Method GET -Uri $ActionsWorkflowUrlCompleted -Headers @{Authorization = "Token " + $token} | ConvertFrom-Json).workflow_runs[0]


# Get the latest workflow run ID
$LatestRunId = $LatestRun.id


# Get the artifacts URL
$ArtifactsUrl = $LatestRun.artifacts_url

$artifactsUrlResult = (Invoke-WebRequest -Method GET -Uri $LatestRun.artifacts_url -Headers @{Authorization = "Token " + $token} | ConvertFrom-Json)

$artifactsDownloadUrl = ""
ForEach ($artifact in $artifactsUrlResult.artifacts) {

    if($artifact.name -eq $runTimeVersion) {
        write-host  "artifact" $artifact.name
        $artifactsDownloadUrl = $artifact.archive_download_url
    }
}

if ([string]::IsNullOrWhitespace($artifactsDownloadUrl)){
    write-host "[FAIL] artifactsDownloadUrl is null or empty"
    exit 1
} 


$outPutZipTempPath= Join-Path -Path $outPut -ChildPath "${versionName}_tmp.zip"

write-host "Next: download output file:" $outPutZipTempPath
Invoke-WebRequest  -Method GET -Uri $artifactsDownloadUrl -Headers @{Authorization = "Token " + $token} -OutFile $outPutZipTempPath

# remove already downloaded outputs
ForEach ($singleVersionZip in $versionZipArray) {
    $singleOutPutZipPath = Join-Path -Path $outPut -ChildPath "${singleVersionZip}"
    if ((Test-Path -Path $singleOutPutZipPath) -eq $true) {
        write-host "remove existing file." $singleOutPutZipPath
        Remove-Item -Path $singleOutPutZipPath -Force
    } 
}


Expand-Archive $outPutZipTempPath -DestinationPath $outPut

Remove-Item $outPutZipTempPath

$outPutZipPath = Join-Path -Path $outPut -ChildPath "${versionName}.zip"

if ((Test-Path -Path $outPutZipPath) -eq $false) {
    write-host "FAIL: output file doesn't exist." $outPutZipPath
    exit 1
}


Write-Output "Script completed."