#!/bin/bash

# when force everything
# docker system prune -a -f

SCRIPT_DIR="$( cd "$( dirname "$0" )" && pwd )"
NET_MONIKER="net8.0"

SONAR_CACHE_DIR=".sonar"
if [[ -d "$HOME/$SONAR_CACHE_DIR" ]] 
then
    echo "Remove sonar cache -> $HOME/$SONAR_CACHE_DIR"
    rm -rf "$HOME/$SONAR_CACHE_DIR"
else
    echo "Skip: remove sonar cache. -> $HOME/$SONAR_CACHE_DIR"
fi

PARENT_DIR="$(dirname "$SCRIPT_DIR")"
# eg starsky/starsky

## docs
GIT_ROOT_DIR="$(dirname "$PARENT_DIR")"

if [[ -d "$GIT_ROOT_DIR""/documentation/bin" ]] 
then
    rm -rf "$GIT_ROOT_DIR""/documentation/bin"
else
    echo "Skip: remove documentation bin. -> ""$GIT_ROOT_DIR""/documentation/bin"
fi

if [[ -d "$GIT_ROOT_DIR""/documentation/obj" ]] 
then
    rm -rf "$GIT_ROOT_DIR""/documentation/obj"
else
    echo "Skip: remove documentation obj. -> ""$GIT_ROOT_DIR""/documentation/obj"
fi
## end docs


if [[ -d "$PARENT_DIR""/TestResults" ]] 
then
    rm -rf "$PARENT_DIR""/TestResults"
else
    echo "Skip: remove TestResults cache. -> ""$PARENT_DIR""/TestResults"
fi

echo "next: search for bin folders in sub projects, but not main due the fact that the config and database is stored there"
SEARCH_OUTPUT=()
SEARCH_INPUT="bin"
while IFS=  read -r -d $'\0'; do
    SEARCH_OUTPUT+=("$REPLY")
done < <(find . -name "${SEARCH_INPUT}" -print0)

for i in "${SEARCH_OUTPUT[@]}"
do
    if [[ "$i" == *"feature"* || "$i" == *"foundation"* ]]; 
    then
       echo "next remove feature/found.: " $i
       rm -rf $i
    fi
    
    if [[ "$i" == *"cli/bin" && "$i" == "./starsky"* &&  "$i" != *"node_modules"* ]]; then
       echo "next remove cli/bins: " $i
       rm -rf $i
    fi
done

echo "next: delete coverage files"

# coverage files
if [[ -f "$PARENT_DIR""/starskytest/coverage-merge-cobertura.xml" ]] 
then
    rm  "$PARENT_DIR""/starskytest/coverage-merge-cobertura.xml"
fi

if [[ -f "$PARENT_DIR""/starskytest/coverage-merge-sonarqube.xml" ]] 
then
    rm  "$PARENT_DIR""/starskytest/coverage-merge-sonarqube.xml"
fi

if [[ -f "$PARENT_DIR""/starskytest/jest-coverage.cobertura.xml" ]] 
then
    rm  "$PARENT_DIR""/starskytest/jest-coverage.cobertura.xml"
fi

if [[ -f "$PARENT_DIR""/starskytest/netcore-coverage.opencover.xml" ]] 
then
    rm  "$PARENT_DIR""/starskytest/netcore-coverage.opencover.xml"
fi
# end coverage files

echo "next: delete dependency files"


# dependency files
DEPENDENCIES_DIR="/dependencies"
STARSKY_BIN_DEBUG_DIR="/starsky/bin/Debug/"
STARSKY_BIN_RELEASE_DIR="/starsky/bin/Release/"

if [[ -d "$PARENT_DIR$STARSKY_BIN_RELEASE_DIR$NET_MONIKER$DEPENDENCIES_DIR" ]] 
then
    rm -rf "$PARENT_DIR$STARSKY_BIN_RELEASE_DIR$NET_MONIKER$DEPENDENCIES_DIR"
else
    echo "Skip: remove dependencies cache (Release). -> $PARENT_DIR$STARSKY_BIN_RELEASE_DIR$NET_MONIKER$DEPENDENCIES_DIR"
fi

if [[ -d "$PARENT_DIR$STARSKY_BIN_DEBUG_DIR$NET_MONIKER$DEPENDENCIES_DIR" ]] 
then
    rm -rf "$PARENT_DIR$STARSKY_BIN_DEBUG_DIR$NET_MONIKER$DEPENDENCIES_DIR"
else
    echo "Skip: remove dependencies cache (Debug) -> $PARENT_DIR$STARSKY_BIN_DEBUG_DIR$NET_MONIKER$DEPENDENCIES_DIR"
fi

# temp folder of the project
if [[ -d "$PARENT_DIR$STARSKY_BIN_RELEASE_DIR$NET_MONIKER/temp" ]] 
then
    rm -rf "$PARENT_DIR$STARSKY_BIN_RELEASE_DIR$NET_MONIKER/temp"
else
    echo "Skip: remove temp cache (Release). -> $PARENT_DIR$STARSKY_BIN_RELEASE_DIR$NET_MONIKER/temp"
fi

if [[ -d "$PARENT_DIR$STARSKY_BIN_DEBUG_DIR$NET_MONIKER/temp" ]] 
then
    rm -rf "$PARENT_DIR$STARSKY_BIN_DEBUG_DIR$NET_MONIKER/temp"
else
    echo "Skip: remove temp cache (Debug) -> $PARENT_DIR$STARSKY_BIN_DEBUG_DIR$NET_MONIKER/temp"
