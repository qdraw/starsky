<#
.SYNOPSIS
    Builds the Starsky Desktop WPF app and packages it as a Velopack installer.

.DESCRIPTION
    Steps:
      1. (Optional) Build the Starsky backend for win-x64 using the NUKE build system.
         Skip with -SkipBackendBuild if starsky\win-x64\ already exists.
      2. dotnet publish the WPF app (also copies runtime-starsky-win-x64 into publish dir).
      3. Install the Velopack CLI (vpk) if not already present.
      4. vpk pack to produce the installer and update feed.
      5. Rename the setup exe to starsky-win-x64-desktop.exe.

.PARAMETER Version
    Version string to embed (e.g. "0.8.2"). Defaults to the <Version> in the csproj.

.PARAMETER SkipBackendBuild
    Skip step 1. Use this when starsky\win-x64\ is already populated.

.PARAMETER OutputDir
    Directory for Velopack output. Defaults to <repo root>\velopack-releases.

.EXAMPLE
    # Full build from scratch:
    .\build-velopack.ps1

    # Skip the slow backend build if you already have starsky\win-x64\:
    .\build-velopack.ps1 -SkipBackendBuild

    # Override version:
    .\build-velopack.ps1 -Version 0.8.2 -SkipBackendBuild
#>
[CmdletBinding()]
param(
    [string] $Version,
    [switch] $SkipBackendBuild,
    [string] $OutputDir
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

# ---------------------------------------------------------------------------
# Resolve paths
# ---------------------------------------------------------------------------
$ScriptDir  = $PSScriptRoot                          # windows/
$RepoRoot   = Split-Path $ScriptDir -Parent          # repo root (contains build.ps1)
$PublishDir = Join-Path $RepoRoot 'windows' 'dist'
if (-not $OutputDir) { $OutputDir = Join-Path $RepoRoot 'windows' 'dist-prod' }

$CsprojPath    = Join-Path $ScriptDir 'Starsky.Desktop.csproj'
$BackendOutDir = Join-Path $RepoRoot 'starsky\win-x64'
$IconPath      = Join-Path $ScriptDir 'Resources\starsky.ico'

# ---------------------------------------------------------------------------
# Resolve version
# ---------------------------------------------------------------------------
if (-not $Version) {
    [xml]$csproj = Get-Content $CsprojPath
    $Version = ($csproj.Project.PropertyGroup |
        Where-Object { $_.Version } |
        Select-Object -First 1).Version
    if (-not $Version) {
        Write-Error "Could not read <Version> from $CsprojPath. Pass -Version explicitly."
    }
}
Write-Host "Building version: $Version" -ForegroundColor Cyan

# ---------------------------------------------------------------------------
# Step 1 — Backend build
# ---------------------------------------------------------------------------
if ($SkipBackendBuild) {
    Write-Host "`n[Step 1] Skipping backend build (-SkipBackendBuild)" -ForegroundColor Yellow
    if (-not (Test-Path $BackendOutDir)) {
        Write-Error "Backend dir '$BackendOutDir' not found. Build it first or remove -SkipBackendBuild."
    }
} else {
    Write-Host "`n[Step 1] Building Starsky backend for win-x64..." -ForegroundColor Cyan
    $BackendScript = Join-Path $RepoRoot 'starsky\build.ps1'
    if (-not (Test-Path $BackendScript)) {
        Write-Error "Backend build script not found at '$BackendScript'."
    }
    Push-Location (Join-Path $RepoRoot 'starsky')
    try {
        & $BackendScript --runtime win-x64 --no-unit-test --ready-to-run
        if ($LASTEXITCODE -ne 0) { throw "Backend build failed (exit $LASTEXITCODE)." }
    } finally {
        Pop-Location
    }
}

# ---------------------------------------------------------------------------
# Step 2 — Publish WPF app
# ---------------------------------------------------------------------------
Write-Host "`n[Step 2] Publishing WPF app to '$PublishDir'..." -ForegroundColor Cyan

if (Test-Path $PublishDir) {
    Remove-Item $PublishDir -Recurse -Force
}

dotnet publish $CsprojPath `
    -c Release `
    -r win-x64 `
    --self-contained true `
    -o $PublishDir

if ($LASTEXITCODE -ne 0) { throw "dotnet publish failed (exit $LASTEXITCODE)." }

$RuntimeInPublish = Join-Path $PublishDir 'runtime-starsky-win-x64'
if (-not (Test-Path $RuntimeInPublish)) {
    Write-Error "runtime-starsky-win-x64 is missing from publish output. Check the CopyStarskyRuntimeToPublish MSBuild target."
}
Write-Host "  runtime-starsky-win-x64: OK" -ForegroundColor Green

# ---------------------------------------------------------------------------
# Step 3 — Ensure Velopack CLI is installed
# ---------------------------------------------------------------------------
Write-Host "`n[Step 3] Checking Velopack CLI (vpk)..." -ForegroundColor Cyan

$vpkCmd = Get-Command vpk -ErrorAction SilentlyContinue
if (-not $vpkCmd) {
    Write-Host "  vpk not found — installing..." -ForegroundColor Yellow
    dotnet tool install -g vpk --version 1.2.0
    if ($LASTEXITCODE -ne 0) { throw "vpk install failed (exit $LASTEXITCODE)." }
} else {
    Write-Host "  vpk found: $($vpkCmd.Source)" -ForegroundColor Green
}

# ---------------------------------------------------------------------------
# Step 4 — Pack with Velopack
# ---------------------------------------------------------------------------
Write-Host "`n[Step 4] Packing with Velopack to '$OutputDir'..." -ForegroundColor Cyan

if (Test-Path $OutputDir) {
    Remove-Item $OutputDir -Recurse -Force
}

vpk pack `
    --packId      Starsky.Desktop `
    --packVersion $Version `
    --packDir     $PublishDir `
    --mainExe     Starsky.Desktop.exe `
    --outputDir   $OutputDir `
    --packTitle   "Starsky Desktop" `
    --icon        $IconPath

if ($LASTEXITCODE -ne 0) { throw "vpk pack failed (exit $LASTEXITCODE)." }

# ---------------------------------------------------------------------------
# Step 5 — Rename setup exe
# ---------------------------------------------------------------------------
Write-Host "`n[Step 5] Renaming installer..." -ForegroundColor Cyan

$SetupExe = Get-Item (Join-Path $OutputDir '*-Setup.exe') -ErrorAction SilentlyContinue
if (-not $SetupExe) {
    Write-Warning "No *-Setup.exe found in '$OutputDir' — skipping rename."
} else {
    $FinalName = Join-Path $OutputDir 'starsky-win-x64-desktop.exe'
    Move-Item $SetupExe.FullName $FinalName -Force
    Write-Host "  Installer: $FinalName" -ForegroundColor Green
}

# ---------------------------------------------------------------------------
# Summary
# ---------------------------------------------------------------------------
Write-Host "`nBuild complete." -ForegroundColor Green
Write-Host "  Installer : $(Join-Path $OutputDir 'starsky-win-x64-desktop.exe')"
Write-Host "  Feed      : $OutputDir\*.nupkg + RELEASES*"
