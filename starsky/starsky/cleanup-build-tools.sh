#!/bin/bash

# when force everything
# docker system prune -a -f

SCRIPT_DIR="$( cd "$( dirname "$0" )" && pwd )"

if [ -d $HOME"/.sonar" ] 
then
    echo "Remove sonar cache -> "$HOME"/.sonar"
    rm -rf $HOME"/.sonar"
else
    echo "Skip: remove sonar cache. -> "$HOME"/.sonar"
fi

PARENT_DIR="$(dirname "$SCRIPT_DIR")"
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
    dotnet clean starsky.sln || true
fi

if ! command -v docker &> /dev/null
then
    echo "FAIL; docker could not be found"
    exit
fi

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