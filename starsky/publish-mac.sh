#!/bin/bash
cd "$(dirname "$0")"
mkdir osx.10.12-x64
pushd starsky
dotnet publish -c release -r osx.10.12-x64 --output ../osx.10.12-x64
popd
pushd starsky-cli
dotnet publish -c release -r osx.10.12-x64 --output ../osx.10.12-x64
popd
pushd starskyimportercli
dotnet publish -c release -r osx.10.12-x64 --output ../osx.10.12-x64
popd
