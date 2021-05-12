param (
    [string]$projectDirectory = ""
)

if($projectDirectory -eq "") {
   write-host "fallback to Get-Location"
   $projectDirectory =  Get-Location
}

write-host "audit dotnet $projectDirectory"

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

  $errors = $output | Select-String '>'

  if ($errors.Count -gt 0)
  {
    foreach ($err in $errors)
    {
        if($err.ToString().Trim().StartsWith( "<PROJECT | SOLUTION>")) {
            continue;
        }
        Write-Host "##vso[task.logissue type=error]Error with $err"
    }
    exit 1
  }
  else
  {
    Write-Host "No vulnerable NuGet-packages"
  }
}

# current
# $solutions = Get-ChildItem -Path $projectDirectory/** -Name -Include *.sln
[array]$solutions=Get-ChildItem -Path $projectDirectory/** -Include *.sln | select -expand fullname

if ($solutions.Length -eq 0)
{
    write-host "no solutions found"
}

foreach ($solution in $solutions)
{
    check-vulnerable -solution  $solution
}

