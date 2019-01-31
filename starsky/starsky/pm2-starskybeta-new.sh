#!/bin/bash

cd "$(dirname "$0")"

export ASPNETCORE_URLS="http://localhost:4843/"
export ASPNETCORE_ENVIRONMENT="Production"
export app__Name="starskybeta"
export app__addMemoryCache="true"
export app__storageFolder="/home/pi/starsky-demo-files"
export app__thumbnailTempFolder="/home/pi/starsky-temp/"
export app__DatabaseType="sqlite"
export app__DatabaseConnection="Data Source=data.db"
export app__ReadOnlyFolders__0="/demo-readonly"
export app__ReadOnlyFolders__1="/readonly"
export app__CameraTimeZone="Europe/Amsterdam"

chmod +x starsky

pm2 start --name starskybeta ./starsky
echo "starsky started"

pm2 status