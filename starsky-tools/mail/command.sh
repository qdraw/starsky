#!/bin/bash

cd "$(dirname "$0")" 

cd ../..
pwd

docker build -t starsky-mail -f starsky-tools/mail/Dockerfile . --progress=plain
docker rm -f starsky-mail
docker run \
  --name starsky-mail \
  starsky-mail
