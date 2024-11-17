#!/bin/bash

EXIFTOOL_DOMAIN="https://exiftool.org"
EXIFTOOL_CHECKSUMS_API=$EXIFTOOL_DOMAIN"/checksums.txt"
BINARY_FOLDERNAME="mirror/exiftool"
INDEX_FILE="checksums.txt"

LAST_CHAR_EXIFTOOL_DOMAIN=${EXIFTOOL_DOMAIN:length-1:1}
[[ $LAST_CHAR_EXIFTOOL_DOMAIN != "/" ]] && EXIFTOOL_DOMAIN="$EXIFTOOL_DOMAIN/"; :
USER_AGENT="Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.3029.110 Safari/537.3"

EXIFTOOL_JSON=$(curl -s $EXIFTOOL_CHECKSUMS_API)

# Extract the latest version
LATEST_EXIFTOOL_VERSION=$(echo "$EXIFTOOL_JSON" | grep -o '\b[0-9]\+\.[0-9]\+\b' | head -n 1)


# Extract the Linux tar.gz filename using the dynamically fetched version
LINUX_EXIFTOOL=$(echo "$EXIFTOOL_JSON" | grep -o "Image-ExifTool-${LATEST_EXIFTOOL_VERSION}.tar.gz" | head -n 1)
WINDOWS_EXIFTOOL=$(echo "$EXIFTOOL_JSON" | grep -o "exiftool-${LATEST_EXIFTOOL_VERSION}_64.zip" | head -n 1)


rm -rf $SCRIPT_DIR$BINARY_FOLDERNAME
mkdir -p $SCRIPT_DIR$BINARY_FOLDERNAME
cd $SCRIPT_DIR$BINARY_FOLDERNAME

curl -L -A "$USER_AGENT" -O "$EXIFTOOL_DOMAIN$LINUX_EXIFTOOL"
curl -L -A "$USER_AGENT" -O "$EXIFTOOL_DOMAIN$WINDOWS_EXIFTOOL"
curl -L -A "$USER_AGENT" -O "$EXIFTOOL_CHECKSUMS_API"


echo "Linux tar.gz filename: $LINUX_EXIFTOOL"
echo "Windows zip filename: $WINDOWS_EXIFTOOL"