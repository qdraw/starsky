# Pure Docker

We recommend using [Docker Compose](docker-compose.md) because it is easier and provides more convenience for
running multiple services than the [pure Docker command-line interface](https://docs.docker.com/engine/reference/commandline/cli/).
Before you proceed, make sure you have [Docker](https://store.docker.com/search?type=edition&offering=community)
installed on your system. It is available for Mac, Linux, and Windows.

## Play With Docker
If you want to try Docker, I highly recommend to give [Play With Docker](https://labs.play-with-docker.com/) a try. 
Also known as PWD, it’s an online playground where you can test all the latest Docker features 
without having to install anything locally. Once in PWD, you can create an instance, 
and you’ll feel as if you’re in the shell of a [Linux VM](https://www.linux.com/learn/why-when-and-how-use-virtual-machine).

# Locations for Docker images

- `qdraw/starsky:latest` is the image name, you can find it on [Docker Hub](https://hub.docker.com/r/qdraw/starsky)
- `ghcr.io/qdraw/starsky:latest` is the latest same version on [GitHub Container Registry](https://github.com/qdraw/starsky/pkgs/container/starsky/versions?filters%5Bversion_type%5D=tagged)

## By version
On Docker Hub you can find tags by version number, for example `qdraw/starsky:v0.5.6`
or on [GitHub Container Registry](https://github.com/qdraw/starsky/pkgs/container/starsky/versions?filters%5Bversion_type%5D=tagged)
This is on GitHub Container Registry: `ghcr.io/qdraw/starsky:v0.5.7-beta.0`

## Unstable test version:
- `ghcr.io/qdraw/starsky:master` is the latest unstable image from the master branch, you can find it on GitHub Container Registry

## Run Docker image with MySQL

Scroll below for example with SQLite, the snippet below is for MariaDB/MySQL.

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
    --add-host=host.docker.internal:host-gateway \
    -e app__databaseConnection="Server=host.docker.internal;port=6499;database=starsky_db;uid=starsky_sql;pwd=change__this__password__please_this_unsafe" \
    -v /app/storage \
    -v ~/Pictures:/app/pictures \
    qdraw/starsky
```

### Next install mysql/MariaDb

Run the following command to run MariaDb in Docker
Make sure you have **replaced** the root and user password

```
docker run -d \
  --name=mariadb \
  --add-host=host.docker.internal:host-gateway \
  -e PUID=1000 \
  -e PGID=1000 \
  -e MYSQL_ROOT_PASSWORD=change__this__root__password__please_this_unsafe \
  -e TZ=UTC \
  -e MYSQL_DATABASE=starsky_db `#optional` \
  -e MYSQL_USER=starsky_sql `#optional` \
  -e MYSQL_PASSWORD=change__this__password__please_this_unsafe `#optional` \
  -p 6499:3306 \
  -v /opt/mariadb:/config \
  --restart unless-stopped \
  lscr.io/linuxserver/mariadb
```  

### (Optional) Next install phpmyadmin

```
docker run -d \
  --name=phpmyadmin \
  --add-host=host.docker.internal:host-gateway \
  -e PUID=1000 \
  -e PGID=1000 \
  -e TZ=UTC \
  -e PMA_HOST=host.docker.internal \
  -e PMA_PORT=6499 \
  -e MYSQL_ROOT_PASSWORD=change__this__root__password__please_this_unsafe \
  -e PMA_ARBITRARY=0 `#optional` \
  -p 10859:80 \
  -v /opt/phpmyadmin:/config \
  --restart unless-stopped \
  lscr.io/linuxserver/phpmyadmin:latest
```

## restore

To restore a existing database run the following command:
Read more about this in the MariaDB documentation

```
docker exec -i mariadb mysql -uroot -pchange__this__root__password__please_this_unsafe starsky < $HOME/__starsky__temp.sql
```

:: Note
    Scroll below for more information about docker commands you can use.

## Run Docker image with SQLite

```
docker run -d \
--name starsky \
--security-opt seccomp=unconfined \
--security-opt apparmor=unconfined \
-p 6470:6470 \
-e PORT=6470 \
-e app__databaseType="sqlite" \
-e app__IsAccountRegisterOpen="false" \
-e app__useDiskWatcher="false" \
-e app__storageFolder="/app/pictures" \
-e DOTNET_USE_POLLING_FILE_WATCHER="true" \
-e DOTNET_HOSTBUILDER__RELOADCONFIGONCHANGE="false" \
-v /app/storage \
-v /Pictures:/app/pictures \
qdraw/starsky
```


## Logging

View logging:

```
docker logs -f starsky
```

## Stop

```
docker stop starsky
```

## remove

```
docker rm -f starsky
```


See [docker-compose.md](docker-compose.md) for more information