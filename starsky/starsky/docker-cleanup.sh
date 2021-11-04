#!/bin/bash

docker builder prune --filter 'until=8h' -f
docker image prune --filter 'until=8h' -f
docker container prune --filter "until=8h" -f