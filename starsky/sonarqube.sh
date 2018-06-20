dotnet /Users/dionvanvelde/Downloads/sonarqube-7.0/sonar-scanner-msbuild-4.1.1.1164-netcoreapp2.0/SonarScanner.MSBuild.dll begin /k:"starsky" /d:sonar.host.url="http://localhost:9000" /d:sonar.login="e23083ae4031b7e01c2045ea5815e1e7a8292a03" /d:sonar.cs.opencover.reportsPaths="/data/git/starsky/starsky/starskyTests/coverage.xml" && dotnet build && dotnet /Users/dionvanvelde/Downloads/sonarqube-7.0/sonar-scanner-msbuild-4.1.1.1164-netcoreapp2.0/SonarScanner.MSBuild.dll end /d:sonar.login="e23083ae4031b7e01c2045ea5815e1e7a8292a03"

# this server is not accesable from outside my computer
# so the passwords are useless!

# without coverage
#dotnet /Users/dionvanvelde/Downloads/sonarqube-7.0/sonar-scanner-msbuild-4.1.1.1164-netcoreapp2.0/SonarScanner.MSBuild.dll begin /k:"starsky" /d:sonar.host.url="http://localhost:9000" /d:sonar.login="e23083ae4031b7e01c2045ea5815e1e7a8292a03" && dotnet build && dotnet /Users/dionvanvelde/Downloads/sonarqube-7.0/sonar-scanner-msbuild-4.1.1.1164-netcoreapp2.0/SonarScanner.MSBuild.dll end /d:sonar.login="e23083ae4031b7e01c2045ea5815e1e7a8292a03"
