#!/bin/bash
cd "$(dirname "$0")"
mkdir win7-x86
pushd starsky
dotnet publish -c release -r win7-x86 --output ../win7-x86
popd
pushd starsky-cli
dotnet publish -c release -r win7-x86 --output ../win7-x86
popd
pushd starskyimportercli
dotnet publish -c release -r win7-x86 --output ../win7-x86
popd
