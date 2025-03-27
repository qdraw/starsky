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
# docker buildx imagetools inspect node:22-alpine
FROM --platform=$BUILDPLATFORM node:22-alpine AS react-build
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

RUN mkdir -p /app
WORKDIR /app

# Copy everything else and build
COPY . ./

RUN /bin/bash -c "\
    echo \"TARGETPLATFORM=$TARGETPLATFORM\" ; \
    rm /app/global.json ; \
    chmod +x ./build.sh ; \
    mkdir -p /documentation ; \
    echo '<Project Sdk=\"Microsoft.NET.Sdk\"><PropertyGroup><OutputType>Exe</OutputType><ImplicitUsings>enable</ImplicitUsings><TargetFramework>net8.0</TargetFramework></PropertyGroup></Project>' > /documentation/documentation.csproj ; \
    echo '<?xml version=\"1.0\" encoding=\"utf-8\"?><Project ToolsVersion=\"15.0\" Sdk=\"Microsoft.Docker.Sdk\"></Project>' > /app/docker-compose.dcproj ; \
    echo 'Console.WriteLine(\"test\");' > /documentation/Program.cs ; \

    if [ \"$TARGETPLATFORM\" = \"linux/amd64\" ]; then \
        ./build.sh --runtime linux-x64 --no-client --no-sonar --no-test --static-output-folder; \
    elif [ \"$TARGETPLATFORM\" = \"linux/arm64\" ]; then \
        ./build.sh --runtime linux-arm64 --no-client --no-sonar --no-test --static-output-folder; \
    elif [ \"$TARGETPLATFORM\" = \"linux/arm/v7\" ]; then \
        ./build.sh --runtime linux-arm --no-client --no-sonar --no-test --static-output-folder; \
    else \
        ./build.sh --no-client --no-sonar --no-test; \
    fi"

# Stage 3 Runtime Image
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
COPY --chown=starsky:starsky --from=build-env /app/starsky/output /app

# Copy react build
COPY --chown=starsky:starsky --from=react-build /app/starsky/clientapp ./clientapp

# Create necessary directories and set ownership
RUN mkdir -p /app/photos && chown starsky:starsky /app/photos
RUN mkdir -p /app/thumbnailTempFolder && chown starsky:starsky /app/thumbnailTempFolder
RUN chown starsky:starsky /app

USER starsky

ENV ASPNETCORE_Environment=Production
ENTRYPOINT ["dotnet", "starsky.dll"]
