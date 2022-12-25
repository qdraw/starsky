# Setup Using Docker Compose

With [Docker Compose](https://docs.docker.com/compose/), you [use a YAML file](../developer-guide/technologies/yaml.md)
to configure all application services so you can easily start them with a single command.
Before you proceed, make sure you have [Docker](https://store.docker.com/search?type=edition&offering=community)
installed on your system. It is available for Mac, Linux, and Windows.

## You also could use the application without Docker or Docker compose
Docker is one way of using the application, it also possible to run it without Docker or Docker Compose.

## Step 1 Configure

Download our [docker-compose.yml](https://raw.githubusercontent.com/qdraw/starsky/master/starsky/docker/compose/generic/docker-compose.yml) example
(right click and *Save Link As...* or use `wget`) to a folder of your choice,
and change the [configuration](config-options.md) as needed:

```bash
wget https://raw.githubusercontent.com/qdraw/starsky/master/starsky/docker/compose/generic/docker-compose.yml
``` 

Commands on Linux may have to be prefixed with `sudo` when not running as root.
Note that this will point the home directory shortcut `~` to `/root` in the `volumes:` 
section of your `docker-compose.yml`. Kernel security modules such as AppArmor and SELinux 
have been [reported to cause issues](troubleshooting/docker.md#kernel-security).
Ensure that your server has [at least 4 GB of swap](troubleshooting/docker.md#adding-swap) configured so that
indexing doesn't cause restarts when there are memory usage spikes.


#### Database ####

Our example includes a pre-configured [MariaDB](https://mariadb.com/) database server. If you remove it
and provide no other database server credentials, SQLite database files will be created in the
*storage* folder. Local [SSD storage is best](troubleshooting/performance.md#storage) for databases of any kind.

Never [store database files](troubleshooting/mariadb.md#corrupted-files) on an unreliable device such as a USB flash drive, SD card, or shared network folder. These may also have [unexpected file size limitations](https://thegeekpage.com/fix-the-file-size-exceeds-the-limit-allowed-and-cannot-be-saved/), which is especially problematic for databases that do not split data into smaller files.

> **TL;DR**<br />
It is not possible to change the password via `MARIADB_PASSWORD` after the database has been started
for the first time. Choosing a secure password is not essential if you don't [expose the database to other apps and hosts](troubleshooting/mariadb.md#cannot-connect).
To enable [automatic schema updates](troubleshooting/mariadb.md#auto-upgrade) after upgrading to a new major version, set `MARIADB_AUTO_UPGRADE` to a non-empty value in your `docker-compose.yml`.

#### Volumes ####

Since the app is running inside a container, you have to explicitly [mount the host folders](https://docs.docker.com/compose/compose-file/compose-file-v3/#volumes) you want to use.
PhotoPrism won't be able to see folders that have not been mounted. That's an important security feature.

##### /app/storageFolder #####

The *storageFolder* folder contains your original photo and video files.

`~/Pictures` will be mounted by default, where `~` is a shortcut for your home directory:
e.g. /home/username on Linux, /Users/username on Mac, or C:\Users\username on Windows.

```yaml
volumes:
  # "/host/folder:/app/storageFolder"  # example
  - "~/Pictures:/app/storageFolder"
```

You may [mount any folder accessible from the host](https://docs.docker.com/compose/compose-file/compose-file-v3/#short-syntax-3)
instead, including network drives. Additional directories can
be mounted as subfolders of `/app/storageFolder`:

```yaml
volumes:
  - "/home/username/Pictures:/app/storageFolder"
  - "/example/friends:/app/storageFolder/friends"
  - "/mnt/photos:/app/storageFolder/media"
```

On Windows, prefix the host path with the drive letter and use `/` instead of `\` as separator:

```yaml
volumes:
  - "D:/Example/Pictures:/app/storageFolder"
```

> **TL;DR**<br />
If *read-only mode* is enabled, all features that require write permission to the *originals/storageFolder* folder
are disabled, uploading and deleting files. Set `PHOTOPRISM_READONLY` to `"true"`
in `docker-compose.yml` for this. You can [mount a folder with the `:ro` flag](https://docs.docker.com/compose/compose-file/compose-file-v3/#short-syntax-3) to make Docker block
write operations as well.

##### /app/thumbnailTempFolder #####

Thumbnails files are created in the *thumbnailTempFolder* folder:

- a *storage* folder mount must always be configured in your `docker-compose.yml` file so that you do not lose these files after a restart or upgrade
- never configure the *thumbnailTempFolder* folder to be inside the *thumbnailTempFolder* folder unless the name starts with a `.` to indicate that it is hidden
- we recommend placing the *thumbnailTempFolder* folder on a [local SSD drive](troubleshooting/performance.md#storage) for best performance
- mounting [symbolic links](https://en.wikipedia.org/wiki/Symbolic_link) or using them inside the *thumbnailTempFolder* folder is currently not supported

> **TL;DR**<br />
Should you later want to move your instance to another host, the easiest and most time-saving way is to copy the entire *storage* folder along with your originals and database.

##### import #####

At the moment we don't have a import folder for docker, but you can use the CLI to import files or use web upload

Import in a structured way that avoids duplicates:

- imported files receive a canonical filename and will be organized by year and month
- never configure the *import* folder to be inside the *originals* folder, as this will cause a loop by importing already indexed files

### Step 2: Start the server ###

Open a terminal and change to the folder in which the `docker-compose.yml` file has been saved.
Run this command to start the application and database services in the background:

```bash
docker compose up -d
```

*Note that our guides now use the new `docker compose` command by default. If your server does not yet support it, you can still use `docker-compose`.*

Now open the Web UI by navigating to http://localhost:6470/. You should see a registration screen.
You may change it on the [account settings page](../features/accountmanagement.md).
Enabling [public mode](config-options.md) will disable authentication.

> **Info**<br />
    It can be helpful to [keep Docker running in the foreground while debugging](troubleshooting/docker.md#viewing-logs) so that log messages are displayed directly. To do this, omit the `-d` parameter when restarting.
    Should the server already be running, or you see no errors, you may have started it
    on a different host and/or port. There could also be an [issue with your browser,
    ad blocker, or firewall settings](troubleshooting/index.md#connection-fails).


The server port and other [config options](config-options.md) can be changed in `docker-compose.yml` at any time.
Remember to restart the services for changes to take effect:

```bash
docker compose stop
docker compose up -d
```

### Step 3: Index Your Library ###

Our [First Steps 👣](first-steps.md) tutorial guides you through the user interface and settings to ensure your library is indexed according to your individual preferences.

> **Note**<br />
    Ensure [there is enough disk space available](troubleshooting/docker.md#disk-space) for creating thumbnails and [verify filesystem permissions](troubleshooting/docker.md#file-permissions)
    before starting to index: Files in the *originals* folder must be readable, while the *storage* folder
    including all subdirectories must be readable and writeable.

Open the Web UI, go to *More* and click *Manual Sync* to start indexing your pictures.

Easy, isn't it?

### Troubleshooting ###

If your server runs out of memory, the index is frequently locked, or other system resources are running low:

- [ ] Try [reducing the number of workers](config-options.md#index-workers) by setting `app__maxDegreesOfParallelism` to a reasonably small value in `docker-compose.yml`, depending on the CPU performance and number of cores
- [ ] Make sure [your server has at least 4 GB of swap space](troubleshooting/docker.md#adding-swap) so that indexing doesn't cause restarts when memory usage spikes; RAW image conversion and video transcoding are especially demanding
- [ ] If you are using SQLite, switch to MariaDB, which is better optimized for high concurrency

Other issues? Our [troubleshooting checklists](troubleshooting/index.md) help you quickly diagnose and solve them.




