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
# docker buildx imagetools inspect node:20-alpine
FROM --platform=$BUILDPLATFORM node:20-alpine AS react-build
ARG TARGETPLATFORM
ARG BUILDPLATFORM

WORKDIR /app/starsky/clientapp
ENV CI=true
ENV INLINE_RUNTIME_CHUNK=false
ENV IMAGE_INLINE_SIZE_LIMIT=1
ENV DISABLE_ESLINT_PLUGIN=true
COPY starsky/clientapp/. ./
RUN for i in 1 2 3; do \
      npm ci --legacy-peer-deps --prefer-offline --no-audit --no-fund --ignore-scripts && break || sleep 15; \
    done && \
    npm run build && \
    rm -rf node_modules && \
    echo "react done"

# Stage 2 .NET Build
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
ARG BUILDPLATFORM
ARG TARGETPLATFORM
ARG ISDEMO=""
ENV E_ISDEMO=$ISDEMO

RUN mkdir -p /app
WORKDIR /app

# Copy everything else and build
COPY . ./

RUN /bin/bash -c "\
    rm /app/global.json ; \
    if [ \"$TARGETPLATFORM\" = \"linux/amd64\" ]; then \
        echo \"$TARGETPLATFORM\" ; \
        ./build.sh --runtime linux-x64 --no-client; \
    elif [ \"$TARGETPLATFORM\" = \"linux/arm64\" ]; then \
        ./build.sh --runtime linux-arm64 --no-client; \
    elif [ \"$TARGETPLATFORM\" = \"linux/arm/v7\" ]; then \
        ./build.sh --runtime linux-arm --no-client; \
    else \
        dotnet restore starsky.csproj ; \
    fi"

# Stage 3 Testing
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

# Stage 4 Runtime Image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS release
WORKDIR /app
ARG TOOLS=false

RUN if [ "$TOOLS" = "true" ]; then \
    apt-get update ;\
    apt-get upgrade -y ;\
    apt-get install --no-install-recommends --no-install-suggests -y curl ;\
    apt-get clean ;\
    rm -rf /var/lib/apt/lists/* ;\
else \
    echo "no tools for debug" ;\
fi && \
  groupadd --gid 1000 starsky \
    && useradd --uid 1000 --gid starsky --shell /bin/bash --create-home starsky

# Copy dlls
COPY --chown=starsky:starsky --from=build-env /app/starsky/out /app

# Copy dependencies & demo content
COPY --chown=starsky:starsky --from=build-env /app/dependencies /app/dependencies
COPY --chown=starsky:starsky --from=build-env /app/temp /app/temp
COPY --chown=starsky:starsky --from=build-env /app/data.db /app/app__data.db

# Copy react build
COPY --chown=starsky:starsky --from=react-build /app/starsky/clientapp ./clientapp

# Makes sure the test stage is run when using docker buildkit
COPY --chown=starsky:starsky --from=test /testresults /tmp

# Create necessary directories and set ownership
RUN mkdir -p /app/photos && chown starsky:starsky /app/photos
RUN mkdir -p /app/thumbnailTempFolder && chown starsky:starsky /app/thumbnailTempFolder
RUN chown starsky:starsky /app

USER starsky

ENV ASPNETCORE_Environment=Production
ENTRYPOINT ["dotnet", "starsky.dll"]
