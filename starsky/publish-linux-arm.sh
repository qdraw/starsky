#!/bin/bash
cd "$(dirname "$0")"
mkdir linux-arm
pushd starsky
dotnet publish -c release -r linux-arm --framework netcoreapp2.0 --output ../linux-arm
popd
pushd starsky-cli
dotnet publish -c release -r linux-arm --framework netcoreapp2.0 --no-dependencies --output ../linux-arm
popd
pushd starskyimportercli
dotnet publish -c release -r linux-arm --framework netcoreapp2.0 --no-dependencies --output ../linux-arm
popd
