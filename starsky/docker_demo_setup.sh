#!/bin/bash

DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" >/dev/null 2>&1 && pwd )"

pushd $DIR

echo "go to: "
echo $DIR

function makeDemoUser {
  mylist=($(find . -type f -name "starskyadmincli.csproj"))
  dotnet run --project ${mylist[0]} -- --verbose --help
  dotnet run --project ${mylist[0]} -- --name demo@qdraw.nl --password demopassword
}

function getSamplePhotos {
  ls

  ls starsky/bin
  echo "-"
  ls starsky/bin

  mkdir -p out/storageFolder
  curl https://media.qdraw.nl/download/starsky-sample-photos/20190530_134303_DSC00279_e.jpg --output out/storageFolder/20190530_134303_DSC00279_e.jpg
  curl https://media.qdraw.nl/download/starsky-sample-photos/20190530_142906_DSC00373_e.jpg --output out/storageFolder/20190530_142906_DSC00373_e.jpg

}

if [ -z "$E_ISDEMO" ]; then
    echo "NO PARAM PASSED"
    makeDemoUser
    getSamplePhotos
else
    echo $E_ISDEMO
fi

popd
