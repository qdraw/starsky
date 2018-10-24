#!/bin/bash
cd "$(dirname "$0")"

# build for all types
./publish-linux-arm.sh
./publish-mac.sh
./publish-windows.sh

# zip
zip -r --filesync starsky-linux-arm.zip linux-arm
zip -r --filesync starsky-osx.10.12-x64.zip osx.10.12-x64
zip -r --filesync starsky-win7-x86.zip win7-x86

# Remove appsettings.*
zip -d starsky-linux-arm.zip "linux-arm/appsettings.*.json"
zip -d starsky-osx.10.12-x64.zip "starsky-osx.10.12-x64/appsettings.*.json"
zip -d starsky-win7-x86.zip "win7-x86/appsettings.*.json"

# remove temp folder
zip -d starsky-linux-arm.zip "temp/*"
zip -d starsky-osx.10.12-x64.zip "temp/*"
zip -d starsky-win7-x86.zip "temp/*"

# remove thumbnailTempFolder
zip -d starsky-linux-arm.zip "thumbnailTempFolder/*"
zip -d starsky-osx.10.12-x64.zip "thumbnailTempFolder/*"
zip -d starsky-win7-x86.zip "thumbnailTempFolder/*"

# remove storageFolder
zip -d starsky-linux-arm.zip "storageFolder/*"
zip -d starsky-osx.10.12-x64.zip "storageFolder/*"
zip -d starsky-win7-x86.zip "storageFolder/*"

rsync -v -e ssh starsky-linux-arm.zip x:/mnt/juno/storage/www/public_apollo/
rsync -v -e ssh starsky-osx.10.12-x64.zip x:/mnt/juno/storage/www/public_apollo/
rsync -v -e ssh starsky-win7-x86.zip x:/mnt/juno/storage/www/public_apollo/
