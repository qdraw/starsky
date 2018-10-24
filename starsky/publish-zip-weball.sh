#!/bin/bash
cd "$(dirname "$0")"

./publish-linux-arm.sh
./publish-mac.sh
./publish-windows.sh
zip -r --filesync starsky-linux-arm.zip linux-arm
zip -r --filesync starsky-osx.10.12-x64.zip osx.10.12-x64
zip -r --filesync starsky-win7-x86.zip win7-x86

rsync -v -e ssh starsky-linux-arm.zip x:/mnt/juno/storage/www/public_apollo/
rsync -v -e ssh starsky-osx.10.12-x64.zip x:/mnt/juno/storage/www/public_apollo/
rsync -v -e ssh starsky-win7-x86.zip x:/mnt/juno/storage/www/public_apollo/

