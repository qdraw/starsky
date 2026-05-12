
param(
    [Parameter(Mandatory=$false)][switch]$help,
    [Parameter(Mandatory=$false)][string]$port='4000'
)
# Port 4823 an example port number

$TestUrl = "http://localhost:$port"

[Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12
Write-Output "Starting"
$MaxAttempts = 30

if (-not [string]::IsNullOrWhiteSpace($TestUrl)) {
    Write-Output "Making request to $TestUrl"
    try {
        $stopwatch = [Diagnostics.Stopwatch]::StartNew()
        # Allow redirections on the warm up
        $response = Invoke-WebRequest -UseBasicParsing $TestUrl -MaximumRedirection 10
        $stopwatch.Stop()
        $statusCode = [int]$response.StatusCode
        $stopwatchMilliSeconds = $Stopwatch.ElapsedMilliseconds
        Write-Output "$statusCode Warmed Up Site $TestUrl in $stopWatchMilliSeconds ms"
    } catch {
        $_.Exception|Format-List -Force
    }
    for ($i = 0; $i -lt $MaxAttempts; $i++) {
        try {
            Write-Output "Checking Site"
            $stopwatch = [Diagnostics.Stopwatch]::StartNew()
            # Don't allow redirections on the check
            $response = Invoke-WebRequest -UseBasicParsing $TestUrl -MaximumRedirection 10
            $stopwatch.Stop()
            $statusCode = [int]$response.StatusCode
            $stopwatchMilliSeconds = $Stopwatch.ElapsedMilliseconds
            Write-Output "$statusCode Second request took $stopWatchMilliSeconds ms"
            if ($statusCode -ge 200 -And $statusCode -lt 400) {
                break;
            }
            Start-Sleep -s 2
        } catch {
            $_.Exception|format-list -force
            Start-Sleep -s 30
        }
    }
    if ($statusCode -ge 200 -and $statusCode -lt 400) {
        # YES!, it worked
    } else {
        throw "Warm up failed for " + $TestUrl
    }
} else {
    Write-Output "No TestUrl configured for this machine."
}
Write-Output "Done"