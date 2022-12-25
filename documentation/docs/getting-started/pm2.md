---
sidebar_position: 6
---

# PM2 wrapper

PM2 is a process manager for Node.js applications. 
It allows you to keep your applications running continuously, 
restarting them automatically if they crash or if the server is restarted. 
PM2 also provides many features for monitoring and managing your applications, 
such as automatic load balancing, log management, and integration with popular monitoring tools
like Prometheus and Grafana. PM2 is simple to use and can greatly improve the reliability
and uptime of your Node.js applications. However it is not needed to be a nodejs application, 
it can be any type of application.

# Installation

Download this bash script and run it with bash

## 1. Download the script

Its recommend to download the script in empty folder.
The script unzips lots of dll's and files in the folder the script is located.

```bash
mkdir -p /opt/starsky/starsky
cd /opt/starsky/starsky
wget https://raw.githubusercontent.com/qdraw/starsky/master/starsky/starsky/pm2-install-latest-release.sh
```

Will be saved as: [pm2-install-latest-release.sh](https://raw.githubusercontent.com/qdraw/starsky/master/starsky/starsky/pm2-install-latest-release.sh)

> If you have downloaded the binaries from the release page, you can use the [pm2-new-instance.sh](https://raw.githubusercontent.com/qdraw/starsky/master/starsky/starsky/pm2-new-instance.sh) script

## 2. Run the download and unzip script

**Make sure you run it on a empty folder!**
It will create lots of files inside that folder

If pm2 is already installed it will auto bind it and saves it, but that's optional.
Without pm2 it still extracts the application and makes it ready to run.

> Mac OS: On a M1/M2 Gatekeeper will block the starsky-executables from running.

```bash
cd /opt/starsky/starsky
bash pm2-install-latest-release.sh
```

This script will download the latest release from github and unzip it in the current folder
And run the pm2-new-instance.sh script
This script will create a new instance of pm2 and bind it to the current folder

It is default binded to localhost but the `--anywhere` flag will bind it to all ip's
Run `bash pm2-new-instance.sh --help` for more information
