#!/bin/bash
cd "$(dirname "$0")"

## DEPLOY ONLY
# use ./pm2-new-instance.sh for upgrading and installation
# this script is only for getting a local file and deploy it over, does not install anything

# for warnup check: ./pm2-warmup.sh --port 4823

PM2NAME="starsky"
RUNTIME="linux-arm"

ARGUMENTS=("$@")

for ((i = 1; i <= $#; i++ )); do
  CURRENT=$(($i-1))
  if [[ ${ARGUMENTS[CURRENT]} == "--help" ]];
  then
      echo "--name pm2name"
      echo "--runtime linux-arm"
      echo "     (or:) --runtime linux-arm64"
      echo "     (or:) --runtime osx-x64"
      echo "     (or:) --runtime osx-arm64"
      echo "     (or:) --runtime win-x64"
      exit 0
  fi
  
  if [ $i -gt 1 ]; then
    PREV=$(($i-2))

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
echo "pm2" $PM2NAME "runtime" $RUNTIME

if [ ! -f "starsky-$RUNTIME.zip" ]; then
    echo "> starsky-$RUNTIME.zip not found"
    echo "./pm2-deploy-on-env.sh --runtime linux-arm64"
    exit 1
fi

pm2 stop $PM2NAME

# Keep UserViews over a release
if [ -d "WebHtmlPublish/UserViews" ]; then
  cp -r "WebHtmlPublish/UserViews" "UserViews/"
fi

# delete files in www-root
if [ -f starsky.dll ]; then
    echo "delete dlls so, and everything except pm2 helpers, and"
    echo "configs, temp, thumbnailTempFolder, deploy zip, sqlite database"

    LSOUTPUT=$(ls)
    for ENTRY in $LSOUTPUT
    do
        if [[ $ENTRY != "appsettings"* 
        && $ENTRY != "pm2-"*
        && $ENTRY != "service-"*
        && $ENTRY != "thumbnailTempFolder"
        && $ENTRY != "temp"
        && $ENTRY != "UserViews"* # Keep UserViews
        && $ENTRY != "starsky-"*
        && $ENTRY != *".db" ]];
        then
            rm -rf "$ENTRY"
        else
            echo "$ENTRY"
        fi
    done
else
   echo "> starsky.dll File not found"
fi

echo "Now Unzipping the archive"

# UnZIP archive
if [ -f starsky-$RUNTIME.zip ]; then
   unzip -o starsky-$RUNTIME.zip
else
   echo "> starsky-$RUNTIME.zip File not found"
   exit 1
fi

# reset rights if those are wrong
/usr/bin/find . -type d -exec chmod 755 {} \;
/usr/bin/find . -type f -exec chmod 644 {} \;

# to restore the content UserViews
if [ -d "UserViews" ]; then
  cp -fr "UserViews" "WebHtmlPublish"
  rm -rf "UserViews"
fi

# excute right for specific files
if [ -f starsky ]; then
    chmod +rwx ./starsky
fi

if [ -f starskygeocli ]; then
    chmod +rwx ./starskygeocli
fi

if [ -f starskyimportercli ]; then
    chmod +rwx ./starskyimportercli
fi

if [ -f starskysynchronizecli ]; then
    chmod +rwx ./starskysynchronizecli
fi

if [ -f starskythumbnailcli ]; then
    chmod +rwx ./starskythumbnailcli
fi

if [ -f starskythumbnailmetacli ]; then
    chmod +rwx ./starskythumbnailmetacli
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

if [ -f pm2-new-instance.sh ]; then
    chmod +rwx ./pm2-new-instance.sh
fi

if [ -f pm2-download-azure-devops.sh ]; then
    chmod +rwx ./pm2-download-azure-devops.sh
fi

if [ -f pm2-warmup.sh ]; then
    chmod +rwx ./pm2-warmup.sh
fi

if [ -f dependencies/exiftool-unix/exiftool ]; then
    chmod +rwx dependencies/exiftool-unix/exiftool
fi

pm2 start $PM2NAME

echo "!> done with deploying"
echo "!> to warmup, you need to run:"
echo "./pm2-warmup.sh --port 4823"
