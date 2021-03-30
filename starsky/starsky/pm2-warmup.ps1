$TestUrl = "$(testUrl)"
[Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12
Write-Output "Starting"
$MaxAttempts = 5
If (![string]::IsNullOrWhiteSpace($TestUrl)) {
    Write-Output "Making request to $TestUrl"
    Try {
        $stopwatch = [Diagnostics.Stopwatch]::StartNew()
        # Allow redirections on the warm up
        $response = Invoke-WebRequest -UseBasicParsing $TestUrl -MaximumRedirection 10
        $stopwatch.Stop()
        $statusCode = [int]$response.StatusCode
        $stopwatchMilliSeconds = $Stopwatch.ElapsedMilliseconds
        Write-Output "$statusCode Warmed Up Site $TestUrl in $stopWatchMilliSeconds ms"
    } catch {
        $_.Exception|format-list -force
    }
    For ($i = 0; $i -lt $MaxAttempts; $i++) {
        try {
            Write-Output "Checking Site"
            $stopwatch = [Diagnostics.Stopwatch]::StartNew()
            # Don't allow redirections on the check
            $response = Invoke-WebRequest -UseBasicParsing $TestUrl -MaximumRedirection 10
            $stopwatch.Stop()
            $statusCode = [int]$response.StatusCode
            $stopwatchMilliSeconds = $Stopwatch.ElapsedMilliseconds
            Write-Output "$statusCode Second request took $stopWatchMilliSeconds ms"
            If ($statusCode -ge 200 -And $statusCode -lt 400) {
                break;
            }
            Start-Sleep -s 2
        } catch {
            $_.Exception|format-list -force
            Start-Sleep -s 30
        }
    }
    If ($statusCode -ge 200 -And $statusCode -lt 400) {
        # YES!, it worked
    } Else {
        throw "Warm up failed for " + $TestUrl
    }
} Else {
    Write-Output "No TestUrl configured for this machine."
}
Write-Output "Done"