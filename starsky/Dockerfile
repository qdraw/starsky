# Stage 1
FROM node:14 as react-build
WORKDIR /app/starsky/clientapp
COPY starsky/clientapp/. ./
RUN npm ci
RUN npm run build
RUN echo "react done"

FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build-env

WORKDIR /app

# copy everything else and build
COPY . ./

WORKDIR /app/starsky
RUN dotnet restore

RUN dotnet publish -c Release -o out


# build runtime image
FROM mcr.microsoft.com/dotnet/core/aspnet:3.1 AS release
#RUN apt-get install -y nano

WORKDIR /app
COPY --from=build-env /app/starsky/out .
COPY --from=react-build /app/starsky/clientapp ./clientapp

ENV ASPNETCORE_Environment=Production

#ENTRYPOINT ["dotnet", "starsky.dll"]
CMD ASPNETCORE_URLS=http://*:$PORT dotnet starsky.dll
