
PUSHD starskyTests
dotnet test /p:hideMigrations=\"true\" /p:CollectCoverage=true /p:CoverletOutputFormat=opencover
dotnet reportgenerator "-reports:coverage.opencover.xml" "-targetdir:coveragereport" -reporttypes:HtmlInline_AzurePipelines
POPD
