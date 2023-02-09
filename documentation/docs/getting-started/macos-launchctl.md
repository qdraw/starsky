---
sidebar_position: 21
---

# macOS Launchctl
Mac OS has a process manager called launchctl. 
This guide will show you how to install and run starsky with launchctl.
launchctl is a command line tool for managing and interacting with launchd,
a daemon that is responsible for launching processes, including system-level processes
and user-level processes. launchctl is used to start, stop, and manage daemons and agents, 
as well as to manage the launchd process itself.
It can also be used to view and modify the configuration of launchd and the processes it manages.
With launchctl, you can easily manage the processes that run on your Mac, 
including system-level processes and user-level processes.
It is a powerful tool for managing and maintaining the stability and performance of your Mac.

## Note on Gatekeeper
Gatekeeper is a security feature in macOS that helps protect your Mac
from malware and other potentially harmful software. 
It works by checking for a valid code signature when you open an application or 
installer package. If an application or installer does not have a valid signature, 
or if it has been modified since it was signed, Gatekeeper will prevent it from running. 
This helps to prevent malicious software from running on your Mac and helps to ensure the 
integrity of the apps you use. 
Gatekeeper can be configured to allow applications from specific sources, 
such as the App Store and identified developers, or to allow any application to run. 
It is an important part of macOS security and helps to keep your Mac safe from threats.

**We currently don't have any code signing for starsky, so you will need to work around Gatekeeper**

## 1. Download the script that downloads the binaries

The guide will assume you run it on a Mac OS machine.
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

## 3. Run the launchctl script

The following script will create a systemd service file and enable it.
you can change the port. The anywhere flag will bind it to all ip's. The default is localhost.

```bash
cd /opt/starsky/starsky
bash service-deploy-systemd.sh --anywhere --port 4000
```

This script will create an plist file in the following location:
`~/Library/LaunchAgents/starsky.plist`
and startup on login of the current user.

