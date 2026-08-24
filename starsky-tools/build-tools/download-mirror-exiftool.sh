#!/bin/bash

EXIFTOOL_DOMAIN="https://exiftool.org"
EXIFTOOL_CHECKSUMS_API=$EXIFTOOL_DOMAIN"/checksums.txt"
SOURCEFORGE_EXIFTOOL_FILES_BASE="https://sourceforge.net/projects/exiftool/files/"
SOURCEFORGE_EXIFTOOL_POSTFIX="/download"
BINARY_FOLDERNAME="mirror/exiftool"
INDEX_FILE="checksums.txt"

while [[ $# -gt 0 ]]; do
  case "$1" in
    --output)
      shift
      if [ -z "$1" ]; then
        echo "Missing value for --output"
        exit 1
      fi
      BINARY_FOLDERNAME="$1"
      ;;
    --help|-h)
      echo "Usage: $0 [--output <directory>]"
      echo "  --output <directory>   Output directory (default: mirror/exiftool)"
      exit 0
      ;;
    *)
      echo "Unknown argument: $1"
      echo "Use --help to see available options."
      exit 1
      ;;
  esac
  shift
done

LAST_CHAR_EXIFTOOL_DOMAIN=${EXIFTOOL_DOMAIN:length-1:1}
[[ $LAST_CHAR_EXIFTOOL_DOMAIN != "/" ]] && EXIFTOOL_DOMAIN="$EXIFTOOL_DOMAIN/"; :
USER_AGENT="Wget/1.21.1"

SCRIPT_DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" &> /dev/null && pwd )"
LAST_CHAR_SCRIPT_DIR=${SCRIPT_DIR:length-1:1}
[[ $LAST_CHAR_SCRIPT_DIR != "/" ]] && SCRIPT_DIR="$SCRIPT_DIR/"; :

LAST_CHAR_SCRIPT_DIR=${SCRIPT_DIR:length-1:1}
[[ $LAST_CHAR_SCRIPT_DIR != "/" ]] && SCRIPT_DIR="$SCRIPT_DIR/"; :

LAST_CHAR_BINARY_FOLDERNAME=${BINARY_FOLDERNAME:length-1:1}
[[ $LAST_CHAR_BINARY_FOLDERNAME != "/" ]] && BINARY_FOLDERNAME="$BINARY_FOLDERNAME/"; :

if [[ "$BINARY_FOLDERNAME" = /* ]]; then
  OUTPUT_DIRECTORY="$BINARY_FOLDERNAME"
else
  OUTPUT_DIRECTORY="$SCRIPT_DIR$BINARY_FOLDERNAME"
fi

# Fetch checksums data
EXIFTOOL_JSON=$(curl -s $EXIFTOOL_CHECKSUMS_API)

if ! echo "$EXIFTOOL_JSON" | grep -q "SHA2-256"; then
  echo "Display contents of $EXIFTOOL_CHECKSUMS_API"
  echo $EXIFTOOL_JSON
  echo "⛌ FAIL -> The word 'SHA2-256' is not present in the checksums data. Exiting..."
  exit 1
fi

# Extract the latest version
LATEST_EXIFTOOL_VERSION=$(echo "$EXIFTOOL_JSON" | grep -o '\b[0-9]\+\.[0-9]\+\b' | head -n 1)

# Extract the Linux tar.gz filename using the dynamically fetched version
LINUX_EXIFTOOL=$(echo "$EXIFTOOL_JSON" | grep -o "Image-ExifTool-${LATEST_EXIFTOOL_VERSION}.tar.gz" | head -n 1)
WINDOWS_EXIFTOOL=$(echo "$EXIFTOOL_JSON" | grep -o "exiftool-${LATEST_EXIFTOOL_VERSION}_64.zip" | head -n 1)

CHECK_FILES=($LINUX_EXIFTOOL $WINDOWS_EXIFTOOL)

echo "Cleaning up previous binaries... $OUTPUT_DIRECTORY"
rm -rf "$OUTPUT_DIRECTORY"

echo "Create new directory... $OUTPUT_DIRECTORY"
mkdir -p "$OUTPUT_DIRECTORY"
cd "$OUTPUT_DIRECTORY"

echo "GO TO: " $SOURCEFORGE_EXIFTOOL_FILES_BASE$LINUX_EXIFTOOL$SOURCEFORGE_EXIFTOOL_POSTFIX
curl -L -A "$USER_AGENT" -o "$LINUX_EXIFTOOL" "$SOURCEFORGE_EXIFTOOL_FILES_BASE$LINUX_EXIFTOOL$SOURCEFORGE_EXIFTOOL_POSTFIX"

echo "GO TO: " $SOURCEFORGE_EXIFTOOL_FILES_BASE$WINDOWS_EXIFTOOL$SOURCEFORGE_EXIFTOOL_POSTFIX
curl -L -A "$USER_AGENT" -o "$WINDOWS_EXIFTOOL" "$SOURCEFORGE_EXIFTOOL_FILES_BASE$WINDOWS_EXIFTOOL$SOURCEFORGE_EXIFTOOL_POSTFIX"

echo "GO TO: " $EXIFTOOL_CHECKSUMS_API
curl -L -A "$USER_AGENT" -O "$EXIFTOOL_CHECKSUMS_API"

if [ ${#CHECK_FILES[@]} -ne 2 ]; then
  echo "⛌ FAIL CHECK_FILES does not contain exactly two items. Exiting..."
  exit 1
fi

for CHECK_FILE in "${CHECK_FILES[@]}"; do
  if [ -f "$OUTPUT_DIRECTORY$CHECK_FILE" ] && [ "$(stat -c%s "$OUTPUT_DIRECTORY$CHECK_FILE" 2>/dev/null || stat -f%z "$OUTPUT_DIRECTORY$CHECK_FILE")" -gt 6300000 ]; then
    echo "✅ $CHECK_FILE exists and is larger than 7 MB."
  elif [ -f "$OUTPUT_DIRECTORY$CHECK_FILE" ]; then
    echo "⛌ FAIL -> $CHECK_FILE exists but is 7 MB or smaller."
    exit 1
  else
    echo "⛌ FAIL -> $CHECK_FILE does not exist."
    echo "                 $OUTPUT_DIRECTORY$CHECK_FILE is missing."
    exit 1
  fi
done