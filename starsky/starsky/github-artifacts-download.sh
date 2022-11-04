#!/bin/bash

# For insiders only - requires token
# Please use: 
# ./pm2-install-latest-release.sh 
# for public builds

# Script goal:
# Download binaries with zip folder from Github Actions
# Get pm2-new-instance.sh ready to run (but not run)

# source: /opt/starsky/starsky/github-artifacts-download.sh

WORKFLOW_ID="release-on-tag-netcore-desktop-electron.yml"

# default will be overwritten
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
        RUNTIME="osx-arm64" # got some gatekeeper issues
    fi
    ;;

  "x86_64")
    if [ $(uname) = "Darwin" ]; then
        # desktop starsky-mac-x64-desktop
        RUNTIME="osx-x64"
    fi
    # there is no linux desktop
    if [ $(uname) = "Linux" ]; then
        RUNTIME="linux-x64"
    fi
    ;;
esac

CURRENT_DIR=$(dirname "$0")
OUTPUT_DIR=$CURRENT_DIR
FORCE=false

# get arguments
ARGUMENTS=("$@")

echo ${ARGUMENTS}

for ((i = 1; i <= $#; i++ )); do
  CURRENT=$(($i-1))
  if [[ ${ARGUMENTS[CURRENT]} == "--help" ]];
  then
    echo "--runtime linux-arm OR --runtime osx-x64 OR --runtime win-x64"
    echo "     (or as fallback:) --runtime "$RUNTIME
    echo "--branch master"
    echo "--token anything"
    echo "--output output_dir default folder_of_this_file"
    exit 0
  fi
  
  if [[ ${ARGUMENTS[CURRENT]} == "--force" ]];
  then
      FORCE=true
  fi
  
  if [ $i -gt 1 ]; then
    PREV=$(($i-2))
    
    if [[ ${ARGUMENTS[PREV]} == "--runtime" ]];
    then
        RUNTIME="${ARGUMENTS[CURRENT]}"
    fi

    if [[ ${ARGUMENTS[PREV]} == "--token" ]];
    then
        STARSKY_GITHUB_PAT="${ARGUMENTS[CURRENT]}"
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

# rename
VERSION=$RUNTIME

if [[ $VERSION != *desktop ]]
then
    VERSION_ZIP="starsky-"$RUNTIME".zip"
else
   VERSION_ZIP=$RUNTIME".zip"
fi 

if [ ! -d $OUTPUT_DIR ]; then
    echo "FAIL "$OUTPUT_DIR" does not exist "
    exit 1
fi

# output dir should have slash at end
if [ -f $OUTPUT_DIR"Startup.cs" ]; then
    echo "FAIL: You should not run this folder from the source folder"
    echo "copy this file to the location to run it from"
    echo "end script due failure"
    exit 1
fi

if [[ -z $STARSKY_GITHUB_PAT ]]; then
  echo "enter your PAT: and press enter"
  read STARSKY_GITHUB_PAT
fi

echo ""

ACTIONS_WORKFLOW_URL="https://api.github.com/repos/qdraw/starsky/actions/workflows/"$WORKFLOW_ID"/runs?status=completed&per_page=1&exclude_pull_requests=true"

# First check if output is not an 401 or 404
API_GATEWAY_STATUS_CODE=$(curl --write-out %{http_code} \
    -H "Accept: application/vnd.github+json" \
    -H "Authorization: Bearer "$STARSKY_GITHUB_PAT --silent --output /dev/null $ACTIONS_WORKFLOW_URL)
if [[ "$API_GATEWAY_STATUS_CODE" -ne 200 ]] && [[ "$API_GATEWAY_STATUS_CODE" -ne 404 ]] ; then
  echo "FAIL: Github token is invalid \$STARSKY_GITHUB_PAT"
  exit 1
elif [[ "$API_GATEWAY_STATUS_CODE" -eq 404 ]] ; then
  echo "FAIL: WORKFLOW_ID='"$WORKFLOW_ID"' is not found"
  exit 1
fi

echo "V: "$VERSION " zip: " $VERSION_ZIP
echo "OUT" $OUTPUT_DIR
echo ">: "$ACTIONS_WORKFLOW_URL
RESULT_ACTIONS_WORKFLOW=$(curl --user :$STARSKY_GITHUB_PAT -sS $ACTIONS_WORKFLOW_URL)

ARTIFACTS_URL=$(grep -E -o "\"artifacts_url\":.+\"" <<< $RESULT_ACTIONS_WORKFLOW)
ARTIFACTS_URL=$(grep -E -o "https:\/\/(\w|\.|\/)+" <<< $ARTIFACTS_URL)
ARTIFACTS_URL=($ARTIFACTS_URL) # make array
ARTIFACTS_URL="${ARTIFACTS_URL[0]}" # first of array

if [[ $ARTIFACTS_URL != *artifacts ]]
then
  echo "url "$ARTIFACTS_URL" should end with zip";
  exit 1
fi

echo ">: "$ARTIFACTS_URL

CREATED_AT=$(grep -E -o "\"created_at\": \"(\d|-|T|:)+" <<< $RESULT_ACTIONS_WORKFLOW)
echo ">: "$CREATED_AT "UTC"

RESULT_ARTIFACTS=$(curl --user :$STARSKY_GITHUB_PAT -sS $ARTIFACTS_URL)

# ([0-9a-zA-Z]|\/|:| |,|\"|_|\.|\t|\n|\r|-)

DOWNLOAD_URL=$(echo $RESULT_ARTIFACTS|tr -d '\n')
INDEX_DOWNLOAD_URL=$(echo $DOWNLOAD_URL | grep -aob $VERSION"\"" --color=never | \grep -oE '^[0-9]+')
DOWNLOAD_URL="${DOWNLOAD_URL:INDEX_DOWNLOAD_URL}"

DOWNLOAD_URL=$(grep -E -o "\"archive_download_url\": \"(\d|\.|\w|\:|\/)+" <<< $DOWNLOAD_URL)

DOWNLOAD_URL=$(echo "$DOWNLOAD_URL" | sed "s/\"archive_download_url\": \"//")
DOWNLOAD_URL=($DOWNLOAD_URL) # make array
DOWNLOAD_URL="${DOWNLOAD_URL[0]}" # first of array

if [[ $DOWNLOAD_URL != *zip ]]
then
  echo "url "$DOWNLOAD_URL" should end with zip";
  exit 1
fi
echo ">: $DOWNLOAD_URL"

# check if hash is already downloaded
GITHUB_HEAD_SHA=$(echo $RESULT_ARTIFACTS|tr -d '\n')
GITHUB_HEAD_SHA=$(grep -E -o "\"head_sha\": \"(\d|\.|\w|\:|\/)+" <<< $GITHUB_HEAD_SHA)
GITHUB_HEAD_SHA=$(echo "$GITHUB_HEAD_SHA" | sed "s/\"head_sha\": \"//")
GITHUB_HEAD_SHA=($GITHUB_HEAD_SHA) # make array
GITHUB_HEAD_SHA="${GITHUB_HEAD_SHA[0]}" # first of array

GITHUB_HEAD_SHA_CACHE_FILE="${OUTPUT_DIR}${VERSION_ZIP}.sha-cache.txt"

if [ "$FORCE" = false ] ; then

    LAST_GITHUB_HEAD_SHA=0
    if [[ -f "$GITHUB_HEAD_SHA_CACHE_FILE" ]]; then
        LAST_GITHUB_HEAD_SHA="$(cat $GITHUB_HEAD_SHA_CACHE_FILE)"
        LAST_GITHUB_HEAD_SHA=`echo $LAST_GITHUB_HEAD_SHA | sed -e 's/^[[:space:]]*//'`
    fi 

    if [[ $LAST_GITHUB_HEAD_SHA == $GITHUB_HEAD_SHA ]]; then
        echo "SKIP: for github file check '"$GITHUB_HEAD_SHA"' exists in "$GITHUB_HEAD_SHA_CACHE_FILE
        echo "SKIP: not downloaded again"
        exit 0;
    else 
        echo "CONTINUE: '" $GITHUB_HEAD_SHA"' does not exists in "$GITHUB_HEAD_SHA_CACHE_FILE
    fi
      
    # set the new hash
    echo $GITHUB_HEAD_SHA > $GITHUB_HEAD_SHA_CACHE_FILE
    # END check if hash is already downloaded
fi

mkdir -p $OUTPUT_DIR

OUTPUT_ZIP_PATH="${OUTPUT_DIR}${VERSION_ZIP}"
echo "Next: download output file: "$OUTPUT_ZIP_PATH
 
curl -sS -L --user :$STARSKY_GITHUB_PAT $DOWNLOAD_URL -o "${OUTPUT_ZIP_PATH}_tmp.zip"
if [ ! -f "${OUTPUT_ZIP_PATH}_tmp.zip" ]; then
    echo "FAIL: ${OUTPUT_ZIP_PATH}_tmp.zip" " is NOT downloaded"
    rm $GITHUB_HEAD_SHA_CACHE_FILE || true
    exit 1
fi

if [ -f "$OUTPUT_ZIP_PATH" ]; then
    rm ${OUTPUT_ZIP_PATH}
fi

# contains an zip in a zip
unzip -q -o -j "${OUTPUT_ZIP_PATH}_tmp.zip" -d "${OUTPUT_DIR}temp"

if [ ! -f "${OUTPUT_DIR}temp/${VERSION_ZIP}" ]; then
    echo "FAIL: ${OUTPUT_DIR}temp/${VERSION_ZIP}" " is NOT unpacked"

    rm -rf "${OUTPUT_DIR}temp"
    rm $GITHUB_HEAD_SHA_CACHE_FILE || true

    exit 1
fi

# move file 
mv "${OUTPUT_DIR}temp/${VERSION_ZIP}" $OUTPUT_ZIP_PATH
rm -rf "${OUTPUT_DIR}temp"
rm "${OUTPUT_ZIP_PATH}_tmp.zip"

echo "zip is downloaded "$OUTPUT_ZIP_PATH

if [[ $VERSION != *desktop ]]
then
    echo "YEAH > download for "$RUNTIME" looks ok"
    echo "Next: get pm2-new-instance.sh installer file from zip"
    if [ -f "${OUTPUT_DIR}__pm2-new-instance.sh" ]; then
        rm "${OUTPUT_DIR}__pm2-new-instance.sh"
    fi
    
    NEW_INSTANCE_TEMP_FILE="${OUTPUT_DIR}__pm2-new-instance.sh"
    NEW_INSTANCE_OUTPUT_FILE="${OUTPUT_DIR}pm2-new-instance.sh"

    unzip -p $OUTPUT_ZIP_PATH "pm2-new-instance.sh" > $NEW_INSTANCE_TEMP_FILE
    
    if [ -f $NEW_INSTANCE_TEMP_FILE ]; then
        # check if file contains something
        if [ -s $NEW_INSTANCE_TEMP_FILE ]; then
           mv $NEW_INSTANCE_TEMP_FILE $NEW_INSTANCE_OUTPUT_FILE
        else 
            rm $NEW_INSTANCE_TEMP_FILE
        fi
        
        if [ -s $NEW_INSTANCE_OUTPUT_FILE ]; then
            chmod +rwx $NEW_INSTANCE_OUTPUT_FILE
            echo "run for the setup:"
            # output dir should have slash at end
            echo $OUTPUT_DIR"pm2-new-instance.sh"
        else 
            ls $OUTPUT_DIR
        fi

    else 
        echo " pm2-new-instance.sh is missing, please download it yourself and run it"
        exit 1
    fi
fi 

exit 0