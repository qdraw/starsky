#!/bin/bash
cd "$(dirname "$0")"
pwd

if [ ! -f starsky-linux-arm.zip ]; then
    echo "> starsky-linux-arm.zip not found"
    exit
fi

pm2 stop starsky

if [ -f starsky.dll ]; then
    echo "delete dlls so, and everything except pm2 helpers, and configs, temp, thumbnailTempFolder, deploy zip"
    LSOUTPUT=$(ls)
    for ENTRY in $LSOUTPUT
    do
        if [[ $ENTRY != "appsettings"* && $ENTRY != "pm2-"* && $ENTRY != "thumbnailTempFolder" && $ENTRY != "temp" && $ENTRY != "starsky-linux-arm.zip" ]];
        then
            rm -rf "$ENTRY"
        else
            echo "$ENTRY"
        fi
    done
fi


if [ -f starsky-linux-arm.zip ]; then
   unzip -o starsky-linux-arm.zip
else
   echo "> starsky-linux-arm.zip File not found"
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
