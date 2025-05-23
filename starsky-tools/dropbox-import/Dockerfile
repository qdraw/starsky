# run from root of repo

FROM node:18-alpine AS dropbox-import
WORKDIR /opt/starsky/starsky-tools/dropbox-import

# Copy dropbox-import files
COPY starsky-tools/dropbox-import/ ./

RUN npm ci

# Set environment variables for Dropbox
ENV STARSKYIMPORTERCLI=/opt/starsky/starskyimportercli

# Stage 2: .NET SDK for starskyimportercli
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
WORKDIR /opt/starsky

# Copy .NET project files
COPY starsky/ /opt/starsky/starsky

# Build starskyimportercli
WORKDIR /opt/starsky/starsky

# to avoid version mismatch
RUN rm /opt/starsky/starsky/global.json || true

RUN dotnet publish starskyimportercli/starskyimportercli.csproj -c Release -o /opt/starsky/starskyimportercli

# Stage 3: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS aspnet
WORKDIR /opt/starsky

# Install Node.js
RUN apt-get update && apt-get install -y nodejs npm --no-install-recommends && rm -rf /var/lib/apt/lists/*

RUN node -v

# Copy built .NET project
COPY --from=build-env /opt/starsky/starskyimportercli ./starsky

RUN ./starsky/starskyimportercli -h

# Copy dropbox-import files
COPY --from=dropbox-import /opt/starsky/starsky-tools/dropbox-import ./starsky-tools/dropbox-import

RUN mkdir -p /opt/starsky/starsky-tools/dropbox-import/temp && chmod 777 /opt/starsky/starsky-tools/dropbox-import/temp
RUN chmod +x /opt/starsky/starsky/dependencies/exiftool-unix/exiftool && chown 1000:1000 /opt/starsky/starsky/dependencies/exiftool-unix/exiftool
RUN mkdir -p /opt/starsky/starsky/dependencies && chmod 777 /opt/starsky/starsky/dependencies
RUN mkdir -p /opt/starsky/starsky/temp && chmod 777 /opt/starsky/starsky/temp

# Set environment variables
ENV STARSKYIMPORTERCLI=/opt/starsky/starsky/starskyimportercli

# Default command to run the required scripts
CMD ["tail", "-f", "/dev/null"]