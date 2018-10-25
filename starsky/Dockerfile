
FROM microsoft/aspnetcore-build:2.0 AS build-env

WORKDIR /app

# copy everything else and build
COPY . ./

WORKDIR /app/starsky
RUN dotnet restore

RUN dotnet publish -c Release -o out



# build runtime image
FROM microsoft/aspnetcore:2.0 AS release

# exiftool is required, nano is optinal
RUN apt-get install libimage-exiftool-perl -y
RUN apt-get install -y nano

WORKDIR /app
COPY --from=build-env /app/starsky/out .
ENTRYPOINT ["dotnet", "starsky.dll"]


