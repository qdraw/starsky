#!/bin/bash
nuget restore

pushd starskySyncNetFramework
MSBuild "starskySyncNetFramework.csproj" /P:Configuration=release
popd


pushd starskyimportercliFramework
MSBuild "starskyimportercliFramework.csproj" /P:Configuration=release
popd
