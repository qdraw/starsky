#!/bin/bash

## Use this only in a docker env.
## This script uses /app

if [[ ! $(cat /proc/1/sched | head -n 1 | grep init) ]]; then {
    echo "in docker"
} else {
    echo "not in docker"
} fi

APPLICATION_DIR=/app/
STORAGE_FOLDER=/app/storageFolder

function makeDemoUser {
  starskyadmincli=($(find . -type f -name "starskyadmincli.csproj"))
  dotnet run --configuration Release --project ${starskyadmincli[0]} -- --connection "Data Source="$APPLICATION_DIR"/app__data.db" --basepath $STORAGE_FOLDER --verbose --help
  dotnet run --configuration Release --project ${starskyadmincli[0]} -- --connection "Data Source="$APPLICATION_DIR"/app__data.db" --basepath $STORAGE_FOLDER --name demo@qdraw.nl --password demo@qdraw.nl
}

function getSamplePhotos {
  mkdir -p $STORAGE_FOLDER
  curl https://media.qdraw.nl/download/starsky-sample-photos/20190530_134303_DSC00279_e.jpg --output $STORAGE_FOLDER"/20190530_134303_DSC00279_e.jpg"
  curl https://media.qdraw.nl/download/starsky-sample-photos/20190530_142906_DSC00373_e.jpg --output $STORAGE_FOLDER"/20190530_142906_DSC00373_e.jpg"

  starskysynccli=($(find . -type f -name "starskysynccli.csproj"))
  dotnet run --configuration Release --project ${starskysynccli[0]} -- --basepath $STORAGE_FOLDER --connection "Data Source="$APPLICATION_DIR"/app__data.db" --verbose --help
  dotnet run --configuration Release --project ${starskysynccli[0]} -- --basepath $STORAGE_FOLDER --connection "Data Source="$APPLICATION_DIR"/app__data.db" -s /20190530_134303_DSC00279_e.jpg
  dotnet run --configuration Release --project ${starskysynccli[0]} -- --basepath $STORAGE_FOLDER --connection "Data Source="$APPLICATION_DIR"/app__data.db" -s /20190530_142906_DSC00373_e.jpg
}

function start_pushd {
  echo "go to: "$APPLICATION_DIR
  mkdir -p $APPLICATION_DIR
  pushd $APPLICATION_DIR
}

function  end_popd {
  popd
}

if [ -z "$E_ISDEMO" ]; then
    echo "NO PARAM PASSED"
    start_pushd
    makeDemoUser
    getSamplePhotos
    end_popd
else
    echo $E_ISDEMO
fi
