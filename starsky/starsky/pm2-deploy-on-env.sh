#!/bin/bash
cd "$(dirname "$0")"

## DEPLOY +
## WARMUP WITHOUT LOGIN


PM2NAME="starsky"
RUNTIME="linux-arm"
PORT=5000

ARGUMENTS=("$@")

for ((i = 1; i <= $#; i++ )); do
  if [ $i -gt 1 ]; then 
    PREV=$(($i-2))
    CURRENT=$(($i-1))

    if [[ ${ARGUMENTS[CURRENT]} == "--help" ]];
    then
        echo "--name pm2name"
        echo "--runtime linux-arm"
        echo "--port 4823"
    fi
    
    if [[ ${ARGUMENTS[PREV]} == "--name" ]];
    then
        PM2NAME="${ARGUMENTS[CURRENT]}"
    fi
    
    if [[ ${ARGUMENTS[PREV]} == "--runtime" ]];
    then
        RUNTIME="${ARGUMENTS[CURRENT]}"
    fi
    
    if [[ ${ARGUMENTS[PREV]} == "--port" ]];
    then
        PORT="${ARGUMENTS[CURRENT]}"
    fi
  fi
done

# settings
echo "pm2" $PM2NAME "runtime" $RUNTIME "port" $PORT

if [ ! -f "starsky-$RUNTIME.zip" ]; then
    echo "> starsky-$RUNTIME.zip not found"
    exit
fi

pm2 stop $PM2NAME

if [ -f starsky.dll ]; then
    echo "delete dlls so, and everything except pm2 helpers, and"
    echo "configs, temp, thumbnailTempFolder, deploy zip, sqlite database"

    LSOUTPUT=$(ls)
    for ENTRY in $LSOUTPUT
    do
        if [[ $ENTRY != "appsettings"* && $ENTRY != "pm2-"*
        && $ENTRY != "thumbnailTempFolder"
        && $ENTRY != "temp"
        && $ENTRY != "starsky-$RUNTIME.zip"
        && $ENTRY != *".db" ]];
        then
            rm -rf "$ENTRY"
        else
            echo "$ENTRY"
        fi
    done
fi


if [ -f starsky-$RUNTIME.zip ]; then
   unzip -o starsky-$RUNTIME.zip
else
   echo "> starsky-$RUNTIME.zip File not found"
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

if [ -f pm2-deploy-on-env.sh ]; then
    chmod +x ./pm2-deploy-on-env.sh
fi

if [ -f pm2-warmup.sh ]; then
    chmod +x ./pm2-warmup.sh
fi

pm2 start $PM2NAME

## WARMUP WITHOUT LOGIN
echo "warmup -->"
bash pm2-warmup.sh --port $PORT