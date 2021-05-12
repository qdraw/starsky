param (
    [Parameter(Mandatory=$True)][string]$targetFolder
)

if($targetFolder -eq ".") {
   write-host "fallback to Get-Location"
   $targetFolder =  Get-Location
}

write-host "audit script dotnet $targetFolder"

# Check .NET version
$version = $(dotnet --version)
if($version.StartsWith("3") || $version.StartsWith("2") || $version.StartsWith("1")) {
    write-host "You need .NET SDK 5.0.200 or newer"
    # https://devblogs.microsoft.com/nuget/how-to-scan-nuget-packages-for-security-vulnerabilities/
    exit 1
}

function check-vulnerable($solution) {

  write-host "run command: dotnet list $solution package --vulnerable"

  $output = $(dotnet list $solution package --vulnerable) 

  $errors = $output | Select-String '>' | Where-Object {!$_.ToString().Trim().StartsWith( "<PROJECT | SOLUTION>")}

  if ($errors.Count -gt 0)
  {
    foreach ($err in $errors)
    {
        Write-Host "##vso[task.logissue type=error]Error with $err"
    }

    write-host "Task failed"
    exit 1
  }
  else
  {
    Write-Host "No vulnerable NuGet-packages"
    return
  }
}

function nuget-restore($solution) {
  dotnet restore $solution
}

[array]$solutions=Get-ChildItem -Path $targetFolder/** -Include *.sln | select -expand fullname

if ($solutions.Length -eq 0)
{
    write-host "no solutions found"
    exit 0
}

foreach ($solution in $solutions)
{
    nuget-restore -solution  $solution
    check-vulnerable -solution  $solution
}

exit 0