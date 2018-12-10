#!/bin/bash
cd "$(dirname "$0")"
pushd starskyTests
dotnet test /p:hideMigrations=\"true\" /p:CollectCoverage=true /p:CoverletOutputFormat=opencover
#| tee readme.coverage.bak
#bash readme-listoftests-update.sh
#bash readme-coverage-update.sh
dotnet reportgenerator "-reports:coverage.opencover.xml" "-targetdir:coveragereport" -reporttypes:HtmlInline_AzurePipelines
popd
