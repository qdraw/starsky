# Dockerfile server version

# ARM build test: 
# cd /root_of_repo
# docker buildx build --push starsky --tag qdraw/starsky:latest --platform linux/arm64

# docker compose build --progress=plain
# DEBUG Dockerfile:
# export DOCKER_BUILDKIT=0
# cd /root_of_repo
# docker build -f starsky/Dockerfile . --no-cache --build-arg TEST=false

# Stage 1 Front-end
# # docker buildx imagetools inspect node:18-alpine
FROM --platform=$BUILDPLATFORM node:18-alpine AS react-build
ARG TARGETPLATFORM
ARG BUILDPLATFORM

WORKDIR /app/starsky/clientapp
ENV CI=true
ENV INLINE_RUNTIME_CHUNK=false
ENV IMAGE_INLINE_SIZE_LIMIT=1
ENV DISABLE_ESLINT_PLUGIN=true
COPY starsky/clientapp/. ./
# random npm ERR! code ECONNRESET
RUN for i in 1 2 3; do \
      npm ci --legacy-peer-deps --prefer-offline --no-audit --no-fund --ignore-scripts && break || sleep 15; \
    done && \
    npm run build && \
    echo "react done"

# no alpine build since there is no support for multi-arch
# docker buildx imagetools inspect mcr.microsoft.com/dotnet/sdk:6.0
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
ARG BUILDPLATFORM
ARG TARGETPLATFORM
# insert demo user and content
ARG ISDEMO=""
ENV E_ISDEMO=$ISDEMO

RUN mkdir -p /app
WORKDIR /app

# copy everything else and build
COPY . ./

WORKDIR /app/starsky

# there is specific version matched in global.json, but docker uses the latest
RUN rm /app/global.json

RUN \
    if [ "$TARGETPLATFORM" = "linux/amd64" ]; then \
        echo "$TARGETPLATFORM" ; \
        dotnet restore --runtime linux-x64 starsky.csproj ; \
    elif [ "$TARGETPLATFORM" = "linux/arm64" ]; then \
        dotnet restore --runtime linux-arm64 starsky.csproj ; \
    elif [ "$TARGETPLATFORM" = "linux/arm/v7" ]; then \
        dotnet restore --runtime linux-arm starsky.csproj ; \
    else \
        dotnet restore starsky.csproj ; \
    fi \
    if [ "$TARGETPLATFORM" = "linux/amd64" ]; then \
        echo "$TARGETPLATFORM" ; \
        dotnet publish -c release -o out --runtime linux-x64 --self-contained false --no-restore ; \
    elif [ "$TARGETPLATFORM" = "linux/arm64" ]; then \
        dotnet publish -c release -o out --runtime linux-arm64 --self-contained false --no-restore ; \
    elif [ "$TARGETPLATFORM" = "linux/arm/v7" ]; then \
        dotnet publish -c release -o out --runtime linux-arm --self-contained false --no-restore ; \
    else \
       dotnet publish -c release -o out\
    fi

WORKDIR /app

RUN \
    mkdir -p "/app/dependencies" && \
    mkdir -p "/app/temp" && \
    touch /app/temp/.gitkeep && \
    dotnet run \
        --project "/app/starskygeocli/starskygeocli.csproj" \
        --configuration Release \
        -- \
        --connection "Data Source=/app/data.db" \
        --dependencies "/app/dependencies" \
        --tempfolder "/app/temp" \
        -h \
        -v
RUN \
    if [ "$ISDEMO" != "" ]; then \
        # it should cache faster \
        dotnet build starsky/starsky.csproj --configuration Release ;\
        DEMO_SEED_CLI_PATH="/app/starskydemoseedcli/starskydemoseedcli.csproj" ;\
        DEPS_FOLDER="/app/dependencies" ;\
        TEMP_FOLDER="/app/temp" ;\
        mkdir -p "$DEPS_FOLDER" ;\
        mkdir -p "$TEMP_FOLDER" ;\
        dotnet run --project "$DEMO_SEED_CLI_PATH" --configuration Release -- --dependencies "$DEPS_FOLDER" --tempfolder "$TEMP_FOLDER" -h -v ;\
    fi

# no alpine build since there is no support for multi-arch
# docker buildx imagetools inspect mcr.microsoft.com/dotnet/sdk:6.0
FROM --platform=$BUILDPLATFORM build-env AS test
ARG BUILDPLATFORM
ARG TEST=true

WORKDIR /app

RUN if [ "$TEST" = "true" ]; then \
        mkdir -p "/testresults" ;\
        if [ "$BUILDPLATFORM" = "$TARGETPLATFORM" ]; then \
          echo "$TEST" "$BUILDPLATFORM" "$TARGETPLATFORM" ; \
          dotnet test \
              -c release \
              --results-directory /testresults \
              --logger "trx;LogFileName=test_results.trx" \
              --collect:"XPlat Code Coverage" \
              --settings build.vstest.runsettings \
              starskytest/starskytest.csproj ;\
        fi ;\
        touch "/testresults/test.enabled" ;\
    else \
      mkdir -p "/testresults" ;\
      touch "/testresults/test.disabled" ;\
    fi
    
# build runtime image (multiarch)
# docker buildx imagetools inspect mcr.microsoft.com/dotnet/aspnet:6.0
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS release
WORKDIR /app
ARG TOOLS=false

# # for debugging
RUN if [ "$TOOLS" = "true" ]; then \
    apt-get update ;\
    apt-get upgrade -y ;\
    apt-get install --no-install-recommends --no-install-suggests -y curl ;\
    apt-get clean ;\
    rm -rf /var/lib/apt/lists/* ;\
else \
    echo "no tools for debug" ;\
fi

# Create a group and user
RUN groupadd --gid 1000 starsky \
    && useradd --uid 1000 --gid starsky --shell /bin/bash --create-home starsky
    
# copy dlls
COPY --chown=starsky:starsky --from=build-env /app/starsky/out /app

# copy dependencies & demo content
COPY --chown=starsky:starsky --from=build-env /app/dependencies /app/dependencies
COPY --chown=starsky:starsky --from=build-env /app/temp /app/temp
COPY --chown=starsky:starsky --from=build-env /app/data.db /app/app__data.db

# copy react build
COPY --chown=starsky:starsky --from=react-build /app/starsky/clientapp ./clientapp

# Makes sure the test stage is run when using docker buildkit
COPY --chown=starsky:starsky --from=test /testresults /tmp

# you could mount this location
RUN mkdir -p /app/photos && chown starsky:starsky /app/photos
RUN mkdir -p /app/thumbnailTempFolder && chown starsky:starsky /app/thumbnailTempFolder

# and change owner of folder
RUN chown starsky:starsky /app

USER starsky

ENV ASPNETCORE_Environment=Production
ENTRYPOINT ["dotnet", "starsky.dll"]