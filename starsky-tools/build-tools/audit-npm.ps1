<#
.SYNOPSIS
Script to generate information about vulnerable js libraries
in an automation-ready form based on npm-audit.
.DESCRIPTION
This script simply runs npm-audit and parses the output.
Allows to fail on threshold severity, exclude devDependencies
and save result as json.
.PARAMETER targetFolder
Folder with package.json file.
.PARAMETER failOnVulnLevel
Specifies threshold of vulnerability severity level that makes script to exit with code 1.
Allowed values: none, low, moderate, high.
.PARAMETER includeDevDeps
Whether to report vulnerabilities for devDependencies.
Allowed values: $true, $false
.PARAMETER outputFileVuln
Output file for resulting json object. When empty, result are printed to console.
.PARAMETER silent
Supress output.
Allowed values: $true, $false
.EXAMPLE
C:\PS> audit.ps1 -targetFolder "myFolder" -failOnVulnLevel moderate -outputFileVuln result.json
#>
param (
    [ValidateSet("all", "check-licenses", "find-vulnerabilities", "generate-attributions", "outdated")]
    [string]$action = "all",
    [string]$targetFolder = ".",
    [ValidateSet("none", "low", "moderate", "high")]
    [string]$failOnVulnLevel = "none",
    [bool]$includeDevDeps = $false,
    [string]$depth = "0",
    [string]$outputFileVuln = "",
    [bool]$silent = $false,
    [string]$attributionsOutputFile = "ATTRIBUTIONS",
    [string]$pathToNpx = "npx",
    [string]$licenseExclusions
)
if (-Not (Test-Path $targetFolder\package.json)) {
    Write-Host "ERR! Required file package.json doesn't exist in specified location"
    Exit 1
}
if (-Not $pathToNpx) {
    $pathToNpx = "npx"
}
$findingsVuln = @()
$findingsOutdated = @()
$highCount = 0
$moderateCount = 0
$lowCount = 0
$vulnCheckStatus = "OK"
$licenseCheckStatus = "OK"
$outdatedCheckStatus = "OK"
[Console]::OutputEncoding = [System.Text.Encoding]::UTF8
$allowedLicenses = @(
    'MIT',
    'MIT*',
    'Apache-2.0',
    'AFL-2.1',
    'AFLv2.1',
    'BSD',
    'AFL-3.0',
    'ASL-1.1',
    'Boost Software License',
    'BSD-2-Clause',
    'BSD-3-Clause',
    'CC-BY',
    'MS-PL',
    'ISC'
) -join ";"
$prohibitedLicenses = @(
    'GPL 2.0',
    'GPL 2.0+',
    'GPL 3.0',
    'GPL 3.0+',
    'MS-RL',
    'ODbL-1.0',
    'OSL-1.1',
    'OSL-2.0',
    'OSL-2.1',
    'OSL-3.0',
    'RPL-1.1',
    'RPL-1.5',
    'AGPL-1.0',
    'AGPL-3.0',
    'Artistic 2.0'
)
Push-Location
Set-Location $targetFolder
Rename-Item -Path package.json -NewName package.json.original
try {
    $content = (Get-Content -Raw -Path package.json.original | ConvertFrom-Json)
    if (-Not $includeDevDeps) {
        $content.PSObject.Properties.Remove("devDependencies")        
    }
    $content | ConvertTo-Json | Out-File package.json -Encoding ASCII
    if (-Not $silent) { Write-Host "Resolving dependencies" }
    if (($action -eq "find-vulnerabilities") ) {
        # we don't need to actually install for vuln check
        npm install --package-lock-only --no-audit --silent | Out-Null
    }
    else {
        npm install --no-audit --silent | Out-Null
    }
    if (($action -eq "find-vulnerabilities") -Or ($action -eq "all")) {
        if (-Not $silent) { Write-Host "Looking for vulnerabilities in dependencies" }
        $audit = $(npm audit -j --registry="https://registry.npmjs.org/" | ConvertFrom-Json )
        $properties = $audit.advisories.PSObject.Properties
        if ($null -ne $properties -and $properties.Count -gt 0) {
            $properties | ForEach-Object {

                # // https://github.com/facebook/create-react-app/issues/10945
                if($($_.Value.url) -eq "https://npmjs.com/advisories/1693" && $($_.Value.module_name) -eq "postcss") {
                    write-host "skip postcss issue"
                    continue;
                }

                switch ($_.Value.severity) {
                    "high" { $highCount += 1 }
                    "moderate" { $moderateCount += 1 }
                    "low" { $lowCount += 1 }
                }
                $findingsVuln += @{
                    "VulnerabilitySource"   = "$($_.Value.module_name)"
                    "VulnerabilityTitle"    = "$($_.Value.title)"
                    "VulnerabilitySeverity" = "$($_.Value.severity)"
                    "VulnerabilityChains"   = $_.Value.findingsVuln.paths
                    "VulnerableVersions"    = "$($_.Value.vulnerable_versions)"
                    "PatchedVersions"       = "$($_.Value.patched_versions)"
                    "AdvisoryUrl"           = "$($_.Value.url)"
                }
            }
        }
    }
    switch ($failOnVulnLevel) {
        "high" { 
            if ($highCount -gt 0) { 
                Write-Host "##vso[task.logissue type=error]There are vulnerabilities found in one the packages."
                $vulnCheckStatus = "FAILED" 
            }
        }
        "moderate" { if (($moderateCount -gt 0) -Or ($highCount -gt 0)) { $vulnCheckStatus = "FAILED" } }
        "low" { if (($moderateCount -gt 0) -Or ($highCount -gt 0) -Or ($lowCount -gt 0)) { $vulnCheckStatus = "FAILED" } }
        "none" { $vulnCheckStatus = "OK" }
    }
    if (($action -eq "check-licenses") -Or ($action -eq "all")) {
        $tool = "license-checker"
        if ($includeDevDeps) {
            $productionFlag = ""
        }
        else {
            $productionFlag = "--production"
        }
        if (-Not $silent) { Write-Host "Checking licenses" }
        if ($depth) {
            if (-Not $silent) { Write-Host "Cleaning-up non-top level dependencies" }
            $prodDepList = $(npm ls $productionFlag --depth $depth --parseable --silent 2>$null)
            Get-ChildItem -Path .\node_modules\ | Where-Object { $_.FullName -notin $prodDepList } | Remove-Item -Force -Recurse
        }
        if (-Not $silent) { Write-Host "Running $pathToNpx" }
        if ($licenseExclusions) {
            $excludeFlag = "--excludePackages $licenseExclusions"
        }
        $args = "$productionFlag --onlyAllow `"$allowedLicenses`" --summary " + $excludeFlag
        $summary = & "$pathToNpx" $tool $args
        if ($LASTEXITCODE -ne 0) {
            Write-Host "##vso[task.logissue type=error]There are third party packages included that have unwanted license model."
            $licenseCheckStatus = "FAILED"
            # get summary for breakdown
            $summary = & $pathToNpx $tool $productionFlag --summary
        }
    }
    if (($action -eq "outdated") -Or ($action -eq "all")) {
        $outdated = $(npm outdated -j -long true -depth $depth | ConvertFrom-Json )
        $minorCount = 0
        $patchCount = 0
        $majorCount = 0
        $majorMinusOneCount = 0
        $properties = $outdated.PSObject.Properties
        if ($null -ne $properties -and $properties.Count -gt 0) {
            $properties | ForEach-Object {
                if ($null -ne $_.Value.current -and ($null -ne $_.Value.latest) -and ($null -ne $_.Name)) {
                    $currentVersion = $_.Value.current.Split('.')
                    $latestVersion = $_.Value.latest.Split('.')
                    if ($currentVersion[0] -ne $latestVersion[0]) {
                        $majorCount++
                        $currentMajor = 0
                        $latestMajor = 0
                        if ([int32]::TryParse($currentVersion[0] , [ref]$currentMajor) -and [int32]::TryParse($latestVersion[0] , [ref]$latestMajor)) {
                            if (($currentMajor + 1) -lt $latestMajor) {
                                $majorMinusOneCount++
                            }
                        }
                    }
                    elseif ($currentVersion[1] -ne $latestVersion[1]) {
                        $minorCount++
                    }
                    elseif ($currentVersion[2] -ne $latestVersion[2]) {
                        $patchCount++
                    }
                    $findingsOutdated += @{
                        "Name"              = "$($_.Name)"
                        "CurrentVersion"    = "$($_.Value.current)"
                        "LatestNonBreaking" = "$($_.Value.wanted)"
                        "LatestVersion"     = "$($_.Value.latest)"
                        "Url"               = "$($_.Value.homepage)"
                    }
                }
            }
        }
        if ($majorMinusOneCount -gt 0) {
            $outdatedCheckStatus = "FAILED"
            Write-Host "##vso[task.logissue type=error]There are packages that outdated by more than one major version."
        }
        elseif ($majorCount -gt 0) {
            $outdatedCheckStatus = "PARTIAL"
            Write-Host "##vso[task.logissue type=warning]There are packages that are outdated by one major version."
        }
    }
    if (($action -eq "generate-attributions") -Or ($action -eq "all")) {
        if (-Not $silent) { Write-Host "Generating license attributions file" }
        $licenseFiles = Get-ChildItem -Path .\node_modules\*\LICENSE
        $attributionsList += "This product uses third-party components with the following licenses:`r`n`r`n"
        foreach ($license in $licenseFiles) {            
            $attributionsList += "=" * $($license.Directory.Name).Length +
            "`r`n$($license.Directory.Name)`r`n" + "=" * $($license.Directory.Name).Length + "`r`n"
            $attributionsList += $(Get-Content -Path $license -Encoding UTF8 -Raw)
            $attributionsList += "`r`n"
        }

        $attributionsList | Out-File -FilePath $attributionsOutputFile
        if (-Not $silent) {
            if (($action -eq "find-vulnerabilities") -Or ($action -eq "all")) {
                Write-Host "--------------------"
                Write-Host "Vulnerability check: $vulnCheckStatus"
                Write-Host "--------------------"
                Write-Host "Found: $($findingsVuln.Count) vulnerabilities. Low: $lowCount - Moderate: $moderateCount - High: $highCount"
                if ($($findingsVuln.Count) -gt 0) {
                    Write-Host $($findingsVuln | ConvertTo-Json)
                }
            }
            if (($action -eq "outdated") -Or ($action -eq "all")) {
                Write-Host "--------------------"
                Write-Host "Outdated check: $outdatedCheckStatus"
                Write-Host "--------------------"
                Write-Host "Found: $($findingsOutdated.Count) outdated. Major: $majorCount - Minor: $minorCount - Patch: $patchCount"
                if ($($findingsOutdated.Count) -gt 0) {
                    Write-Host $($findingsOutdated | ConvertTo-Json)
                }
            }
            if (($action -eq "check-licenses") -Or ($action -eq "all")) {
                Write-Host "-------------------------"
                Write-Host "License compliance check: $licenseCheckStatus"
                Write-Host "-------------------------"
                Write-Host "License breakdown:"
                foreach ($line in $summary) {
                    # otherwise, line breaks are gone :/
                    Write-Host "$line"
                }
            }
        }
    }

    if ($outputFileVuln) {
        $findingsVuln | ConvertTo-Json | Out-File $outputFileVuln -Encoding ASCII
    }
}
catch {
    if (-Not $silent) { Write-Host "Error occurred: $($_.Exception)" }
}
finally {
    Remove-Item -Path package.json
    Rename-Item -Path package.json.original -NewName package.json
    Pop-Location
    if ($vulnCheckStatus -ne "OK" -or $licenseCheckStatus -ne "OK" -or $outdatedCheckStatus -eq "FAILED") {
        Exit 1
    }
    elseif ($outdatedCheckStatus -eq "PARTIAL") {
        Write-Host "##vso[task.complete result=SucceededWithIssues;]DONE"
        Exit 0
    }
    else {
        Exit 0
    }    
}