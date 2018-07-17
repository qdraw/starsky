#!/bin/bash
cd "$(dirname "$0")"
mkdir linux-arm
pushd starsky
dotnet publish -c release -r linux-arm --output ../linux-arm
popd
pushd starsky-cli
dotnet publish -c release -r linux-arm --no-dependencies --output ../linux-arm
popd
pushd starskyimportercli
dotnet publish -c release -r linux-arm --no-dependencies --output ../linux-arm
popd
