#!/bin/bash

# when force everything
# docker system prune -a -f

docker builder prune --filter 'until=8h' -f
docker image prune --filter 'until=8h' -f
docker container prune --filter "until=8h" -f