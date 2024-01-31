#Requires -RunAsAdministrator

param(
    [Parameter(Mandatory=$false)][switch]$help,
    [Parameter(Mandatory=$false)][string]$netMoniker='net8.0',
    [Parameter(Mandatory=$false)][string]$solutionName='starsky.sln'
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
    write-host "help"
    exit 0
}

$sonarCache = Join-Path -Path $env:USERPROFILE -ChildPath ".sonar"

if ((Test-Path -Path $sonarCache) -eq $True) {

    Remove-Item -Recurse -Force $sonarCache
    write-host     $sonarCache " in User Folder removed"
} 
else {
 write-host     $sonarCache " in User Folder does not exists"
}

$scriptRootPath = (Split-Path $MyInvocation.MyCommand.Path -Parent)
$rootPath = (Split-Path $scriptRootPath -Parent)
$starskyWebProject = Join-Path -Path (Split-Path $scriptRootPath -Parent) -ChildPath "starsky"
$repoRoot = (Split-Path $rootPath -Parent)


write-host "search for bin folder in rootPath: " $rootPath
write-host "but ignore starskyWebProject" $starskyWebProject

$binFolders = Get-ChildItem $rootPath -Directory -recurse -Filter "bin" -ErrorAction SilentlyContinue -Force | 
    Where-Object {$_.PSIsContainer -eq $true -and $_.Name -match "bin" -and $_.FullName -notmatch "node_modules"};

foreach ($binFolder in $binFolders) {
    if ($binFolder.FullName -ne (Join-Path -Path $starskyWebProject -ChildPath "bin" )) {
        write-host "next delete: " $binFolder.FullName
        Remove-Item $binFolder.FullName -Recurse
    }
    else {
        write-host "skip intended: "$binFolder.FullName
    }
}

write-host "next: delete coverage files"

$coverageFile = Join-Path -Path (Join-Path -Path $rootPath -ChildPath "starskytest") -ChildPath "coverage-merge-cobertura.xml"

if ((Test-Path -Path $coverageFile) -eq $True) {
    write-host "> delete " $coverageFile
    Remove-Item $coverageFile
}

$coverageFile2 = Join-Path -Path (Join-Path -Path $rootPath -ChildPath "starskytest") -ChildPath "coverage-merge-sonarqube.xml"

if ((Test-Path -Path $coverageFile2) -eq $True) {
    write-host "> delete " $coverageFile2
    Remove-Item $coverageFile2
}
else {
    write-host "> skip " $coverageFile2
}

$coverageFile3 = Join-Path -Path (Join-Path -Path $rootPath -ChildPath "starskytest") -ChildPath "jest-coverage.cobertura.xml"

if ((Test-Path -Path $coverageFile3) -eq $True) {
    write-host "> delete " $coverageFile3
    Remove-Item $coverageFile3
}
else {
    write-host "> skip " $coverageFile3
}

$coverageFile4 = Join-Path -Path (Join-Path -Path $rootPath -ChildPath "starskytest") -ChildPath "netcore-coverage.opencover.xml"

if ((Test-Path -Path $coverageFile4) -eq $True) {
    write-host "> delete " $coverageFile4
    Remove-Item $coverageFile4
}
else {
    write-host "> skip " $coverageFile4
}

write-host "next: delete dependency files"

$releaseDepsFolder =  Join-Path -Path (Join-Path -Path ( Join-Path -Path (Join-Path -Path $starskyWebProject -ChildPath "bin") -ChildPath "Release" )  -ChildPath $netMoniker ) -ChildPath "dependencies"

if ((Test-Path -Path $releaseDepsFolder) -eq $True) {
    write-host "> delete " $releaseDepsFolder
    Remove-Item $releaseDepsFolder
}
else {
    write-host "> skip " $releaseDepsFolder
}

$debugDepsFolder =  Join-Path -Path (Join-Path -Path ( Join-Path -Path (Join-Path -Path $starskyWebProject -ChildPath "bin") -ChildPath "Debug" )  -ChildPath $netMoniker ) -ChildPath "dependencies"

if ((Test-Path -Path $debugDepsFolder) -eq $True) {
    write-host "> delete " $debugDepsFolder
    Remove-Item $debugDepsFolder
}
else {
    write-host "> skip " $debugDepsFolder
}

$tempDebugFolder =  Join-Path -Path (Join-Path -Path ( Join-Path -Path (Join-Path -Path $starskyWebProject -ChildPath "bin") -ChildPath "Debug" )  -ChildPath $netMoniker ) -ChildPath "temp"

if ((Test-Path -Path $tempDebugFolder) -eq $True) {
    write-host "> delete " $tempDebugFolder
    Remove-Item $tempDebugFolder
}
else {
    write-host "> skip " $tempDebugFolder
}

$sonarQubeFolder = Join-Path -Path (Split-Path $scriptRootPath -Parent) -ChildPath ".sonarqube"

