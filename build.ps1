# Get the directory containing the script being executed
$scriptDirectory = $PSScriptRoot

# Define the path to the child script relative to the script directory
$netChildScriptPath = Join-Path -Path $scriptDirectory -ChildPath "starsky\build.ps1"
$netSourceFolder = Join-Path -Path $scriptDirectory -ChildPath "starsky"

# Check if the child script exists
if (Test-Path $netChildScriptPath -PathType Leaf) {
    # Execute the child script
    pushd $netSourceFolder
    & $netChildScriptPath $args
    popd
} else {
    Write-Host "The child script '$childScriptPath' does not exist."
}