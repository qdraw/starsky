REM Windows batch file
PUSHD starskytest
dotnet test /p:hideMigrations=\"true\" /p:CollectCoverage=true /p:CoverletOutputFormat=opencover
POPD
