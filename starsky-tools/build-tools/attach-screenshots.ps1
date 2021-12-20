#!/usr/bin/env pwsh

# Authentication - Pipeline (release)

# ADO_SYSTEM_ACCESSTOKEN = $(System.AccessToken)
# SYSTEM_TEAMFOUNDATIONCOLLECTIONURI = $(System.TeamFoundationCollectionUri)
# SYSTEM_TEAMPROJECTID = $(System.TeamProjectId)
# RELEASE_ID = $(Release.ReleaseId)
# REPO_DIR = $(System.DefaultWorkingDirectory)/_starsky
# CYPRESS_SCREENSHOTS = starsky-tools/end2end/cypress

# Authentication - Local Testing

# $Env:ADO_SYSTEM_ACCESSTOKEN = ""
# $Env:SYSTEM_TEAMFOUNDATIONCOLLECTIONURI = "https://dev.azure.com/<<username>>/"
# $Env:SYSTEM_TEAMPROJECTID = "<>"
# $Env:RELEASE_ID = 520
# $Env:REPO_DIR = "~/workspaces/starsky/"
# $Env:CYPRESS_SCREENSHOTS = "starsky-tools/end2end/cypress"

# Script --> 
$global:cwd = $env:REPO_DIR
$global:screenshotPath = Join-Path -Path $global:cwd -ChildPath $CYPRESS_SCREENSHOTS

write-host $screenshotPath

# Global Variables
$screenshotsHashtable = @{ } # Key = test name, Value = Full screenshot filename

# Authentication - Azure DevOps

$accessToken = $env:ADO_SYSTEM_ACCESSTOKEN
$teamFoundationCollectionUri = $env:SYSTEM_TEAMFOUNDATIONCOLLECTIONURI
$teamProjectId = $env:SYSTEM_TEAMPROJECTID
$releaseId = $env:RELEASE_ID

$pair = ":$($accessToken)"
$encodedCreds = [System.Convert]::ToBase64String([System.Text.Encoding]::ASCII.GetBytes($pair))
$basicAuthValue = "Basic $encodedCreds"

$headers = @{
  Authorization = $basicAuthValue
}

# Functions

function Save-Screenshots-Hashtable {
	Write-Host "Searching for screenshots in '$($global:screenshotPath)'"

	$screenshots = Get-ChildItem -Path $screenshotPath  -Filter *.png -Recurse -File | Select-Object -Property FullName, Name
	
	foreach ($screenshot in $screenshots) {

		$testName = $screenshot.Name -replace " --", ""
		$testName = $testName -replace " \(failed\).png", ""

		$screenshotsHashtable[$testName] = $screenshot.FullName

		Write-Host "Found screenshot '$($screenshot)' for test '$($testName)'"
	}
}

function Get-TestRuns {

	$today = Get-Date
	$tomorrow = $today.AddDays(1)
	$yesterday = $today.AddDays(-6)

	$minLastUpdatedDate = $yesterday.ToString("yyyy-MM-dd")
	$maxLastUpdatedDate = $tomorrow.ToString("yyyy-MM-dd")

	$testRunUrl = "$($teamFoundationCollectionUri)$($teamProjectId)/_apis/test/runs?api-version=5.1&minLastUpdatedDate=$($minLastUpdatedDate)&maxLastUpdatedDate=$($maxLastUpdatedDate)"
  # &buildIds=$($buildId)
  Write-Host ">> Getting Test Runs from '$testRunUrl'"

  $testRunResponse = @{ }
  try {
    $testRunResponse = Invoke-RestMethod -Uri $testRunUrl -Headers $headers
  }
  catch {
    # Dig into the exception to get the Response details.
    # Note that value__ is not a typo.
    Write-Host -ForegroundColor:Red "StatusCode:" $_.Exception.Response.StatusCode.value__ 
    Write-Host -ForegroundColor:Red "StatusDescription:" $_.Exception.Response.StatusDescription
  }

	$allResults = 0;
  write-host $testRunResponse.value.count

	foreach ($run in $testRunResponse.value) {
		if ( $run.release.id -eq $releaseId) {
		# write-host "Test" $run.release.id

		$results = Get-Test-Results -runId $run.id
			$allResults = $allResults + $results;
		}
	}

	return $allResults
}

