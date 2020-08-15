#!/bin/bash

PM2NAME="starsky"
RUNTIME="linux-arm"

ARGUMENTS=("$@")

for ((i = 1; i <= $#; i++ )); do
  if [ $i -gt 1 ]; then
    PREV=$(($i-2))
    CURRENT=$(($i-1))

    if [[ ${ARGUMENTS[CURRENT]} == "--help" ]];
    then
        echo "--name pm2name"
        echo "--runtime linux-arm"
        echo "(or:) --runtime linux-arm64"
    fi

    if [[ ${ARGUMENTS[PREV]} == "--name" ]];
    then
        PM2NAME="${ARGUMENTS[CURRENT]}"
    fi

    if [[ ${ARGUMENTS[PREV]} == "--runtime" ]];
    then
        RUNTIME="${ARGUMENTS[CURRENT]}"
    fi
  fi
done

# settings
echo "--name " $PM2NAME " --runtime" $RUNTIME

cd "$(dirname "$0")"

export ASPNETCORE_URLS="http://localhost:4823/"
export ASPNETCORE_ENVIRONMENT="Production"

echo "Copy the App Insights key string and press [ENTER]:"
echo "for example: "
echo "11111111-2222-3333-4444-555555555555"
echo ">>>"
read -p "Enter: " INSTRUMENTATIONKEY

export APPINSIGHTS_INSTRUMENTATIONKEY=$INSTRUMENTATIONKEY

if [ -f starsky-$RUNTIME.zip ]; then
   unzip -o starsky-$RUNTIME.zip
else
   echo "> starsky-$RUNTIME.zip File not found"
fi

# reset rights if those are wrong
/usr/bin/find . -type d -exec chmod 755 {} \;
/usr/bin/find . -type f -exec chmod 644 {} \;


# excute right for specific files
if [ -f starskygeocli ]; then
    chmod +rwx ./starskygeocli
fi

if [ -f starskyimportercli ]; then
    chmod +rwx ./starskyimportercli
fi

if [ -f starskysynccli ]; then
    chmod +rwx ./starskysynccli
fi

if [ -f starskywebftpcli ]; then
    chmod +rwx ./starskywebftpcli
fi

if [ -f starskywebhtmlcli ]; then
    chmod +rwx ./starskywebhtmlcli
fi

if [ -f starskyadmincli ]; then
    chmod +rwx ./starskyadmincli
fi

if [ -f pm2-deploy-on-env.sh ]; then
    chmod +rwx ./pm2-deploy-on-env.sh
fi

if [ -f pm2-warmup.sh ]; then
    chmod +rwx ./pm2-warmup.sh
fi

if [ -f starsky ]; then
    chmod +rwx ./starsky

    pm2 start --name $PM2NAME ./starsky
    echo $PM2NAME " started"

    pm2 status
    echo "Need to run `pm2 save` yourself"
fi
