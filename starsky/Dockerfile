# Dockerfile

# ARM build test: docker buildx build --push starsky --tag qdraw/starsky:latest --platform linux/arm64

# Stage 1 Front-end
# # docker buildx imagetools inspect node:16-alpine
FROM --platform=$BUILDPLATFORM node:16-alpine as react-build
ARG TARGETPLATFORM
ARG BUILDPLATFORM

WORKDIR /app/starsky/clientapp
ENV CI=true
ENV INLINE_RUNTIME_CHUNK=false
ENV IMAGE_INLINE_SIZE_LIMIT=1
ENV DISABLE_ESLINT_PLUGIN=true
COPY starsky/clientapp/. ./
RUN npm ci
RUN npm run build
RUN echo "react done"

# no alpine build since there is no support for multi-arch
# docker buildx imagetools inspect mcr.microsoft.com/dotnet/sdk:6.0
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:6.0 AS build-env
ARG BUILDPLATFORM

RUN mkdir -p /app
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

RUN if [ "$TARGETPLATFORM" = "linux/amd64" ]; then \
        RID=linux-x64 ; \
    elif [ "$TARGETPLATFORM" = "linux/arm64" ]; then \
        RID=linux-arm64 ; \
    elif [ "$TARGETPLATFORM" = "linux/arm/v7" ]; then \
        RID=linux-arm ; \
    fi \
    && dotnet restore starsky.csproj

RUN if [ "$TARGETPLATFORM" = "linux/amd64" ]; then \
        RID=linux-x64 ; \
    elif [ "$TARGETPLATFORM" = "linux/arm64" ]; then \
        RID=linux-arm64 ; \
    elif [ "$TARGETPLATFORM" = "linux/arm/v7" ]; then \
        RID=linux-arm ; \
    fi \
    && dotnet publish -c release -o out -r $RID --self-contained false


# build runtime image (multiarch)
# docker buildx imagetools inspect mcr.microsoft.com/dotnet/core/aspnet:3.1
FROM mcr.microsoft.com/dotnet/core/aspnet:3.1 AS release

WORKDIR /app
COPY --from=build-env /app/starsky/out .
COPY --from=react-build /app/starsky/clientapp ./clientapp

ENV ASPNETCORE_Environment=Production

CMD ASPNETCORE_URLS=http://*:$PORT dotnet starsky.dll
