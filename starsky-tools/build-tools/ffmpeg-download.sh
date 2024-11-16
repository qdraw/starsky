#!/bin/bash

FFBINARIES_API="https://ffbinaries.com/api/v1/version/6.1"
OSX_ARM64="https://www.osxexperts.net/ffmpeg71arm.zip"

# Fetch the JSON data
json=$(curl -s $FFBINARIES_API)

# Extract URLs for ffmpeg binaries
urls=$(echo "$json" | grep -o '"ffmpeg":"[^"]*"' | sed 's/"ffmpeg":"//;s/"//')

# Create a directory to store the binaries
mkdir -p ffmpeg_binaries
cd ffmpeg_binaries

# Download each ffmpeg binary
for url in $urls; do
  echo "Downloading $url"
  curl -L -O "$url"
done

curl -O $OSX_ARM64

echo "All ffmpeg binaries downloaded successfully."
