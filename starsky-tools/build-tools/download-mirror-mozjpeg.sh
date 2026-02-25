#!/bin/bash

# for: starsky-dependencies

set -euo pipefail

# List of binaries to download, zip, and hash
BINARIES=(
  "linux-arm64|https://github.com/qdraw/mozjpeg-binaries/releases/download/v0.0.1/linux-arm64|mozjpeg|mozjpeg-linux-arm64.zip"
  "linux-x64|https://github.com/qdraw/mozjpeg-binaries/releases/download/v0.0.1/linux-x64|mozjpeg|mozjpeg-linux-x64.zip"
  "osx-x64|https://github.com/qdraw/mozjpeg-binaries/releases/download/v0.0.1/macos-x64|mozjpeg|mozjpeg-osx-x64.zip"
  "osx-arm64|https://github.com/qdraw/mozjpeg-binaries/releases/download/v0.0.1/macos-arm64|mozjpeg|mozjpeg-osx-arm64.zip"
  "win-x64|https://github.com/qdraw/mozjpeg-binaries/releases/download/v0.0.1/windows-x64.exe|mozjpeg.exe|mozjpeg-win-x64.zip"
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
  REMAINDER2="${REMAINDER#*|}"
  ZIPNAME="${REMAINDER2##*|}"
  FILENAME_INSIDE_ZIP="${REMAINDER2%%|*}"
  BASENAME=$(basename "$URL")

  echo "Downloading $URL ..."
  curl -L -o "$BASENAME" "$URL"

  # Rename to the desired filename inside the zip if needed
  if [ "$BASENAME" != "$FILENAME_INSIDE_ZIP" ]; then
    mv "$BASENAME" "$FILENAME_INSIDE_ZIP"
  fi

  echo "Zipping $FILENAME_INSIDE_ZIP to $ZIPNAME ..."
  zip -q "$ZIPNAME" "$FILENAME_INSIDE_ZIP"

  echo "Calculating SHA256 for $ZIPNAME ..."
  SHA256=$(openssl dgst -sha256 "$ZIPNAME" | awk '{print $2}')

  rm -f "$FILENAME_INSIDE_ZIP"

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

  if [ "$FILE_SIZE" -gt 260000 ]; then
    echo "✅ $CHECK_FILE exists and is larger than 260 KB. actual: $FILE_SIZE"
  else
    echo "⛌ FAIL -> $CHECK_FILE exists but is 260 KB or smaller."
    exit 1
  fi
done

echo "All binaries processed and manifest saved to $INDEX_FILE_PATH"
