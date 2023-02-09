---
sidebar_position: 20
---

# Linux SystemD

Systemd is an init system and system manager for Linux operating systems. It is a collection of programs and libraries, not a single binary. Systemd is responsible for starting and managing processes on the system, including system services, user programs, and daemons. It provides a standard interface for controlling and interacting with these processes, as well as a number of features to improve the system's reliability, performance, and management.


## 1. Download the script

The guide will assume you run it on a linux machine with systemd installed
Its recommend to download the script in empty folder.
The script unzips lots of dll's and files in the folder the script is located.

```bash
mkdir -p /opt/starsky/starsky
cd /opt/starsky/starsky
wget https://raw.githubusercontent.com/qdraw/starsky/master/starsky/starsky/pm2-install-latest-release.sh
```

If pm2 is not installed it will unzips the application and makes it ready to run.

Will be saved as: [pm2-install-latest-release.sh](https://raw.githubusercontent.com/qdraw/starsky/master/starsky/starsky/pm2-install-latest-release.sh)

> If you have downloaded the binaries from the release page, you can use the [pm2-new-instance.sh](https://raw.githubusercontent.com/qdraw/starsky/master/starsky/starsky/pm2-new-instance.sh) script

## 2. Run the download and unzip script

**Make sure you run it on a empty folder!**
It will create lots of files inside that folder

If pm2 is already installed it will auto bind it and saves it, but that's optional.
Without pm2 it still extracts the application and makes it ready to run.

```bash
cd /opt/starsky/starsky
bash pm2-install-latest-release.sh
```

## 3. Run the systemd script

The following script will create a systemd service file and enable it.
you can change the port. The anywhere flag will bind it to all ip's. The default is localhost.

```bash
cd /opt/starsky/starsky
bash service-deploy-systemd.sh --anywhere --port 4000
```