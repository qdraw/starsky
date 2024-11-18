#!/bin/bash

EXIFTOOL_DOMAIN="https://exiftool.org"
EXIFTOOL_CHECKSUMS_API=$EXIFTOOL_DOMAIN"/checksums.txt"
BINARY_FOLDERNAME="mirror/exiftool"
INDEX_FILE="checksums.txt"

LAST_CHAR_EXIFTOOL_DOMAIN=${EXIFTOOL_DOMAIN:length-1:1}
[[ $LAST_CHAR_EXIFTOOL_DOMAIN != "/" ]] && EXIFTOOL_DOMAIN="$EXIFTOOL_DOMAIN/"; :
USER_AGENT="Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.3029.110 Safari/537.3"

SCRIPT_DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" &> /dev/null && pwd )"
LAST_CHAR_SCRIPT_DIR=${SCRIPT_DIR:length-1:1}
[[ $LAST_CHAR_SCRIPT_DIR != "/" ]] && SCRIPT_DIR="$SCRIPT_DIR/"; :

LAST_CHAR_SCRIPT_DIR=${SCRIPT_DIR:length-1:1}
[[ $LAST_CHAR_SCRIPT_DIR != "/" ]] && SCRIPT_DIR="$SCRIPT_DIR/"; :

LAST_CHAR_BINARY_FOLDERNAME=${BINARY_FOLDERNAME:length-1:1}
[[ $LAST_CHAR_BINARY_FOLDERNAME != "/" ]] && BINARY_FOLDERNAME="$BINARY_FOLDERNAME/"; :

# Fetch the JSON data
EXIFTOOL_JSON=$(curl -s $EXIFTOOL_CHECKSUMS_API)

if ! echo "$EXIFTOOL_JSON" | grep -q "SHA256"; then
  echo "Display contents of $EXIFTOOL_CHECKSUMS_API"
  echo $EXIFTOOL_JSON
  echo "⛌ FAIL -> The word 'SHA256' is not present in the checksums data. Exiting..."
  exit 1
fi

# Extract the latest version
LATEST_EXIFTOOL_VERSION=$(echo "$EXIFTOOL_JSON" | grep -o '\b[0-9]\+\.[0-9]\+\b' | head -n 1)

# Extract the Linux tar.gz filename using the dynamically fetched version
LINUX_EXIFTOOL=$(echo "$EXIFTOOL_JSON" | grep -o "Image-ExifTool-${LATEST_EXIFTOOL_VERSION}.tar.gz" | head -n 1)
WINDOWS_EXIFTOOL=$(echo "$EXIFTOOL_JSON" | grep -o "exiftool-${LATEST_EXIFTOOL_VERSION}_64.zip" | head -n 1)

CHECK_FILES=($LINUX_EXIFTOOL $WINDOWS_EXIFTOOL)

echo "Cleaning up previous binaries... $SCRIPT_DIR$BINARY_FOLDERNAME"
rm -rf $SCRIPT_DIR$BINARY_FOLDERNAME

echo "Create new directory... $SCRIPT_DIR$BINARY_FOLDERNAME"
mkdir -p $SCRIPT_DIR$BINARY_FOLDERNAME
cd $SCRIPT_DIR$BINARY_FOLDERNAME

curl -L -A "$USER_AGENT" -O "$EXIFTOOL_DOMAIN$LINUX_EXIFTOOL"
curl -L -A "$USER_AGENT" -O "$EXIFTOOL_DOMAIN$WINDOWS_EXIFTOOL"
curl -L -A "$USER_AGENT" -O "$EXIFTOOL_CHECKSUMS_API"


if [ ${#CHECK_FILES[@]} -ne 2 ]; then
  echo "⛌ FAIL CHECK_FILES does not contain exactly two items. Exiting..."
  exit 1
fi

for CHECK_FILE in "${CHECK_FILES[@]}"; do
  if [ -f "$SCRIPT_DIR$BINARY_FOLDERNAME$CHECK_FILE" ] && [ "$(stat -c%s "$SCRIPT_DIR$BINARY_FOLDERNAME$CHECK_FILE" 2>/dev/null || stat -f%z "$SCRIPT_DIR$BINARY_FOLDERNAME$CHECK_FILE")" -gt 6300000 ]; then
    echo "✅ $CHECK_FILE exists and is larger than 7 MB."
  elif [ -f "$SCRIPT_DIR$BINARY_FOLDERNAME$CHECK_FILE" ]; then
    echo "⛌ FAIL -> $CHECK_FILE exists but is 7 MB or smaller."
    exit 1
  else
    echo "⛌ FAIL -> $CHECK_FILE does not exist."
    echo "                 $SCRIPT_DIR$BINARY_FOLDERNAME$CHECK_FILE is missing."
    exit 1
  fi
done