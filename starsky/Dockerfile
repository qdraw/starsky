# # Stage 1 Front-end
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

# insert demo user and content
ARG ISDEMO=""
ENV E_ISDEMO=$ISDEMO
RUN chmod +x /app/docker_demo_setup.sh && /app/docker_demo_setup.sh

WORKDIR /app/starsky
RUN dotnet restore

RUN dotnet publish -c Release -o out

# build runtime image
FROM mcr.microsoft.com/dotnet/core/aspnet:3.1 AS release

WORKDIR /app
COPY --from=build-env /app/starsky/out .
COPY --from=react-build /app/starsky/clientapp ./clientapp

ENV ASPNETCORE_Environment=Production

CMD ASPNETCORE_URLS=http://*:$PORT dotnet starsky.dll
