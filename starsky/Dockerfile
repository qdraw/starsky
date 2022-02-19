# Dockerfile

# ARM build test: 
# cd /root_of_repo
# docker buildx build --push starsky --tag qdraw/starsky:latest --platform linux/arm64

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
ARG TARGETPLATFORM

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
        echo $TARGETPLATFORM ; \
        dotnet restore --runtime linux-x64 starsky.csproj ; \
    elif [ "$TARGETPLATFORM" = "linux/arm64" ]; then \
        dotnet restore --runtime linux-arm64 starsky.csproj ; \
    elif [ "$TARGETPLATFORM" = "linux/arm/v7" ]; then \
        dotnet restore --runtime linux-arm starsky.csproj ; \
    else \
        dotnet restore starsky.csproj ; \
    fi

RUN if [ "$TARGETPLATFORM" = "linux/amd64" ]; then \
        echo $TARGETPLATFORM ; \
        dotnet publish -c release -o out --runtime linux-x64 --self-contained false ; \
    elif [ "$TARGETPLATFORM" = "linux/arm64" ]; then \
        dotnet publish -c release -o out --runtime linux-arm64 --self-contained false ; \
    elif [ "$TARGETPLATFORM" = "linux/arm/v7" ]; then \
        dotnet publish -c release -o out --runtime linux-arm --self-contained false ; \
    else \
       dotnet publish -c release -o out  ; \
    fi

# build runtime image (multiarch)
# docker buildx imagetools inspect mcr.microsoft.com/dotnet/aspnet:3.1
FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS release
WORKDIR /app
COPY --from=build-env /app/starsky/out .
COPY --from=react-build /app/starsky/clientapp ./clientapp

ENV ASPNETCORE_Environment=Production

CMD ASPNETCORE_URLS=http://*:$PORT dotnet starsky.dll
