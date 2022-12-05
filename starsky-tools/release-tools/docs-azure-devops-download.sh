#!/bin/bash

# For insiders only - requires token

# Script goal:
# Download docs from Azure Devops

# Filename: docs-azure-devops-download.sh

BRANCH="master"
# azure devops
ORGANIZATION="qdraw"
DEVOPSPROJECT="starsky"
DEVOPSDEFIDS=( 24 )
BUILD_ID_DEF=""
# STARSKY_DEVOPS_PAT <= use this one
# export STARSKY_DEVOPS_PAT=""

CURRENT_DIR=$(dirname "$0")
OUTPUT_DIR=$CURRENT_DIR

# get arguments
ARGUMENTS=("$@")

for ((i = 1; i <= $#; i++ )); do
  CURRENT=$(($i-1))
  if [[ ${ARGUMENTS[CURRENT]} == "--help" ]];
  then
      echo "--branch master"
      echo "--token anything"
      echo "--clean (remove content before download)"
      echo "(optional) --id BUILD_ID"

      exit 0
  fi
  
  if [ $i -gt 1 ]; then
    PREV=$(($i-2))

    if [[ ${ARGUMENTS[PREV]} == "--branch" ]];
    then
        BRANCH="${ARGUMENTS[CURRENT]}"
    fi

    if [[ ${ARGUMENTS[PREV]} == "--token" ]];
    then
        STARSKY_DEVOPS_PAT="${ARGUMENTS[CURRENT]}"
    fi
    
    if [[ ${ARGUMENTS[PREV]} == "--id" ]];
    then
        BUILD_ID_DEF="${ARGUMENTS[CURRENT]}"
        DEVOPSDEFIDS=( -1 )
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

if [ -f $OUTPUT_DIR"readme.md" ]; then # output dir should have slash at end
    echo "FAIL: You should not run this folder from the source folder"
    echo "copy this file to the location to run it from"
    echo "end script due failure"
    exit 1
fi

if [[ -z $STARSKY_DEVOPS_PAT ]]; then
  echo "enter your PAT: and press enter"
  read STARSKY_DEVOPS_PAT
fi

BRANCH="${BRANCH/refs\/heads\//}"
echo $BRANCH



GET_DATA () {
  LOCALDEVOPSDEFID=$1

  if [[ "$LOCALDEVOPSDEFID" != -1 ]]; then
    echo "try: get artifact for Id: "$LOCALDEVOPSDEFID
  else  
    echo "try: get artifact"
  fi
  
  URLBUILDS="https://dev.azure.com/"$ORGANIZATION"/"$DEVOPSPROJECT"/_apis/build/builds?api-version=5.1&\$top=1&statusFilter=completed&definitions="$LOCALDEVOPSDEFID"&branchName=refs%2Fheads%2F"$BRANCH
  RESULTBUILDS=$(curl -sS --user :$STARSKY_DEVOPS_PAT $URLBUILDS)
  
  if [[ "$RESULTBUILDS" == *"Object moved to"* || "$RESULTBUILDS" == *"Access Denied"* ]]; then
    echo "FAIL: You don't have access!"
    exit 1
  fi

   # echo '-28T16:20:31.273Z"},"uri":"vstfs:///Build/Build/3216","sou' | grep -Eo 'uri.{3}?vstfs.{4}Build.Build.[0-9]+'

  if [[ -z $BUILD_ID_DEF ]]; then
      VSTFSURL=$(echo $RESULTBUILDS | grep -Eo 'uri.{3}?vstfs.{4}Build.Build.[0-9]+') 
    
      BUILDNUMBER=$(echo $RESULTBUILDS | grep -Eo '(buildNumber.{3})([0-9]{8}.[0-9]{1,5})') 
      if [[ ! -z $BUILDNUMBER ]]; then
         echo $BUILDNUMBER
      fi
        
      BUILDID=$(grep -E -o '[0-9]+' <<< $VSTFSURL)
      if [[ -z $BUILDID ]]; then
        echo "Continue > No build id found for: "$LOCALDEVOPSDEFID
        return 1
      fi   
  else 
     BUILDID=$BUILD_ID_DEF
  fi
  echo "build id: "$BUILDID

  URLGETARTIFACT="https://dev.azure.com/"$ORGANIZATION"/"$DEVOPSPROJECT"/_apis/build/builds/"$BUILDID"/artifacts?api-version=5.1&artifactName="$RUNTIME
  RESULTARTIFACT=$(curl -sS --user :$STARSKY_DEVOPS_PAT $URLGETARTIFACT)

  # find url to download from
  DOWNLOADJSONURL=$(grep -E -o "\"downloadUrl\":.+\"" <<< $RESULTARTIFACT)
  DOWNLOADJSONURL=$(grep -E -o "https:\/\/.+" <<< $DOWNLOADJSONURL)
  # replace quote at end
  DOWNLOADJSONURL="${DOWNLOADJSONURL%\"}"

  if [[ -z $DOWNLOADJSONURL ]]; then
    echo "> for buildId: "$BUILDID" there is no artifact"
    return 1
  fi

  echo "Download > "$DOWNLOADJSONURL
  curl -sS --user :$STARSKY_DEVOPS_PAT $DOWNLOADJSONURL -o "temp_docs.zip"

  # ignore folders
  echo "unzip double zipped output from azure devops"
  unzip -q -o -j "temp_docs.zip"
  rm "temp_docs.zip"

  if [ -f "index.html" ]; then
    echo "index.html and docs is downloaded"
    return 0
  fi

  echo "FAIL output file: index.html and docs not found"
  exit 1
}

UNIQUE_VALUES() {
  typeset i
  for i do
    [ "$1" = "$i" ] || return 1
  done
  return 0
}

if [ ! -d $OUTPUT_DIR ]; then
    echo "FAIL "$OUTPUT_DIR" does not exist "
    exit 1
fi
if [ -f $OUTPUT_DIR"/readme.md" ]; then
    echo "FAIL: You should not run this folder from the source folder"
    echo "copy this file to the location to run it from"
    echo "end script due failure"
    exit 1
fi

cd $OUTPUT_DIR

RESULTS_GET_DATA=()
for i in "${DEVOPSDEFIDS[@]}"
do
     echo "_______________________ "
     GET_DATA $i
     RESULTS_GET_DATA+=($?) 
done

