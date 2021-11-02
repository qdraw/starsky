# # Stage 1 Front-end
FROM node:16-alpine as react-build
WORKDIR /app/starsky/clientapp
ENV CI=true
ENV INLINE_RUNTIME_CHUNK=false
ENV IMAGE_INLINE_SIZE_LIMIT=1
ENV DISABLE_ESLINT_PLUGIN=true
COPY starsky/clientapp/. ./
RUN npm ci
RUN npm run build
RUN echo "react done"

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-alpine AS build-env

# for alpine builds
RUN apk add --no-cache bash
RUN apk add --no-cache curl

WORKDIR /app

# copy everything else and build
COPY . ./

# insert demo user and content
ARG ISDEMO=""
ENV E_ISDEMO=$ISDEMO
RUN chmod +x docker_demo_setup.sh 
RUN ls
RUN ./docker_demo_setup.sh

WORKDIR /app/starsky
RUN dotnet restore

RUN dotnet publish -c Release -o out

# build runtime image
FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-alpine AS release

# to avoid System.TimeZoneNotFoundException
RUN apk add --no-cache tzdata
RUN apk add --no-cache exiftool

WORKDIR /app
COPY --from=build-env /app/starsky/out .
COPY --from=react-build /app/starsky/clientapp ./clientapp

ENV ASPNETCORE_Environment=Production

CMD ASPNETCORE_URLS=http://*:$PORT dotnet starsky.dll
