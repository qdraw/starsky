---
sidebar_position: 1
---

# Windows Service

You optional use Windows Services. This is **not** required. Windows Services (also known as Services, services.msc, service control manager, part of Microsoft Management Console, and services snap-in) is an app in Windows that allows you to manage the settings of other apps and processes that run in the background. 

You can find existing services by entering in the searchbar or powershell window

```powershell
services.msc
```

## 1. Download the binaries

The guide will assume you run it on a Windows machine.
Its recommend to download the script in empty folder.
The script unzips lots of dll's and files in the folder the script is located.


```powershell
mkdir -p C:\inetpub\starsky
cd C:\inetpub\starsky
```

[Download here the latest release for win-x64](https://github.com/qdraw/starsky/releases)

- Get the **server** version

- place it inside the folder you mentioned above

Next extract folder

```powershell
cd C:\inetpub\starsky
Expand-Archive -Path .\starsky-win-x64.zip -DestinationPath $pwd
```

Run the following script to see the configuration options

```powershell
.\service-deploy-windows.ps1 -help
```

and append any params when needed to actualy excute the script

```powershell
.\service-deploy-windows.ps1
```

By default its creating a Windows Service and runs on
http://localhost:4000

