# Configuration
# source: http://codestyle.dk/2020/05/19/cypress-screenshots-are-missing-in-azure-pipelines/
# https://github.com/krileo/azure-devops-screenshot-attachments

$cwd = Get-Location # D:\azagent_wecycle\A1\_work\r4\a
$global:screenshotPath = Join-Path -Path $cwd -ChildPath "_repo\tests\e2e\cypress\screenshots"

If(!(test-path $global:screenshotPath))
{
	write-host $screenshotPath 
	write-host "Does not exists"
	exit 0
}

# Global Variables
$screenshotsHashtable = @{ } # Key = test name, Value = Full screenshot filename

# Authentication - Azure DevOps

$accessToken = $env:SYSTEM_ACCESSTOKEN
$teamFoundationCollectionUri = $env:SYSTEM_TEAMFOUNDATIONCOLLECTIONURI
$teamProjectId = $env:SYSTEM_TEAMPROJECTID
$buildId = $env:BUILD_BUILDID
$headers = @{ Authorization = "Bearer " + $accessToken }

# Authentication - Local Testing
# $accessToken = "ENTER_YOUR_TOKEN"
# $teamFoundationCollectionUri = "https://dev.azure.com/lbforsikring/"
# $teamProjectId = "1e10926f-6e19-47b5-9049-b1661f115ebe"
# $buildId = 8600
# $pair = "$($accessToken):$($accessToken)"
# $encodedCreds = [System.Convert]::ToBase64String([System.Text.Encoding]::ASCII.GetBytes($pair))
# $headers = @{ Authorization = "Basic $encodedCreds" }

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
	$yesterday = $today.AddDays(-1)

	$minLastUpdatedDate = $yesterday.ToString("yyyy-MM-dd")
	$maxLastUpdatedDate = $tomorrow.ToString("yyyy-MM-dd")

	$testRunUrl = "$($teamFoundationCollectionUri)$($teamProjectId)/_apis/test/runs?api-version=5.1&minLastUpdatedDate=$($minLastUpdatedDate)&maxLastUpdatedDate=$($maxLastUpdatedDate)&buildIds=$($buildId)"
	Write-Host "Getting Test Runs from '$testRunUrl'"

	$testRunResponse = Invoke-RestMethod -Uri $testRunUrl -Headers $headers

	$allResults = 0;

	foreach ($run in $testRunResponse.value) {
		$results = Get-Test-Results -runId $run.id

		$allResults = $allResults + $results;
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
		Write-Host -ForegroundColor:Red "No screenshot found matching test '$($name)'"
	}

	return $screenshotFilename
}

function Add-Attachment-To-TestResult {
	param($screenshotFilename, $testResultId)

	$screenshotFilenameWithoutPath = Split-Path $screenshotFilename -leaf

	$createTestResultsAttachmentUrl = "$teamFoundationCollectionUri$teamProjectId/_apis/test/runs/$($runId)/results/$($testResultId)/attachments?api-version=5.1-preview&outcomes=Failed"

	$base64string = [Convert]::ToBase64String([IO.File]::ReadAllBytes($screenshotFilename))

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
Write-Host "Azure DevOps Test Result Attacher v.0.1b"
Write-Host ""
Write-Host "AccessToken: $accessToken"
Write-Host "TeamFoundationCollectionUri: $teamFoundationCollectionUri"
Write-Host "TeamProjectId: $teamProjectId"
Write-Host "BuildId: $buildId"
Write-Host ""

Save-Screenshots-Hashtable
$failedTests = Get-TestRuns

Write-Host "Failed Tests: $($failedTests)"

$LASTEXITCODE = 0

if ($failedTests -gt 0) {
	$LASTEXITCODE = 1
}

Write-Host "Exiting with exitCode $($LASTEXITCODE)"
exit $LASTEXITCODE
