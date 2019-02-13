#!/bin/bash
cd "$(dirname "$0")"
mkdir osx.10.12-x64

# First CLI, then MVC, to avoid this error:
# Unhandled Exception: System.IO.FileLoadException: Could not load file or assembly 'Microsoft.AspNetCore.Authentication, Version=2.1.2.0, Culture=neutral, PublicKeyToken=adb9793829ddae60'.
# The located assembly's manifest definition does not match the assembly reference. (Exception from HRESULT: 0x80131040)

pushd starskyimportercli
	dotnet publish --no-dependencies -c release -r osx.10.12-x64 --output ../osx.10.12-x64
popd

pushd starskywebhtmlcli
	dotnet publish --no-dependencies -c release -r osx.10.12-x64 --output ../osx.10.12-x64
popd

pushd starskygeocli
	dotnet publish -c release -r osx.10.12-x64 --output ../osx.10.12-x64
popd

pushd starskywebftpcli
	dotnet publish --no-dependencies -c release -r osx.10.12-x64 --output ../osx.10.12-x64
popd

pushd starskysynccli
	dotnet publish --no-dependencies -c release -r osx.10.12-x64 --output ../osx.10.12-x64
popd

pushd starsky
	dotnet publish -c release -r osx.10.12-x64 --self-contained --output ../osx.10.12-x64
popd
