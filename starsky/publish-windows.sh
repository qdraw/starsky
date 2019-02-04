#!/bin/bash
cd "$(dirname "$0")"
mkdir win7-x86

# First CLI, then MVC

pushd starskysynccli
	dotnet publish -c release -r win7-x86 --framework netcoreapp2.1 --no-dependencies --output ../win7-x86
popd

pushd starskyimportercli
	dotnet publish -c release -r win7-x86 --framework netcoreapp2.1 --no-dependencies --output ../win7-x86
popd

pushd starskywebhtmlcli
	dotnet publish -c release -r win7-x86 --framework netcoreapp2.1 --output ../win7-x86
popd

pushd starskywebftpcli
	dotnet publish --no-dependencies -c release -r win7-x86 --output ../win7-x86
popd

pushd starskygeocli
	dotnet publish -c release -r win7-x86 --framework netcoreapp2.1 --output ../win7-x86
popd

pushd starsky
	dotnet publish -c release -r win7-x86 --framework netcoreapp2.1 --output ../win7-x86
popd
