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
        echo "(optional) --appinsights true - to ask for app insights keys"
    fi

    if [[ ${ARGUMENTS[PREV]} == "--name" ]];
    then
        PM2NAME="${ARGUMENTS[CURRENT]}"
    fi

    if [[ ${ARGUMENTS[PREV]} == "--runtime" ]];
    then
        RUNTIME="${ARGUMENTS[CURRENT]}"
    fi
    
    if [[ ${ARGUMENTS[PREV]} == "--appinsights" ]];
    then
        USEAPPINSIGHTS="${ARGUMENTS[CURRENT]}"
    fi
  fi
done

if ! command -v pm2 &> /dev/null
then
    echo "warning: pm2 is missing, the script continues but skips the last step"
fi

# settings
echo "run with the following parameters "
echo "--name " $PM2NAME " --runtime" $RUNTIME " --appinsights" $USEAPPINSIGHTS

cd "$(dirname "$0")"

export ASPNETCORE_URLS="http://localhost:4823/"
export ASPNETCORE_ENVIRONMENT="Production"

# only asked with --appinsights true parameter
if [ ! -z "$USEAPPINSIGHTS" ];
then
    echo "Copy the App Insights key string and press [ENTER]:"
    echo "for example: "
    echo "11111111-2222-3333-4444-555555555555"
    echo ">>>"
    read -p "Enter: " INSTRUMENTATIONKEY
    export APPINSIGHTS_INSTRUMENTATIONKEY=$INSTRUMENTATIONKEY
fi

if [ -f starsky-$RUNTIME.zip ]; then
   echo "upgrade existing zip file" 
   echo "going to unzip starsky-$RUNTIME.zip"
   
    unzip -q -o starsky-$RUNTIME.zip -x "pm2-new-instance.sh"
else
   echo "continue > starsky-$RUNTIME.zip File not found" 
   echo "try to download latest release"
   
   # Get latest stable from Github Releases
   curl -s https://api.github.com/repos/qdraw/starsky/releases/latest \
   | grep "browser_download_url.*starsky-$RUNTIME.zip" \
   | cut -d ":" -f 2,3 \
   | tr -d \" \
   | wget -qi -
   
   if [ -f starsky-$RUNTIME.zip ]; then
      echo "use latest stable,"
      echo "going to unzip starsky-$RUNTIME.zip"
      unzip -q -o starsky-$RUNTIME.zip
   else
      echo "FAILED > starsky-$RUNTIME.zip Download failed; exit now"
      exit 1
   fi
fi

echo "reset rights if those are wrong"
/usr/bin/find . -type d -exec chmod 755 {} \;
/usr/bin/find . -type f -exec chmod 644 {} \;


# execute rights for specific files
if [ -f starskygeocli ]; then
    chmod +rwx ./starskygeocli
fi

if [ -f starskyimportercli ]; then
    chmod +rwx ./starskyimportercli
fi

if [ -f starskysynccli ]; then
    chmod +rwx ./starskysynccli
fi

if [ -f starskysynchronizecli ]; then
    chmod +rwx ./starskysynchronizecli
fi

if [ -f starskythumbnailcli ]; then
    chmod +rwx ./starskythumbnailcli
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
fi

if [ -f starskyimportercli ]; then
    echo "run starskyimportercli to auto download dependencies"
    ./starskyimportercli -h > /dev/null 2>&1
fi

if [ -f starskygeocli ]; then
    echo "run starskygeocli to auto download dependencies"
    ./starskygeocli -h > /dev/null 2>&1
fi

if ! command -v pm2 &> /dev/null
then
    echo "FAIL pm2 is missing run: "
    echo "sudo npm install -g pm2"
    echo "and run it again to add it to pm2"
    exit 1
fi

if [ -f starsky ]; then

    echo "check if service exist >"
    pm2 describe $PM2NAME > /dev/null
    HASDESCRIBE=$?
    
    SHOULDSCRIPTPATH=$(pwd)"/starsky"
    SCRIPTPATH=$(pm2 describe $PM2NAME | grep "script path" | grep -oP '\s│\K[^|]+' | sed 's/..$//' | xargs)
    echo "<"
    echo "$SCRIPTPATH"
    echo "$SHOULDSCRIPTPATH"
    echo "$HASDESCRIBE"
    
    if [ ! -z "$SCRIPTPATH" ] && [ "$SCRIPTPATH" != "$SHOULDSCRIPTPATH" ]; then
        echo "remove pm2 instance due script path on a different location " 
        echo "script path:" $SCRIPTPATH 
        echo "should script path: " $SHOULDSCRIPTPATH 
        pm2 delete $PM2NAME
        pm2 save --force
        # now the service does not exist anymore
        HASDESCRIBE=1
    fi
    
    if [ "${HASDESCRIBE}" -ne 0 ]; then
      echo "start"
      pm2 start --name $PM2NAME ./starsky
    else
      echo "re start"
      pm2 restart $PM2NAME
    fi;
    
    echo "pm2 show status of " $PM2NAME
    pm2 status

    echo "> AUTO SAVE - pm2 save " $PM2NAME
    pm2 save --force
fi

echo "Done and saved :)"
