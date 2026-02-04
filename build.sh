#!/bin/bash

# Navigate to the directory containing the script
cd "$(dirname "$0")"

# Define the paths to the scripts
bashScript="./starsky/build.sh"

# Check if the PowerShell script exists and execute it
if [ -f "$bashScript" ]; then
    bash "$bashScript" "$@"
else
    echo "build.sh is not found."
fi
