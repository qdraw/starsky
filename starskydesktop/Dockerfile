
# root of repo
#  docker build -f starskyapp/Dockerfile .  --progress=plain

# Stage 1 Front-end
# # docker buildx imagetools inspect node:18-alpine
FROM --platform=$BUILDPLATFORM node:18-alpine as react-build
ARG TARGETPLATFORM
ARG BUILDPLATFORM

RUN mkdir -p /app/starsky/starsky/clientapp
WORKDIR /app/starsky/starsky/clientapp
ENV CI=true
ENV INLINE_RUNTIME_CHUNK=false
ENV IMAGE_INLINE_SIZE_LIMIT=1
ENV DISABLE_ESLINT_PLUGIN=true
COPY starsky/starsky/clientapp/. ./
RUN npm ci --legacy-peer-deps --prefer-offline --no-audit
RUN npm run build
RUN echo "react done"

FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:6.0 AS build-env
ARG BUILDPLATFORM
ARG TARGETPLATFORM

RUN curl -sL https://deb.nodesource.com/setup_18.x | bash - 
RUN apt-get install -y nodejs

WORKDIR /app

COPY starsky ./starsky
COPY starsky-tools/build-tools ./starsky-tools/build-tools

COPY --from=react-build /app/starsky/starsky/clientapp ./starsky/starsky/clientapp

WORKDIR /app/starsky

RUN ./build.sh --runtime osx-x64,win-x64 --no-tests --no-client

FROM --platform=$BUILDPLATFORM electronuserland/builder:wine as setup-wine
ARG TARGETPLATFORM
ARG BUILDPLATFORM

RUN mkdir -p /app/starskyapp
WORKDIR /app/starskyapp

COPY starskyapp/package-lock.json ./package-lock.json 
COPY starskyapp/package.json ./package.json

RUN ls -lah

RUN npm ci

FROM --platform=$BUILDPLATFORM setup-wine as build-wine
ARG BUILDPLATFORM
ARG TARGETPLATFORM

COPY --from=build-env /app/starsky/osx-x64 /app/starsky/osx-x64
COPY --from=build-env /app/starsky/win-x64 /app/starsky/win-x64

RUN mkdir -p /app/starskyapp

WORKDIR /app/starskyapp

COPY starskyapp ./


RUN npm ci

RUN npm run macwindows
