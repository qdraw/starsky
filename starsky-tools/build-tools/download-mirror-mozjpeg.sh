#!/bin/bash

# for: starsky-dependencies

set -euo pipefail

# List of binaries to download, zip, and hash
BINARIES=(
  "linux-x64|https://github.com/imagemin/mozjpeg-bin/raw/refs/heads/main/vendor/linux/amd64/cjpeg|mozjpeg-linux-x64.zip"
  "linux-arm64|https://github.com/imagemin/mozjpeg-bin/raw/refs/heads/main/vendor/linux/arm64/cjpeg|mozjpeg-linux-arm64.zip"
  "osx-x64|https://github.com/imagemin/mozjpeg-bin/raw/refs/heads/main/vendor/macos/amd64/cjpeg|mozjpeg-osx-x64.zip"
  "osx-arm64|https://github.com/imagemin/mozjpeg-bin/raw/refs/heads/main/vendor/macos/arm64/cjpeg|mozjpeg-osx-arm64.zip"
  "win-x64|https://github.com/mozilla/mozjpeg/releases/download/v4.0.3/mozjpeg-v4.0.3-win-x64.zip|mozjpeg-win-x64.zip"
)

# Output folder setup
SCRIPT_DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" &> /dev/null && pwd )"
BINARY_FOLDERNAME="mirror/mozjpeg"
INDEX_FILE="index.json"
CHECK_FILES=("mozjpeg-linux-x64.zip" "mozjpeg-linux-arm64.zip" "mozjpeg-osx-x64.zip" "mozjpeg-osx-arm64.zip" "mozjpeg-win-x64.zip")

LAST_CHAR_SCRIPT_DIR=${SCRIPT_DIR: -1}
[[ $LAST_CHAR_SCRIPT_DIR != "/" ]] && SCRIPT_DIR="$SCRIPT_DIR/"; :
LAST_CHAR_BINARY_FOLDERNAME=${BINARY_FOLDERNAME: -1}
[[ $LAST_CHAR_BINARY_FOLDERNAME != "/" ]] && BINARY_FOLDERNAME="$BINARY_FOLDERNAME/"; :
INDEX_FILE_PATH=$SCRIPT_DIR$BINARY_FOLDERNAME$INDEX_FILE

# Clean and prepare output folder
echo "Cleaning up previous binaries... $SCRIPT_DIR$BINARY_FOLDERNAME"
rm -rf "$SCRIPT_DIR$BINARY_FOLDERNAME"
mkdir -p "$SCRIPT_DIR$BINARY_FOLDERNAME"
cd "$SCRIPT_DIR$BINARY_FOLDERNAME"

# Download, zip, hash, and collect manifest entries
OUTPUT_JSON='{"binaries":['
FIRST=1
for ENTRY in "${BINARIES[@]}"; do
  ARCH="${ENTRY%%|*}"
  REMAINDER="${ENTRY#*|}"
  URL="${REMAINDER%%|*}"
  ZIPNAME="${REMAINDER#*|}"
  BASENAME=$(basename "$URL")

  echo "Calculating SHA256 for $ZIPNAME ..."
  SHA256=$(openssl dgst -sha256 "$ZIPNAME" | awk '{print $2}')

  # Add to JSON
  if [ $FIRST -eq 0 ]; then OUTPUT_JSON+=","; fi
  OUTPUT_JSON+="{\"architecture\":\"$ARCH\",\"fileName\":\"$ZIPNAME\",\"sha256\":\"$SHA256\"}"
  FIRST=0
done
OUTPUT_JSON+=']}'

# Write manifest
echo "$OUTPUT_JSON" > "$INDEX_FILE_PATH"

if command -v node &> /dev/null
then
  node -e "console.log(JSON.stringify(JSON.parse(require('fs').readFileSync(process.argv[1])), null, 4));" "$INDEX_FILE_PATH" > "$INDEX_FILE_PATH.bak"
  mv "$INDEX_FILE_PATH.bak" "$INDEX_FILE_PATH"
fi

for CHECK_FILE in "${CHECK_FILES[@]}"; do
  FILE_PATH="$SCRIPT_DIR$BINARY_FOLDERNAME$CHECK_FILE"

  if [ ! -f "$FILE_PATH" ]; then
    echo "⛌ FAIL -> $CHECK_FILE does not exist."
    exit 1
  fi

  FILE_SIZE="$(stat -c%s "$FILE_PATH" 2>/dev/null || stat -f%z "$FILE_PATH")"

  if [[ "$CHECK_FILE" == *"linux-x64"* ]]; then
    if [ "$FILE_SIZE" -gt 30000 ]; then
      echo "✅ $CHECK_FILE exists and is larger than 30 KB."
    else
      echo "⛌ FAIL -> $CHECK_FILE exists but is 30 KB or smaller."
      exit 1
    fi
  else
    if [ "$FILE_SIZE" -gt 200000 ]; then
      echo "✅ $CHECK_FILE exists and is larger than 200 KB."
    else
      echo "⛌ FAIL -> $CHECK_FILE exists but is 200 KB or smaller."
      exit 1
    fi
  fi
done

echo "All binaries processed and manifest saved to $INDEX_FILE_PATH"
