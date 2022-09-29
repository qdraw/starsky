#!/bin/bash

# when force everything
# docker system prune -a -f


if [ -d $HOME"/.sonar" ] 
then
    echo "Remove sonar cache -> "$HOME"/.sonar"
    rm -rf $HOME"/.sonar"
else
    echo "Skip: remove sonar cache. -> "$HOME"/.sonar"
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
fi

if ! command -v docker &> /dev/null
then
    echo "FAIL; docker could not be found"
    exit
fi

docker builder prune --filter 'until=8h' -f
docker image prune --filter 'until=8h' -f
docker container prune --filter "until=8h" -f