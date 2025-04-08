#!/bin/bash

cd "$(dirname "$0")" # Change to the script's directory

cd ../..
pwd

docker build -t starsky-dropbox-import -f starsky-tools/dropbox-import/Dockerfile . --progress=plain
docker rm -f starsky-dropbox-import
docker run \
  -v ./fotobieb:/mnt/data/sync/fotobieb \
  -p 6590:80 \
  --name starsky-dropbox-import \
  starsky-dropbox-import
