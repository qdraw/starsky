#!/bin/bash
cd "$(dirname "$0")"

pm2 stop starsky

if [ -f starsky-linux-arm.zip ]; then
   unzip -o starsky-linux-arm.zip
else
   echo "File not found"
   exit
fi

if [ -f starsky ]; then
    chmod +x ./starsky
fi

if [ -f starskygeocli ]; then
    chmod +x ./starskygeocli
fi

if [ -f starskyimportercli ]; then
    chmod +x ./starskyimportercli
fi

if [ -f starskysynccli ]; then
    chmod +x ./starskysynccli
fi

if [ -f starskywebftpcli ]; then
    chmod +x ./starskywebftpcli
fi

if [ -f starskywebhtmlcli ]; then
    chmod +x ./starskywebhtmlcli
fi


pm2 start starsky