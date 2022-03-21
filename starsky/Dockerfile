# Dockerfile

# ARM build test: 
# cd /root_of_repo
# docker buildx build --push starsky --tag qdraw/starsky:latest --platform linux/arm64

# docker compose build --progress=plain
# DEBUG Dockerfile:
# export DOCKER_BUILDKIT=0
# cd /root_of_repo
# docker build -f starsky/Dockerfile . --no-cache --build-arg TEST=false

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

RUN if [ "$ISDEMO" != "" ]; then \
        chmod +x docker_demo_setup.sh ;\
        ./docker_demo_setup.sh ;\
    else \
        # it should cache faster
        GEO_CLI_PATH="/app/starskygeocli/starskygeocli.csproj" ;\
        TEMP_FOLDER="/app/out/temp" ;\
        mkdir -p $TEMP_FOLDER ;\
        dotnet run --project $GEO_CLI_PATH --configuration Release -- --tempfolder $TEMP_FOLDER -h -v ;\
    fi

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
 
# no alpine build since there is no support for multi-arch
# docker buildx imagetools inspect mcr.microsoft.com/dotnet/sdk:6.0
FROM --platform=$BUILDPLATFORM build-env AS test
ARG BUILDPLATFORM
ARG TEST=true

WORKDIR /app

RUN if [ "$TEST" = "true" ]; then \
        echo $TEST ; \
        mkdir -p "/testresults" ;\
        dotnet test -c release --results-directory /testresults --logger "trx;LogFileName=test_results.trx" --collect:"XPlat Code Coverage" --settings build.vstest.runsettings starskytest/starskytest.csproj ; \
        touch "/testresults/test.enabled" ;\
    else \
      mkdir -p "/testresults" ;\
      touch "/testresults/test.disabled" ;\
    fi
    
# build runtime image (multiarch)
# docker buildx imagetools inspect mcr.microsoft.com/dotnet/aspnet:3.1
FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS release
WORKDIR /app
COPY --from=build-env /app/starsky/out .
COPY --from=react-build /app/starsky/clientapp ./clientapp

# Makes sure the test stage is run when using docker buildkit
COPY --from=test /testresults /tmp

ENV ASPNETCORE_Environment=Production

CMD ASPNETCORE_URLS=http://*:$PORT dotnet starsky.dll
