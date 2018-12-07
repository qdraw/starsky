#!/bin/bash
cd "$(dirname "$0")"
mkdir linux-arm
pushd starsky
dotnet publish -c release -r linux-arm --framework netcoreapp2.1 --output ../linux-arm
popd
pushd starskysynccli
dotnet publish -c release -r linux-arm --framework netcoreapp2.1 --no-dependencies --output ../linux-arm
popd
pushd starskyimportercli
dotnet publish -c release -r linux-arm --framework netcoreapp2.1 --no-dependencies --output ../linux-arm
popd
pushd starskywebhtmlcli
dotnet publish -c release -r linux-arm --framework netcoreapp2.1 --output ../linux-arm
popd
pushd starskygeocli
dotnet publish -c release -r linux-arm --framework netcoreapp2.1 --output ../linux-arm
popd
