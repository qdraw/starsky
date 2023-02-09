---
sidebar_position: 2
---

# IIS

This guide is work in progress

```powershell
Enable-WindowsOptionalFeature -Online -FeatureName IIS-WebServerRole, IIS-WebServer, IIS-CommonHttpFeatures, IIS-ManagementConsole, IIS-HttpErrors, IIS-HttpRedirect, IIS-WindowsAuthentication, IIS-StaticContent, IIS-DefaultDocument, IIS-HttpCompressionStatic, IIS-DirectoryBrowsing
```

```
cd  C:\Windows\System32\inetsrv
.\appcmd.exe  install module /name:AspNetCoreModule /image:%windir%\system32\inetsrv\aspnetcore.dll
```

@see: https://stackoverflow.com/questions/57878610/aspnetcoremodulev2-missing-from-iis-modules-after-running-the-runtime-bundle

```
mkdir -p C:\inetpub\starsky\thumbnailTempFolder
mkdir -p C:\inetpub\starsky\storageFolder
```

on my machine I currently have 503 gateway timeout