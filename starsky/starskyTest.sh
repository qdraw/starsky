#!/bin/bash
cd "$(dirname "$0")"
pushd starskytest
dotnet test /p:hideMigrations=\"true\" /p:CollectCoverage=true /p:CoverletOutputFormat=opencover
popd
