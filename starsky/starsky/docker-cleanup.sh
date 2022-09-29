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

docker builder prune --filter 'until=8h' -f
docker image prune --filter 'until=8h' -f
docker container prune --filter "until=8h" -f