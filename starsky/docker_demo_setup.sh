#!/bin/bash

DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" >/dev/null 2>&1 && pwd )"

pushd $DIR

function makeDemoUser {
  dotnet run --project ../starsky/starskyadmincli/starskyadmincli.csproj --name demo@qdraw.nl --password demopassword
}

function getSampleImages {
  mkdir -p starsky/bin/Release/netcoreapp*/storageFolder
  curl https://media.qdraw.nl/download/starsky-sample-photos.zip --output starsky/bin/Release/netcoreapp*/storageFolder/sample-photos.zip
  unzip starsky/bin/Release/netcoreapp*/storageFolder/sample-photos.zip
}

if [ -z "$E_ISDEMO" ]; then
    echo "NO PARAM PASSED"
    makeDemoUser
else
    echo $E_ISDEMO
fi

popd
