# Pure Docker

We recommend using [Docker Compose](docker-compose.md) because it is easier and provides more convenience for
running multiple services than the [pure Docker command-line interface](https://docs.docker.com/engine/reference/commandline/cli/).
Before you proceed, make sure you have [Docker](https://store.docker.com/search?type=edition&offering=community)
installed on your system. It is available for Mac, Linux, and Windows.

```
docker run -d \
    --name starsky \
    --security-opt seccomp=unconfined \
    --security-opt apparmor=unconfined \
    -p 6470:6470 \
    -e PORT=6470 \
    -e app__databaseType="mysql" \
    -e app__IsAccountRegisterOpen="false" \
    -e app__useDiskWatcher="false" \
    -e app__storageFolder="/app/pictures" \
    -e DOTNET_USE_POLLING_FILE_WATCHER="true" \
    -e DOTNET_HOSTBUILDER__RELOADCONFIGONCHANGE="false" \
    -e app__databaseConnection="Server=host.docker.internal;port=6499;database=starsky_db;uid=starsky_sql;pwd=change__this__password__please_this_unsafe" \
    -v /app/storage \
    -v ~/Pictures:/app/pictures \
    qdraw/starsky
```

See [docker-compose.md](docker-compose.md) for more information