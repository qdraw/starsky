#!/bin/bash
cd "$(dirname "$0")"
mkdir win7-x86
pushd starsky
dotnet publish -c release -r win7-x86 --framework netcoreapp2.1 --output ../win7-x86
popd
pushd starskysynccli
dotnet publish -c release -r win7-x86 --framework netcoreapp2.1 --output ../win7-x86
popd
pushd starskyimportercli
dotnet publish -c release -r win7-x86 --framework netcoreapp2.1 --output ../win7-x86
popd
