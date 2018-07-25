#!/bin/bash
cd "$(dirname "$0")"
pushd starskyTests
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover
#| tee readme.coverage.bak
bash readme-listoftests-update.sh
#bash readme-coverage-update.sh

popd
