# Starsky
## List of [Starsky](../readme.md) Projects
 * __[inotify-settings](../inotify-settings/readme.md) to setup auto indexing on linux__
 * [starsky (sln)](../starsky/readme.md) _database photo index & import index project_
    * [starsky](../starsky/starsky/readme.md)  _mvc application / web interface_
    * [starskySyncCli](../starsky/starskysynccli/readme.md)  _database command line interface_
    * [starskyImporterCli](../starsky/starskyimportercli/readme.md)  _import command line interface_
    * [starskyGeoCli](../starsky/starskygeocli/readme.md)  _gpx sync and reverse 'geo tagging'_
    * [starskyWebHtmlCli](../starsky/starskywebhtmlcli/readme.md)  _publish web images to a content package_
    * [starskyWebFtpCli](../starsky/starskywebftpcli/readme.md)  _copy a content package to a ftp service_
    * [starskycore](../starsky/starskycore/readme.md) _business logic (netstandard 2.0)_
    * [starskyTest](../starsky/starskytest/readme.md)  _mstest unit tests_
 * [starsky.netframework](../starsky.netframework/readme.md) _Client for older machines_
 * [starsky-node-client](../starsky-node-client/readme.md) _nodejs tools to add-on tasks_
 * [starskyapp](../starskyapp/readme.md) _React-Native app (Pre-alpha code)_


## inotifywait

For linux systems there is `inotifywait`. To auto update the database in real time, you could use a helper to do this:

This example configuration handles currently only `.jpg` and `.JPG` files

### First install tools
Install the tools that are used by this script
```sh
sudo apt-get install inotify-tools
```

### inotify bash script
Make a bash script with content that is in the `inotify.sh` script that is placed in this folder
```sh
sudo nano /usr/local/sbin/inotify.sh
```
>  copy content from `inotify.sh` to the `/usr/local/sbin` folder

Make the `inotify.sh` script executable
```sh
sudo chmod ug+x /usr/local/sbin/inotify.sh
```

### Starsky conf
Make a config file with content that is in the `starsky.conf` script that is placed in this folder
```sh
sudo nano /usr/local/etc/inotify.d/starsky.conf
```
>  copy content from `inotify.conf` to the `/usr/local/etc/inotify.d` folder

### inotifyd bash script
Make a bash script with content that is in the `inotifyd.sh` script that is placed in this folder
```sh
sudo nano /etc/init.d/inotifyd
```
>  copy content from `inotifyd.sh` to the `/etc/init.d/` folder

Make the `inotifyd.sh` script executable
```sh
sudo chmod +x /etc/init.d/inotifyd
```

### update-rc
To init the service run the following command
```sh
sudo update-rc.d -f inotifyd defaults
```
To start and restart the service run the next commands
```sh
sudo service inotifyd start

sudo service inotifyd restart
```

> Source: http://wiki.lenux.org/using-csync2-with-inotifywait/



## Cron Tab's
To update the database using a interval you could use a crontab. In this folder there are example configs added.

### starskyhourly
Copy the `starskyhourly` script to the `/usr/local/sbin/` folder

```sh
sudo nano /usr/local/sbin/starskyhourly.sh
```
> Add the data from `starskyhourly.sh`

### crontab -e

To run the script hourly you need to add this to the crontab helper:
```sh
crontab -e
```
And add the following content
```sh
0       *            *       *       *       /usr/local/sbin/starskyhourly.sh > /home/pi/z-starskycli.log 2>&1
```
