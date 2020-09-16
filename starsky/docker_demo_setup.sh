#!/bin/bash

DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" >/dev/null 2>&1 && pwd )"

pushd $DIR

function makeDemoUser {
  mylist=($(find . -type f -name "starskyadmincli.csproj"))
  dotnet run --project ${mylist[0]} -h -v
  dotnet run --project ${mylist[0]} --name demo@qdraw.nl --password demopassword
}

function getSamplePhotos {
  ls

  ls starsky/bin
  echo "-"
  ls starsky/bin

  mkdir -p starsky/bin/Release/netcoreapp*/storageFolder
  curl https://media.qdraw.nl/download/starsky-sample-photos.zip --output starsky/bin/Release/netcoreapp*/storageFolder/sample-photos.zip
  unzip starsky/bin/Release/netcoreapp*/storageFolder/sample-photos.zip
}

if [ -z "$E_ISDEMO" ]; then
    echo "NO PARAM PASSED"
    makeDemoUser
    getSamplePhotos
else
    echo $E_ISDEMO
fi

popd