function Find-Screenshot-From-Test-Result {
	param($testName)

	Write-Host "Searching for screenshot  for '$($testName)'"

	$screenshotFilename = $screenshotsHashtable[$testName]

	if ($null -ne $screenshotFilename) {
		Write-Host -ForegroundColor:Green "Found screenshot '$($screenshotFilename)' matching test '$($testName)'"
	}
	else {
		$testName = $testName.Replace("|","")
		$screenshotFilename = $screenshotsHashtable[$testName]
		if ($null -ne $screenshotFilename) {
			Write-Host -ForegroundColor:Green "Found screenshot '$($screenshotFilename)' matching test '$($testName)'"
		}
		else {
			Write-Host -ForegroundColor:Red "No screenshot found matching test: '$($testName)'"
		}
	}
	return $screenshotFilename
}

function Add-Attachment-To-TestResult {
	param($screenshotFilename, $testResultId)

	$screenshotFilenameWithoutPath = Split-Path $screenshotFilename -leaf

	$createTestResultsAttachmentUrl = "$teamFoundationCollectionUri$teamProjectId/_apis/test/runs/$($runId)/results/$($testResultId)/attachments?api-version=5.1-preview&outcomes=Failed"

	$base64string = [Convert]::ToBase64String([IO.File]::ReadAllBytes($screenshotFilename))

  	$screenshotFilenameWithoutPath = $screenshotFilenameWithoutPath.Replace("|","-");

	$body = @{
		fileName       = $screenshotFilenameWithoutPath
		comment        = "Attaching screenshot"
		attachmentType = "GeneralAttachment"
		stream         = $base64string
	}

	$json = $body | ConvertTo-Json

	Write-Host "Attaching screenshot by posting to '$($createTestResultsAttachmentUrl)'"

	$response = Invoke-RestMethod $createTestResultsAttachmentUrl -Headers $headers -Method Post -Body $json -ContentType "application/json"

	Write-Host "Response from posting screenshot '$($response)'"
}

function Get-Test-Results {
	param($runId)

	$testResultsUrl = "$teamFoundationCollectionUri$teamProjectId/_apis/test/runs/$($runId)/results?api-version=5.1&outcomes=Failed"

	Write-Host "Getting Test Results from '$($testResultsUrl)'"

	$testResultsResponse = Invoke-RestMethod -Uri $testResultsUrl -Headers $headers

	foreach ($testResult in $testResultsResponse.value) {

		Write-Host "Found failing test '$($testResult.testCase.name)'"

		$screenshotFilename = Find-Screenshot-From-Test-Result -testName $testResult.testCase.name

		if ($null -ne $screenshotFilename) {
			Add-Attachment-To-TestResult -screenshotFilename $screenshotFilename -testResultId	$testResult.id
		}
	}

	Write-Host "testResultsResponse.value.Count: $($testResultsResponse.value.Count)"

	return $testResultsResponse.value.Count
}

# Entry Point
Write-Host "Azure DevOps Test Result Attacher v.0.1b - edited"
Write-Host ""
Write-Host "TeamFoundationCollectionUri: $teamFoundationCollectionUri"
Write-Host "TeamProjectId: $teamProjectId"
Write-Host "releaseId: $releaseId"
Write-Host ""

Save-Screenshots-Hashtable
$failedTests = Get-TestRuns

Write-Host "Failed Tests: $($failedTests)"

$LASTEXITCODE = 0

if ($failedTests -gt 0) {
	$LASTEXITCODE = 1
}

# source: http://codestyle.dk/2020/05/19/cypress-screenshots-are-missing-in-azure-pipelines/
# https://github.com/krileo/azure-devops-screenshot-attachments

Write-Host "Exiting with exitCode $($LASTEXITCODE)"
exit $LASTEXITCODE
