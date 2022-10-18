#!/bin/bash

# when force everything
# docker system prune -a -f

SCRIPT_DIR="$( cd "$( dirname "$0" )" && pwd )"
NET_MONIKER="net6.0"

if [ -d $HOME"/.sonar" ] 
then
    echo "Remove sonar cache -> "$HOME"/.sonar"
    rm -rf $HOME"/.sonar"
else
    echo "Skip: remove sonar cache. -> "$HOME"/.sonar"
fi

PARENT_DIR="$(dirname "$SCRIPT_DIR")"

if [ -d "$PARENT_DIR""/TestResults" ] 
then
    rm -rf "$PARENT_DIR""/TestResults"
else
    echo "Skip: remove TestResults cache. -> ""$PARENT_DIR""/TestResults"
fi

# coverage files
if [ -f "$PARENT_DIR""/starskytest/coverage-merge-cobertura.xml" ] 
then
    rm  "$PARENT_DIR""/starskytest/coverage-merge-cobertura.xml"
fi

if [ -f "$PARENT_DIR""/starskytest/coverage-merge-sonarqube.xml" ] 
then
    rm  "$PARENT_DIR""/starskytest/coverage-merge-sonarqube.xml"
fi

if [ -f "$PARENT_DIR""/starskytest/jest-coverage.cobertura.xml" ] 
then
    rm  "$PARENT_DIR""/starskytest/jest-coverage.cobertura.xml"
fi

if [ -f "$PARENT_DIR""/starskytest/netcore-coverage.opencover.xml" ] 
then
    rm  "$PARENT_DIR""/starskytest/netcore-coverage.opencover.xml"
fi
# end coverage files

# dependency files
if [ -d "$PARENT_DIR""/starsky/bin/Release/"$NET_MONIKER"/dependencies" ] 
then
    rm -rf "$PARENT_DIR""/starsky/bin/Release/"$NET_MONIKER"/dependencies"
else
    echo "Skip: remove dependencies cache (Release). -> ""$PARENT_DIR""/starsky/bin/Release/"$NET_MONIKER"/dependencies"
fi

if [ -d "$PARENT_DIR""/starsky/bin/Debug/"$NET_MONIKER"/dependencies" ] 
then
    rm -rf "$PARENT_DIR""/starsky/bin/Debug/"$NET_MONIKER"/dependencies"
else
    echo "Skip: remove dependencies cache (Debug) -> ""$PARENT_DIR""/starsky/bin/Debug/"$NET_MONIKER"/dependencies"
fi

# temp folder of the project
if [ -d "$PARENT_DIR""/starsky/bin/Release/"$NET_MONIKER"/temp" ] 
then
    rm -rf "$PARENT_DIR""/starsky/bin/Release/"$NET_MONIKER"/temp"
else
    echo "Skip: remove temp cache (Release). -> ""$PARENT_DIR""/starsky/bin/Release/"$NET_MONIKER"/temp"
fi

if [ -d "$PARENT_DIR""/starsky/bin/Debug/"$NET_MONIKER"/temp" ] 
then
    rm -rf "$PARENT_DIR""/starsky/bin/Debug/"$NET_MONIKER"/temp"
else
    echo "Skip: remove temp cache (Debug) -> ""$PARENT_DIR""/starsky/bin/Debug/"$NET_MONIKER"/temp"
fi

if [ -d $PARENT_DIR"/.sonarqube" ] 
then
    echo "Remove sonar cache -> "$PARENT_DIR"/.sonarqube"
    rm -rf $PARENT_DIR"/.sonarqube"
else
    echo "Skip: remove sonar cache. -> "$PARENT_DIR"/.sonarqube"
fi

if command -v npm &> /dev/null
then
    echo "clean npm"
    npm cache clean --force
fi


echo "remove obj folder"
find $PARENT_DIR -name "obj" -type d -exec rm -r "{}" \;
echo "end rm obj"

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
if [ -d "$HOME""/Library/Caches/Cypress" ] 
then
    COUNT_CYPRESS=$(ls "$HOME""/Library/Caches/Cypress" | wc -l | sed 's/ *$//g')
    if [ $COUNT_CYPRESS -ne "1" ]; then
        echo "Remove cypress cache -> "$HOME"/Library/Caches/Cypress"
        rm -rf "$HOME""/Library/Caches/Cypress"
        
        # and install it again
        ROOT_REPO_DIR="$(dirname "$PARENT_DIR")"
        
        cd $ROOT_REPO_DIR"/starsky-tools/end2end"
        echo "next: re-install cypress"
        npm ci
    else
        echo "Skip: remove cypress cache. There is only 1 folder in the cypress cache, skip remove"
    fi

else
    echo "Skip: remove cypress cache. -> "$HOME"/Library/Caches/Cypress"
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


if (! docker stats --no-stream &> /dev/null); then
  if [[ "$(uname)" == "Darwin" ]]; then
    # On Mac OS this would be the terminal command to launch Docker
    open /Applications/Docker.app
  elif [[ "$(uname -s)" == *"MINGW64_NT"* ]]; then
    printf '%s%s%s\n' $COLOR_RED "Make sure Docker Desktop is running and restart this script" $COLOR_REST
    echo "C:\Program Files\Docker\Docker\Docker Desktop.exe"
    exit 1
  fi
  
  printf '%s%s%s\n' $COLOR_BLUE "Waiting for Docker to launch..." $COLOR_REST
  # Wait until Docker daemon is running and has completed initialisation
  while (! docker stats --no-stream &> /dev/null); do
    printf '%s%s%s' $COLOR_GREEN '..' $COLOR_REST
    # Docker takes a few seconds to initialize
    sleep 2
  done
fi
echo ""

docker builder prune --filter 'until=8h' -f
docker image prune --filter 'until=8h' -f
docker container prune --filter "until=8h" -f

echo "end"