fi

SONARQUBE_CACHE_DIR="/.sonarqube"
if [[ -d "$PARENT_DIR$SONARQUBE_CACHE_DIR" ]] 
then
    echo "Remove sonar cache -> $PARENT_DIR$SONARQUBE_CACHE_DIR"
    rm -rf "$PARENT_DIR$SONARQUBE_CACHE_DIR"
else
    echo "Skip: remove sonar cache. -> $PARENT_DIR$SONARQUBE_CACHE_DIR"
fi

echo "next clean npm"

if command -v npm &> /dev/null
then
    echo "clean npm"
    npm cache clean --force
fi


echo "remove obj folder"
find $PARENT_DIR -name "obj" -type d -exec rm -r "{}" \;
echo "end rm obj"

echo "next clean dotnet"


if command -v dotnet &> /dev/null
then
    echo "clean dotnet nuget"
    dotnet nuget locals all --clear
    
    cd $PARENT_DIR
    echo "next clean debug"
    dotnet clean starsky.sln || true
    echo "next clean release"
    dotnet clean starsky.sln --configuration Release || true
fi


# cypress cache on mac os
CYPRESS_CACHE_DIR_MACOS="/Library/Caches/Cypress"
if [[ -d "$HOME$CYPRESS_CACHE_DIR_MACOS" ]] 
then
    COUNT_CYPRESS=$(ls "$HOME$CYPRESS_CACHE_DIR_MACOS" | wc -l | sed 's/ *$//g')
    if [[ $COUNT_CYPRESS -ne "1" ]]; then
        echo "Remove cypress cache -> $HOME$CYPRESS_CACHE_DIR_MACOS"
        rm -rf "$HOME$CYPRESS_CACHE_DIR_MACOS"
        
        # and install it again
        ROOT_REPO_DIR="$(dirname "$PARENT_DIR")"
        
        cd $ROOT_REPO_DIR"/starsky-tools/end2end"
        echo "next: re-install cypress"
        npm ci
    else
        echo "Skip: remove cypress cache. There is only 1 folder in the cypress cache, skip remove"
    fi

else
    echo "Skip: remove cypress cache. -> $HOME$CYPRESS_CACHE_DIR_MACOS"
fi

echo "Next clean electron builder cache"

# https://github.com/electron/get#cache-location
ELECTRON_CACHE_DIR_LINUX="/.cache/electron"
if [[ -d "$HOME$ELECTRON_CACHE_DIR_LINUX" ]] 
then
    echo "Remove electron cache [linux] -> $HOME$ELECTRON_CACHE_DIR_LINUX"
    rm -rf "$HOME$ELECTRON_CACHE_DIR_LINUX"
    mkdir -p "$HOME$ELECTRON_CACHE_DIR_LINUX"
else
    echo "Skip: remove electron cache [linux] -> $HOME$ELECTRON_CACHE_DIR_LINUX"
fi

echo "Next clean electron builder cache [macOS]"
ELECTRON_CACHE_DIR_MACOS="/Library/Caches/electron"
if [[ -d $HOME$ELECTRON_CACHE_DIR_MACOS ]] 
then
    echo "Remove electron cache [macOS] -> $HOME$ELECTRON_CACHE_DIR_MACOS"
    rm -rf $HOME$ELECTRON_CACHE_DIR_MACOS   
    mkdir -p $HOME$ELECTRON_CACHE_DIR_MACOS
else
    echo "Skip: remove electron cache [macOS]. -> $HOME$ELECTRON_CACHE_DIR_MACOS"
fi

# not used in the project
if command -v yarn &> /dev/null
then
    echo "clean yarn [not used in project]"
    yarn cache clean
fi

# not used in the project
if command -v pnpm &> /dev/null
then
    echo "clean pnpm [not used in project]"
    pnpm store prune
fi

# Docker cache clean
if ! command -v docker &> /dev/null
then
    echo "FAIL; docker could not be found"
    exit
fi

echo "next: docker"

COLOR_REST="$(tput sgr0)"
COLOR_RED="$(tput setaf 1)"
COLOR_GREEN="$(tput setaf 2)"
COLOR_BLUE="$(tput setaf 4)"
PRINTF_COLOR_FORMAT_NEWLINE='%s%s%s\n'
PRINTF_COLOR_FORMAT='%s%s%s'

if (! docker stats --no-stream &> /dev/null); then
  if [[ "$(uname)" == "Darwin" ]]; then
    # On Mac OS this would be the terminal command to launch Docker
    open /Applications/Docker.app
  elif [[ "$(uname -s)" == *"MINGW64_NT"* ]]; then
    printf $PRINTF_COLOR_FORMAT_NEWLINE $COLOR_RED "Make sure Docker Desktop is running and restart this script" $COLOR_REST
    echo "C:\Program Files\Docker\Docker\Docker Desktop.exe"
    exit 1
  fi
  
  printf $PRINTF_COLOR_FORMAT_NEWLINE $COLOR_BLUE "Waiting for Docker to launch..." $COLOR_REST
  # Wait until Docker daemon is running and has completed initialisation
  while (! docker stats --no-stream &> /dev/null); do
    printf $PRINTF_COLOR_FORMAT $COLOR_GREEN '..' $COLOR_REST
    # Docker takes a few seconds to initialize
    sleep 2
  done
fi
echo ""

docker builder prune --filter 'until=8h' -f
docker image prune --filter 'until=8h' -f
docker container prune --filter "until=8h" -f

echo "end"