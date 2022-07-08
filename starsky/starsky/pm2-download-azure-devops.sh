#!/bin/bash

# For insiders only - requires token
# Please use: 
# ./pm2-install-latest-release.sh 
# for public builds

# Script goal:
# Download binaries with zip folder from Azure Devops
# Get pm2-new-instance.sh ready to run (but not run)

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
    ;;
esac


BRANCH="master"
# azure devops
ORGANIZATION="qdraw"
DEVOPSPROJECT="starsky"
DEVOPSDEFIDS=( 17 20 )
BUILD_ID_DEF=""
# STARSKY_DEVOPS_PAT <= use this one
# export STARSKY_DEVOPS_PAT=""

# get arguments
ARGUMENTS=("$@")

for ((i = 1; i <= $#; i++ )); do
  CURRENT=$(($i-1))
  if [[ ${ARGUMENTS[CURRENT]} == "--help" ]];
  then
      echo "--branch master"
      echo "--token anything"
      echo "--runtime linux-arm"
      echo "     (or:) --runtime linux-arm64"
      echo "     (or:) --runtime osx-x64"
      echo "     (or:) --runtime win7-x64"
      echo "     (or as fallback:) --runtime "$RUNTIME
      echo "(optional) --id BUILD_ID"

      exit 0
  fi
  
  if [ $i -gt 1 ]; then
    PREV=$(($i-2))

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
        STARSKY_DEVOPS_PAT="${ARGUMENTS[CURRENT]}"
    fi
    
    if [[ ${ARGUMENTS[PREV]} == "--id" ]];
    then
        BUILD_ID_DEF="${ARGUMENTS[CURRENT]}"
        DEVOPSDEFIDS=( -1 )
    fi
  fi
done


if [[ -z $STARSKY_DEVOPS_PAT ]]; then
  echo "enter your PAT: and press enter"
  read STARSKY_DEVOPS_PAT
fi

BRANCH="${BRANCH/refs\/heads\//}"
echo $BRANCH

cd "$(dirname "$0")"

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
  curl -sS --user :$STARSKY_DEVOPS_PAT $DOWNLOADJSONURL -o "temp_"$RUNTIME".zip"

  # ignore folders
  echo "unzip double zipped output from azure devops"
  unzip -q -o -j "temp_"$RUNTIME".zip"
  rm "temp_"$RUNTIME".zip"

  if [ -f "starsky-"$RUNTIME".zip" ]; then
    echo "starsky-"$RUNTIME".zip is downloaded"
    return 0
  fi

  echo "FAIL output file: starsky-"$RUNTIME".zip not found"
  exit 1
}

UNIQUE_VALUES() {
  typeset i
  for i do
    [ "$1" = "$i" ] || return 1
  done
  return 0
}

RESULTS_GET_DATA=()
for i in "${DEVOPSDEFIDS[@]}"
do
     echo "_______________________ "
     GET_DATA $i
     RESULTS_GET_DATA+=($?) 
done

echo "RESULTS_GET_DATA"
echo  "${RESULTS_GET_DATA[*]}"


if UNIQUE_VALUES "${RESULTS_GET_DATA[@]}"; then
  echo "OK"
else
  echo "not OK"
fi

if [[ "${RESULTS_GET_DATA[*]}" =~ "1" ]]; then
    # whatever you want to do when array doesn't contain value
    echo "> Download failed, there is no artifact for any definitionId"
    exit 1
fi

if [ -f "starsky-"$RUNTIME".zip" ]; then
    echo "YEAH > download for "$RUNTIME" looks ok"
    echo "get pm2-new-instance.sh installer file"
    unzip -p "starsky-"$RUNTIME".zip" "pm2-new-instance.sh" > ./__pm2-new-instance.sh
    
    if [ -s ./__pm2-new-instance.sh ]; then
       mv __pm2-new-instance.sh pm2-new-instance.sh
    else 
        rm ./__pm2-new-instance.sh
    fi
    
fi

if [ -f pm2-new-instance.sh ]; then
    chmod +rwx ./pm2-new-instance.sh
    echo "run for the setup:"
    echo "./pm2-new-instance.sh"
else 
    echo " pm2-new-instance.sh is missing, please download it yourself and run it"
    exit 1
fi
