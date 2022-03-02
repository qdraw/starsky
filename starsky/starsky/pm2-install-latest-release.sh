#!/bin/bash

# Script goals:
# Removes starsky-$RUNTIME script 
# Assumes that pm2-new-instance.sh download and install a new pm2 instance
# download pm2-new-instance.sh installer and run afterwards

PM2NAME="starsky"
RUNTIME="linux-arm"
case $(uname -m) in
  "aarch64")
    RUNTIME="linux-arm64"
    ;;

  "armv7l")
    RUNTIME="linux-arm"
    ;;

  "x86_64")
    if [ $(uname) = "Darwin" ]; then
        RUNTIME="osx-x64"
    fi
    ;;
esac


ARGUMENTS=("$@")

for ((i = 1; i <= $#; i++ )); do
  CURRENT=$(($i-1))
  if [[ ${ARGUMENTS[CURRENT]} == "--help" ]];
  then
      echo "--name pm2name"
      echo "--runtime linux-arm"
      echo "     (or:) --runtime linux-arm64"
      echo "     (or:) --runtime osx-x64"
      echo "     (or:) --runtime win7-x64"
      exit 0
  fi
      
  if [ $i -gt 1 ]; then
    PREV=$(($i-2))
    CURRENT=$(($i-1))

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

cd "$(dirname "$0")"
echo $RUNTIME

# to upgrade delete this file
if [ -f "starsky-$RUNTIME.zip" ]; then
    echo "remove exiting zip file"
    rm "starsky-$RUNTIME.zip"
fi

if [ ! -f pm2-new-instance.sh ]; then
    echo "install script is downloaded from github"
    wget -q https://raw.githubusercontent.com/qdraw/starsky/master/starsky/starsky/pm2-new-instance.sh -O pm2-new-instance.sh
fi

if [ -f pm2-new-instance.sh ]; then
    chmod +rwx ./pm2-new-instance.sh
    bash pm2-new-instance.sh $ARGUMENTS
else 
    echo " pm2-new-instance.sh is missing, please download it yourself and run it"
    exit 1
fi

