# Troubleshooting MariaDB Problems

> You are welcome to ask for help in our [discussions](https://github.com/qdraw/starsky/discussions) page


## Compatibility

Starsky is compatible with [SQLite 3](sqlite.md) and [MariaDB 10.5.12+](https://mariadb.org/).
Official support for MySQL 8 is discontinued as Oracle seems to have stopped shipping new features and enhancements.
As a result, the testing effort required before each release is no longer feasible.

Our [configuration examples](../docker/docker.md) are usually based on the [current stable version](https://mariadb.com/kb/en/mariadb-server-release-dates/) to take advantage of performance improvements. This does not mean that [older versions](../readme.mdx#databases) are no longer supported and you must upgrade immediately.

We recommend not using the `:latest` tag for the MariaDB Docker image and to upgrade manually by changing the tag once we had a chance to test a new major version, e.g.:

```yaml
services:
  mariadb:
    image: mariadb:10.9
    # ... etc...
```

## Cannot Connect

First, verify that you are using the correct port (default is `3306`) and host:

- in the internal Docker network, the default hostname is `mariadb` (same as the [service](../docker/docker.md))
- avoid changing the default network configuration, unless you are experienced with this
- avoid using IP addresses other than `127.0.0.1` (localhost) directly, as they can change
- only use `localhost` or `127.0.0.1` if the database port [has been exposed](https://docs.docker.com/compose/compose-file/compose-file-v3/#ports) as described below and you are on the same computer (host)
- we recommend [configuring a local hostname](../../assets/getting-started-index-pihole-local-dns.png) to access other hosts on your network

To connect to MariaDB from your host or home network, you need to expose port `3306` in your `docker-compose.yml`
and [restart the service for changes to take effect](../docker/docker-compose.md#step-2-start-the-server):

```yaml
services:
  mariadb:
    ports:
      - "3306:3306"
```

!!! danger ""
    Set strong passwords if the database is exposed to an external network. Never expose your database to the public
    Internet in this way, for example, if it is running on a cloud server.

If this doesn't help, check the [Docker Logs](docker.md#viewing-logs) for messages like *disk full*, *disk quota exceeded*,
*no space left on device*, *read-only file system*, *error creating path*, *wrong permissions*, *no route to host*, *connection failed*, *exec format error*,
*no matching manifest*, and *killed*:

- [ ] Make sure that the database *storage* folder is readable and writable: Errors such as "read-only file system", "error creating path", or "wrong permissions" indicate a [filesystem permission problem](docker.md#file-permissions)
- [ ] If [symbolic links](https://en.wikipedia.org/wiki/Symbolic_link) are mounted or used within the *storage* folder, replace them with the actual paths and verify that they are accessible
- [ ] If the MariaDB service has been "killed" or otherwise automatically terminated, this can point to a [memory problem](docker.md#adding-swap) (add swap and/or memory; remove or increase usage limits)
- [ ] In case the logs also show "disk full", "quota exceeded", or "no space left" errors, either [the disk containing the *storage* folder is full](docker.md#disk-space) (add storage) or a disk usage limit is configured (remove or increase it)
- [ ] Log messages that contain "no route to host" may also indicate a general network configuration problem (follow our [examples](../docker/docker.md))
- [ ] You have to resort to [alternative Docker images](../raspberry-pi.md#older-armv7-based-devices) to run MariaDB on ARMv7-based devices and those with a 32-bit operating system

## Bad Performance

Many users reporting poor performance and high CPU usage have migrated from SQLite to MariaDB, so their database schema is no longer optimized for performance. For example, MariaDB cannot handle rows with `text` columns in memory and always uses temporary tables on disk if there are any.

The instructions for these migrations were provided by a contributor and are not part of the original software distribution. As such, they have not been officially released, recommended, or extensively tested by us.

If this is the case, please make sure that your migrated database schema matches that of a fresh, non-migrated installation.

[Get Performance Tips ›](performance.md#mariadb)

## Version Upgrade

If MariaDB fails to start after upgrading from an earlier version (or migrating from MySQL), the [internal management schema](https://mariadb.com/kb/en/understanding-mariadb-architecture/#system-databases) may be outdated. With older versions, it could only be updated manually.
However, newer MariaDB Docker images **support automatic upgrades** on startup, so you don't have to worry about that anymore.

### Manual Update

To manually upgrade the internal database schema, run this command in a terminal:

```bash
docker compose exec mariadb mariadb-upgrade -uroot -p
```

Enter the MariaDB "root" password specified in your `docker-compose.yml` when prompted.


### Auto Upgrade

To enable automatic schema updates, set `MARIADB_AUTO_UPGRADE` to a non-empty value in your `docker-compose.yml` as shown in our [config example](../docker/docker.md):

```yaml
services:
  mariadb:
    image: mariadb:10.9
    ...
    environment:
      MARIADB_AUTO_UPGRADE: "1"
      MARIADB_INITDB_SKIP_TZINFO: "1"
      ...
```

Before starting MariaDB in production mode, the database image entrypoint script now runs `mariadb-upgrade` to update the internal management schema as needed. For example, when you pull a new major release and restart the service.

!!! tldr ""
    Since Starsky does not require time zone support, you can also add `MARIADB_INITDB_SKIP_TZINFO` to your config as shown above. However, this is only a recommendation and optional.

## Incompatible Schema

If your database does not seem to be compatible with the currently installed version of Starsky, for example because search results are missing or incorrect, first make sure you are using a [supported database](../readme.mdx#databases) and that its internal management schema is up-to-date. How to do that is explained in the [previous section](#version-upgrade).


### Complete Rescan

We recommend that you **re-index your pictures after a schema migration**, especially if problems persist. You can either start a rescan from the user interface by navigating to *More* > *Manual Sync*, click "Manual Sync", or by running this command in a terminal:

```bash
docker compose exec /app/starskysynchronizecli -s
```

> **TL;DR**<br />
    Be careful not to start multiple indexing processes at the same time, as this will lead to a high server load.

## Server Crashes

If the server crashes unexpectedly or your database files get corrupted frequently, it is usually because they are stored on an unreliable device such as a USB flash drive, an SD card, or a shared network folder mounted via NFS or CIFS. These may also have [unexpected file size limitations](https://thegeekpage.com/fix-the-file-size-exceeds-the-limit-allowed-and-cannot-be-saved/), which is especially problematic for databases that do not split data into smaller files.

- [ ] Never use the same database files with more than one server instance
- [ ] To share a database over a network, run the database server directly on the remote server instead of sharing database files
- [ ] To repair your tables after you have moved the files to a local disk, you can [start MariaDB with `--innodb-force-recovery=1`](https://mariadb.com/kb/en/innodb-recovery-modes/) (otherwise the same procedure as for recovering a lost password, see above)
- [ ] Make sure you are using the latest Docker version and read the release notes for the database server version you are using

## Corrupted Files

↪ [Server Crashes](#server-crashes)

## Lost Root Password

In case you forgot the MariaDB "root" password and the one specified in your configuration does not work,
you can [start the server with the `--skip-grant-tables` flag](https://mariadb.com/docs/reference/mdb/cli/mariadbd/skip-grant-tables/)
added to the `mysqld` command in your `docker-compose.yml`. This will temporarily give full access
to all users after a restart:

```yaml
services:
  mariadb:
    command: mysqld --skip-grant-tables
```

Restart the `mariadb` service for changes to take effect:

```bash
docker compose stop mariadb
docker compose up -d mariadb
```

Now open a database console:

```bash
docker compose exec mariadb mysql -uroot
```

Enter the following commands to change the password for "root":

```sql
FLUSH PRIVILEGES;
ALTER USER 'root'@'%' IDENTIFIED BY 'new_password';
ALTER USER 'root'@'localhost' IDENTIFIED BY 'new_password';
UPDATE mysql.user SET authentication_string = '' WHERE user = 'root';
UPDATE mysql.user SET plugin = '' WHERE user = 'root';
exit
```

When you are done, remove the `--skip-grant-tables` flag again to restore the original
command and restart the `mariadb` service as described above.

## Server Relocation

When moving MariaDB to another computer, cloud server, or virtual machine:

- [ ] Move the complete *storage* folder along with it and preserve the [file permissions](../docker/docker.md#file-permissions)
- [ ] **or** restore your index [from an SQL dump](https://mariadb.com/kb/en/mysqldump/) (backup file)
- [ ] Perform a [version upgrade](#version-upgrade) if necessary
- [ ] Make sure that Starsky can access the database on the new host
- [ ] Set strong passwords if the database is exposed to an external network
- [ ] Never expose your database to the public Internet

## Unicode Support

If the logs show "incorrect string value" database errors and you are running a custom MariaDB or MySQL
server that is not based on our [default configuration](../docker/docker.md):

- [ ] Full [Unicode](https://home.unicode.org/basic-info/faq/) support [must be enabled](https://mariadb.com/kb/en/setting-character-sets-and-collations/#example-changing-the-default-character-set-to-utf-8), e.g. using the `mysqld` command parameters `--character-set-server=utf8mb4` and `--collation-server=utf8mb4_unicode_ci`
- [ ] Note that an existing database may use a different character set if you imported it from another server
- [ ] Before submitting a support request, verify the problem still occurs with a newly created database based on our example

Run this command in a terminal to see the current values of the collation and character set variables (change the root
password `insecure` and database name `starsky` as specified in your `docker-compose.yml`):

```bash
echo "SHOW VARIABLES WHERE Variable_name LIKE 'character\_set\_%' OR Variable_name LIKE 'collation%';" | \
docker compose exec -T mariadb mysql -uroot -pinsecure starsky
```

## MySQL Errors

Official [support for MySQL 8 is discontinued](../readme.mdx#databases) as Oracle seems to have stopped shipping new features and enhancements.
As a result, the testing effort required before each release is no longer feasible.

*[SQLite]: self-contained, serverless SQL database