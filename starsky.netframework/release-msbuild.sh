#!/bin/bash
nuget restore

pushd starskySyncFramework
MSBuild "starskySyncFramework.csproj" /P:Configuration=release
popd


pushd starskyimportercliFramework
MSBuild "starskyimportercliFramework.csproj" /P:Configuration=release
popd
