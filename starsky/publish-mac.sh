#!/bin/bash
cd "$(dirname "$0")"
mkdir osx.10.12-x64
pushd starsky
dotnet publish -c release -r osx.10.12-x64 --framework netcoreapp2.0 --output ../osx.10.12-x64
popd
pushd starskysynccli
dotnet publish -c release -r osx.10.12-x64 --framework netcoreapp2.0 --no-dependencies --output ../osx.10.12-x64
popd
pushd starskyimportercli
dotnet publish -c release -r osx.10.12-x64 --framework netcoreapp2.0 --no-dependencies --output ../osx.10.12-x64
popd
pushd starskywebhtmlcli
dotnet publish -c release -r osx.10.12-x64 --framework netcoreapp2.0 --output ../osx.10.12-x64
popd
pushd starskygeocli
dotnet publish -c release -r osx.10.12-x64 --framework netcoreapp2.0 --output ../osx.10.12-x64
popd
