#!/bin/bash


#dotnet test starskyTests/starskytests.csproj /p:CollectCoverage=true /p:CoverletOutputFormat=opencover

# dotnet build-server shutdown
# dotnet sonarscanner begin /k:"starsky" /d:sonar.host.url=http://localhost:9000 /d:sonar.login="e23083ae4031b7e01c2045ea5815e1e7a8292a03" /d:sonar.cs.opencover.reportsPaths="starskyTests\coverage.opencover.xml" /d:sonar.coverage.exclusions="**Tests*.cs"
# dotnet build /p:hideMigrations=\"true\"
# dotnet sonarscanner end /d:sonar.login="e23083ae4031b7e01c2045ea5815e1e7a8292a03"



# # # For .net Core 2.0
# dotnet /Applications/sonarqube/sonar-scanner-msbuild-4.1.1.1164-netcoreapp2.0/SonarScanner.MSBuild.dll begin /k:"starsky" /d:sonar.host.url="http://localhost:9000" /d:sonar.login="e23083ae4031b7e01c2045ea5815e1e7a8292a03" /d:sonar.cs.opencover.reportsPaths="/data/git/starsky/starsky/starskyTests/coverage.opencover.xml" && dotnet build --framework netstandard2.0 && dotnet /Applications/sonarqube/sonar-scanner-msbuild-4.1.1.1164-netcoreapp2.0/SonarScanner.MSBuild.dll end /d:sonar.login="e23083ae4031b7e01c2045ea5815e1e7a8292a03"

# this server is not accesable from outside my computer
# so the passwords are useless!


# For .net Core 2.1 

#dotnet tool install --global dotnet-sonarscanner
dotnet sonarscanner begin /k:"starsky" /d:sonar.host.url="http://localhost:9000" /d:sonar.login="e23083ae4031b7e01c2045ea5815e1e7a8292a03" /d:sonar.cs.opencover.reportsPaths="/data/git/starsky/starsky/starskyTests/coverage.opencover.xml"
dotnet build /p:hideMigrations=\"true\"
dotnet sonarscanner end /d:sonar.login="e23083ae4031b7e01c2045ea5815e1e7a8292a03"
