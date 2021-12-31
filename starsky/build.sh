#!/bin/sh

# Windows 64 bits: 'win7-x64'
# Mac: 'osx-x64'
# ARM64: 'linux-arm64'
# Raspberry Pi: 'linux-arm'
# Windows 32 bits: 'win7-x86'

# >>>>>>>>>>>                WHEN NEW PROJECT RUN FIRST!!    >>>
# dotnet new tool-manifest
# dotnet tool install Cake.Tool
# <<<<<<<<<<<               WHEN NEW PROJECT RUN FIRST!!    <<<

#  Source: Simplifying the Cake global tool bootstrapper scripts with .NET Core 3 local tools (https://andrewlock.net/simplifying-the-cake-global-tool-bootstrapper-scripts-in-netcore3-with-local-tools/)

# set some .NET settings 
export DOTNET_CLI_TELEMETRY_OPTOUT=1
export DOTNET_SKIP_FIRST_TIME_EXPERIENCE=1

pushd $(dirname "$0")

# Define default arguments.
SCRIPT="build.cake"
CAKE_ARGUMENTS=""

# Parse arguments.
for i in "$@"; do
    case $1 in
        -s|--script) SCRIPT="$2"; shift ;;
        --) shift; CAKE_ARGUMENTS="${CAKE_ARGUMENTS} $@"; break ;;
        *) CAKE_ARGUMENTS="${CAKE_ARGUMENTS} $1" ;;
    esac
    shift
done
set -- ${CAKE_ARGUMENTS}

# Restore Cake tool
dotnet tool restore

if [ $? -ne 0 ]; then
    echo "An error occured while installing Cake."
    exit 1
fi

# Start Cake

dotnet tool run dotnet-cake  "--" "$SCRIPT" "$@"
EXITSTATUS=$?

popd

[ $EXITSTATUS -eq 0 ] && echo "Build was successful :)" || echo "Build has failed - unit test search for: Error Message"
exit $EXITSTATUS
