#!/bin/bash

# Script goals:
# Removes starsky-$RUNTIME script 
# Assumes that pm2-new-instance.sh download and install a new pm2 instance
# download pm2-new-instance.sh installer and run afterwards

CURRENT_DIR=$(dirname "$0")
OUTPUT_DIR=$CURRENT_DIR

PM2NAME="starsky"
RUNTIME="linux-arm"
case $(uname -m) in
  "aarch64")
    RUNTIME="linux-arm64"
    ;;

  "armv7l")
    RUNTIME="linux-arm"
    ;;

  "arm64")
    if [ $(uname) = "Darwin" ]; then
        RUNTIME="osx-arm64"
    fi
    ;;

  "x86_64")
    if [ $(uname) = "Darwin" ]; then
        RUNTIME="osx-x64"
    fi
    if [ $(uname) = "Linux" ]; then
        RUNTIME="linux-x64"
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
      echo "     (or as fallback:) --runtime "$RUNTIME
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
    
    if [[ ${ARGUMENTS[PREV]} == "--output" ]];
    then
        OUTPUT_DIR="${ARGUMENTS[CURRENT]}"
    fi
  fi
done

# add slash if not exists
LAST_CHAR_OUTPUT_DIR=${OUTPUT_DIR:length-1:1}
[[ $LAST_CHAR_OUTPUT_DIR != "/" ]] && OUTPUT_DIR="$OUTPUT_DIR/"; :

if [ ! -d $OUTPUT_DIR ]; then
    echo "FAIL "$OUTPUT_DIR" does not exist "
    exit 1
fi

if [ -f $OUTPUT_DIR"Startup.cs" ]; then # output dir should have slash at end
    echo "FAIL: You should not run this folder from the source folder"
    echo "copy this file to the location to run it from"
    echo "end script due failure"
    exit 1
fi

cd $OUTPUT_DIR
echo "runtime: "$RUNTIME

# to upgrade delete this file
if [ -f "starsky-$RUNTIME.zip" ]; then
    echo "remove exiting zip file"
    rm "starsky-$RUNTIME.zip"
fi

if [ ! -f pm2-new-instance.sh ]; then
    echo "install script is downloaded from github"
    
    if command -v wget &> /dev/null
    then
        wget -q https://raw.githubusercontent.com/qdraw/starsky/master/starsky/starsky/pm2-new-instance.sh -O pm2-new-instance.sh
    else
        curl https://raw.githubusercontent.com/qdraw/starsky/master/starsky/starsky/pm2-new-instance.sh --output pm2-new-instance.sh
    fi 
    
fi

if [ -f pm2-new-instance.sh ]; then
    chmod +rwx ./pm2-new-instance.sh
    echo "NEXT run ./pm2-new-instance.sh ""${ARGUMENTS[*]}"
    bash pm2-new-instance.sh $ARGUMENTS
else 
    echo " pm2-new-instance.sh is missing, please download it yourself and run it"
    exit 1
fi

