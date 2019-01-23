#!/bin/bash
cd "$(dirname "$0")"

nuget restore

mkdir starsky.netframework-msbuild

pushd starskySyncNetFramework
MSBuild "starskySyncNetFramework.csproj" /P:Configuration=release
cp -a bin/Release/. ../starsky.netframework-msbuild
popd


pushd starskyimportercliNetFramework
MSBuild "starskyimportercliNetFramework.csproj" /P:Configuration=release
cp -a bin/Release/. ../starsky.netframework-msbuild
popd