if ((Test-Path -Path $sonarQubeFolder) -eq $True) {
    write-host "> delete " $sonarQubeFolder
    Remove-Item $sonarQubeFolder
}
else {
    write-host "> skip " $sonarQubeFolder
}

write-host "next: clean npm"

if ($null -ne (Get-Command "npm" -ErrorAction SilentlyContinue)) {

    write-host "next: npm cache clean --force"
    Invoke-Expression -Command "npm cache clean --force"
}

write-host "search for obj folder in rootPath: " $rootPath

$objFolders = Get-ChildItem $rootPath -Directory -recurse -Filter "obj" -ErrorAction SilentlyContinue -Force | 
    Where-Object {$_.PSIsContainer -eq $true -and $_.Name -match "obj" -and $_.FullName -notmatch "node_modules"};

foreach ($objFolder in $objFolders) {
    write-host "next delete: " $objFolder.FullName
    Remove-Item $objFolder.FullName -Recurse
}

write-host "next: clean dotnet"
if ($null -ne (Get-Command "dotnet" -ErrorAction SilentlyContinue)) {
    write-host "next: dotnet nuget locals all --clear"
    Invoke-Expression -Command "dotnet nuget locals all --clear" -ErrorAction SilentlyContinue



    Push-Location $rootPath
        $cleanCommand = "dotnet clean "+$solutionName
        write-host "next: "$cleanCommand
        Invoke-Expression -Command $cleanCommand -ErrorAction SilentlyContinue

        $cleanCommand2 = "dotnet clean "+$solutionName + " --configuration Release"
        write-host "next: "$cleanCommand2
        Invoke-Expression -Command $cleanCommand2 -ErrorAction SilentlyContinue
    Pop-Location
}

# MacOS: ~/Library/Caches/Cypress
# Linux: ~/.cache/Cypress
# Windows: /AppData/Local/Cypress/Cache
#  C:\Users\myname\AppData\Local\Cypress\Cache\12.3.0\Cypress


# This assumes windows

$cypressCache = Join-Path -Path (Join-Path -Path $env:LOCALAPPDATA -ChildPath "Cypress") -ChildPath "Cache"

$currentVersionPackage = Join-Path -Path ( Join-Path -Path (Join-Path -Path $repoRoot -ChildPath "starsky-tools") -ChildPath "end2end" ) -ChildPath "package.json"

if (((Test-Path -Path $currentVersionPackage) -eq $True) -and ((Test-Path -Path $cypressCache) -eq $True) ) {

    $jsonCurrentVersion = (Get-Content -Raw $currentVersionPackage | ConvertFrom-Json )
    $version = ($jsonCurrentVersion.devDependencies.cypress).replace('^','')
    $versionPath = Join-Path -Path $cypressCache -ChildPath $version

    foreach ($otherVersion in (Get-ChildItem $cypressCache)) {
        if ($otherVersion.FullName -ne $versionPath) {
            write-host "next delete: " $otherVersion.FullName
            Remove-Item $otherVersion.FullName -Recurse    
        }
        else {
            write-host "skip: " $otherVersion.FullName
        }
    }
}

$electronCache = Join-Path -Path (Join-Path -Path $env:LOCALAPPDATA -ChildPath "electron") -ChildPath "Cache"

if ((Test-Path -Path $electronCache) -eq $True) {
    write-host "> delete " $electronCache
    Remove-Item $electronCache -Recurse
}
else {
    write-host "> skip " $electronCache
}

write-host "next: clean pnpm"

if ($null -ne (Get-Command "pnpm" -ErrorAction SilentlyContinue)) {

    write-host "next: clean pnpm [not used in project]"
    Invoke-Expression -Command "pnpm store prune"
}

write-host "next: clean docker"

if ($null -eq (Get-Command "docker" -ErrorAction SilentlyContinue)) {

    write-host "next: docker does not exists"
    exit 0
}

write-host "Docker exists now checking if its up?!"

Invoke-Expression -Command "docker stats --no-stream" -ErrorAction SilentlyContinue
if ($LASTEXITCODE -ne 0) {
    write-host "docker should be started"
    $dockerDesktopPathWindows = "C:\Program Files\Docker\Docker\Docker Desktop.exe"

    if ((Test-Path -Path $dockerDesktopPathWindows) -eq $True) {
       # Start-Process -FilePath $dockerDesktopPathWindows -Verb RunAs
    }
    else {
        write-host "> could not find " $dockerDesktopPathWindows
    }
    $status = 1;

    while ($status -ne 0) {
        Start-Sleep -Seconds 2 

        Try {
            Invoke-Expression -Command "docker stats --no-stream" -ErrorAction Stop
            $status=$LASTEXITCODE
        }
        catch {
            Write-Host "."
        }
    }
}
write-host "Docker is up now"

write-host "next clean: "

Invoke-Expression -Command "docker builder prune --filter 'until=8h' -f"
Invoke-Expression -Command "docker image prune --filter 'until=8h' -f"
Invoke-Expression -Command "docker container prune --filter 'until=8h' -f"

write-host "end"