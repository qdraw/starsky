#!/bin/bash
cd "$(dirname "$0")"
pushd starskyTests
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover
popd
