#!/bin/bash

RUNTIME="linux-arm"
# linux-arm64, linux-arm, osx.10.12-x64 (or windows)

BRANCH="master"
# azure devops
ORGANIZATION="qdraw"
DEVOPSPROJECT="starsky"
DEVOPSDEFIDS=( 17 20 )
# DEVOPSPAT <= use this one

# get arguments
ARGUMENTS=("$@")

for ((i = 1; i <= $#; i++ )); do
  if [ $i -gt 1 ]; then
    PREV=$(($i-2))
    CURRENT=$(($i-1))

    if [[ ${ARGUMENTS[CURRENT]} == "--help" ]];
    then
        echo "--runtime linux-arm"
        echo "--branch master"
        echo "--token anything"

    fi

    if [[ ${ARGUMENTS[PREV]} == "--branch" ]];
    then
        BRANCH="${ARGUMENTS[CURRENT]}"
    fi

    if [[ ${ARGUMENTS[PREV]} == "--runtime" ]];
    then
        RUNTIME="${ARGUMENTS[CURRENT]}"
    fi

    if [[ ${ARGUMENTS[PREV]} == "--token" ]];
    then
        DEVOPSPAT="${ARGUMENTS[CURRENT]}"
    fi
  fi
done

if [[ -z $DEVOPSPAT ]]; then
  echo "enter your PAT: and press enter"
  read DEVOPSPAT
fi

BRANCH="${BRANCH/refs\/heads\//}"
echo $BRANCH

cd "$(dirname "$0")"

GET_DATA () {
  LOCALDEVOPSDEFID=$1
  echo "try: get artifact for Id: "$LOCALDEVOPSDEFID
  URLBUILDS="https://dev.azure.com/"$ORGANIZATION"/"$DEVOPSPROJECT"/_apis/build/builds?api-version=5.1&\$top=1&statusFilter=completed&definitions="$LOCALDEVOPSDEFID"&branchName=refs%2Fheads%2F"$BRANCH
  RESULTBUILDS=$(curl -sS --user :$DEVOPSPAT $URLBUILDS)

  if [[ "$RESULTBUILDS" == *"Object moved to"* ]]; then
    echo "FAIL: You don't have access!"
    exit 1
  fi

  VSTFSURL=$(grep -E -o 'uri\":\"vstfs:\/\/\/Build\/Build\/\d+' <<< $RESULTBUILDS)
  BUILDID=$(grep -E -o '\d+' <<< $VSTFSURL)

  if [[ -z $BUILDID ]]; then
    echo "FAIL no build id found"
    exit 1
  fi


  URLGETARTIFACT="https://dev.azure.com/"$ORGANIZATION"/"$DEVOPSPROJECT"/_apis/build/builds/"$BUILDID"/artifacts?api-version=5.1&artifactName="$RUNTIME
  echo $URLGETARTIFACT
  RESULTARTIFACT=$(curl -sS --user :$DEVOPSPAT $URLGETARTIFACT)

  # find url to download from
  DOWNLOADJSONURL=$(grep -E -o "\"downloadUrl\":.+\"" <<< $RESULTARTIFACT)
  DOWNLOADJSONURL=$(grep -E -o "https:\/\/.+" <<< $DOWNLOADJSONURL)
  # replace quote at end
  DOWNLOADJSONURL="${DOWNLOADJSONURL%\"}"

  if [[ -z $DOWNLOADJSONURL ]]; then
    echo "> for buildId: "$LOCALDEVOPSDEFID" there is no artifact"
    return 1
  fi

  echo "Download > "$DOWNLOADJSONURL
  curl -sS --user :$DEVOPSPAT $DOWNLOADJSONURL -o "temp_"$RUNTIME".zip"

  # ignore folders
  echo "unzip double zipped output from azure devops"
  unzip -q -o -j "temp_"$RUNTIME".zip"
  rm "temp_"$RUNTIME".zip"

  if [ -f "starsky-"$RUNTIME".zip" ]; then
    echo "starsky-"$RUNTIME".zip is downloaded"
    exit
  fi

  echo "FAIL output file: starsky-"$RUNTIME".zip not found"
  exit 1
}

for i in "${DEVOPSDEFIDS[@]}"
do
   GET_DATA $i
done
