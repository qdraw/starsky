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
  
  if [[ $i -gt 1 ]]; then
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

if [[ ! -f "starsky-$RUNTIME.zip" ]]; then
    echo "> starsky-$RUNTIME.zip not found"
    echo "./pm2-deploy-on-env.sh --runtime linux-arm64"
    exit 1
fi

pm2 stop $PM2NAME

USER_VIEWS="UserViews"
# Keep UserViews over a release
if [[ -d "WebHtmlPublish/$USER_VIEWS" ]]; then
  cp -r "WebHtmlPublish/$USER_VIEWS" "$USER_VIEWS/"
fi

# delete files in www-root
if [[ -f starsky.dll ]]; then
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
        && $ENTRY != "$USER_VIEWS"* # Keep UserViews
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
if [[ -f starsky-$RUNTIME.zip ]]; then
   unzip -o starsky-$RUNTIME.zip
else
    echo "> starsky-$RUNTIME.zip File not found"
    exit 1
fi

# reset rights if those are wrong
/usr/bin/find . -type d -exec chmod 755 {} \;
/usr/bin/find . -type f -exec chmod 644 {} \;

# to restore the content UserViews
if [[ -d "$USER_VIEWS" ]]; then
  cp -fr "$USER_VIEWS" "WebHtmlPublish"
  rm -rf "$USER_VIEWS"
fi

# excute right for specific files
files=(
  "starskygeocli"
  "starskyimportercli"
  "starskysynchronizecli"
  "starskythumbnailcli"
  "starskythumbnailmetacli"
  "starskywebftpcli"
  "starskywebhtmlcli"
  "starskyadmincli"
  "starskymountwatchercli"
  "starskydependenciesdownloadcli"
  "pm2-deploy-on-env.sh"
  "pm2-install-latest-release.sh"
  "pm2-new-instance.sh"
  "pm2-download-azure-devops.sh"
  "dependencies/exiftool-unix/exiftool"
  "pm2-restore-x-rights.sh"
  "pm2-warmup.sh"
  "service-deploy-systemd.sh",
  "github-artifacts-download.sh",
  "download-mirror.sh",
  "ollama-dependencies-download.sh",
  "starsky"
)

for file in "${files[@]}"; do
  if [[ -f "$file" ]]; then
    chmod +x "$file"
  fi
done

pm2 start $PM2NAME

echo "!> done with deploying"
echo "!> to warmup, you need to run:"
echo "./pm2-warmup.sh --port 4823"
