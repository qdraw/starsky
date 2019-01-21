#!/bin/bash
cd "$(dirname "$0")"

nuget restore

mkdir msbuild.netframework

pushd starskySyncNetFramework
MSBuild "starskySyncNetFramework.csproj" /P:Configuration=release
cp -a bin/Release/. ../msbuild.netframework
popd


pushd starskyimportercliNetFramework
MSBuild "starskyimportercliNetFramework.csproj" /P:Configuration=release
cp -a bin/Release/. ../msbuild.netframework
popd
