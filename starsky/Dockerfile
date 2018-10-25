FROM microsoft/dotnet:sdk AS build-env
WORKDIR /app
RUN mkdir /app/starsky

# Copy csproj and restore as distinct layers
COPY starskyimportercli/*.csproj ./starskyimportercli
COPY starskywebhtmlcli/*.csproj ./starskywebhtmlcli
COPY starskygeosync/*.csproj ./starskygeosync
COPY starsky.corehelpers/*.csproj ./starsky.corehelpers
COPY starsky.metacache/*.csproj ./starsky.metacache
COPY starsky.meta/*.csproj ./starsky.meta
COPY starskyTests/*.csproj ./starskyTests
COPY starsky.models/*.csproj ./starsky.models
COPY starsky/*.csproj ./starsky

RUN ls /app/starsky.meta
RUN ls /app/starsky.meta
WORKDIR /app/starsky
RUN dotnet restore

# Copy everything else and build
COPY . ./
RUN dotnet publish -c Release -o out

# Build runtime image
FROM microsoft/dotnet:aspnetcore-runtime
WORKDIR /starsky/starsky/
COPY --from=build-env /app/out .
ENTRYPOINT ["dotnet", "starsky.dll"]
