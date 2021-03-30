#!/bin/bash

# Script goals:
# Removes starsky-$RUNTIME script 
# Assumes that pm2-new-instance.sh download and install a new pm2 instance

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

cd "$(dirname "$0")"
echo $RUNTIME

rm "starsky-$RUNTIME.zip"

bash pm2-new-instance.sh $ARGUMENTS