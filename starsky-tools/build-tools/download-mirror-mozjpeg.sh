#!/bin/bash

# for: starsky-dependencies

set -euo pipefail

# List of binaries to download, zip, and hash
BINARIES=(
  "linux-arm|https://github.com/qdraw/mozjpeg-binaries/releases/download/v0.0.2/linux-armhf|mozjpeg|mozjpeg-linux-arm.zip"
  "linux-arm64|https://github.com/qdraw/mozjpeg-binaries/releases/download/v0.0.2/linux-arm64|mozjpeg|mozjpeg-linux-arm64.zip"
  "linux-x64|https://github.com/qdraw/mozjpeg-binaries/releases/download/v0.0.2/linux-x64|mozjpeg|mozjpeg-linux-x64.zip"
  "osx-x64|https://github.com/qdraw/mozjpeg-binaries/releases/download/v0.0.2/macos-x64|mozjpeg|mozjpeg-osx-x64.zip"
  "osx-arm64|https://github.com/qdraw/mozjpeg-binaries/releases/download/v0.0.2/macos-arm64|mozjpeg|mozjpeg-osx-arm64.zip"
  "win-x64|https://github.com/qdraw/mozjpeg-binaries/releases/download/v0.0.2/windows-x64.exe|mozjpeg.exe|mozjpeg-win-x64.zip"
)

# Output folder setup
SCRIPT_DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" &> /dev/null && pwd )"
BINARY_FOLDERNAME="mirror/mozjpeg"
INDEX_FILE="index.json"
CHECK_FILES=("mozjpeg-linux-x64.zip" "mozjpeg-linux-arm64.zip" "mozjpeg-linux-arm.zip" "mozjpeg-osx-x64.zip" "mozjpeg-osx-arm64.zip" "mozjpeg-win-x64.zip")

usage() {
  echo "Usage: $0 [--output <directory>] [--help]"
  echo
  echo "Options:"
  echo "  --output <directory>  Output directory for generated artifacts."
  echo "                        Default: \$SCRIPT_DIR/$BINARY_FOLDERNAME"
  echo "  --help                Show this help message and exit."
}

OUTPUT_ARG=""
while [[ $# -gt 0 ]]; do
  case "$1" in
    --output)
      if [[ $# -lt 2 ]]; then
        echo "Error: --output requires a directory argument."
        usage
        exit 1
      fi
      OUTPUT_ARG="$2"
      shift 2
      ;;
    --help)
      usage
      exit 0
      ;;
    *)
      echo "Error: Unknown argument '$1'."
      usage
      exit 1
      ;;
  esac
done

if [[ -n "$OUTPUT_ARG" ]]; then
  if [[ "$OUTPUT_ARG" = /* ]]; then
    OUTPUT_DIR="$OUTPUT_ARG"
  else
    OUTPUT_DIR="$(pwd)/$OUTPUT_ARG"
  fi
else
  OUTPUT_DIR="$SCRIPT_DIR/$BINARY_FOLDERNAME"
fi

INDEX_FILE_PATH="$OUTPUT_DIR/$INDEX_FILE"

# Clean and prepare output folder
echo "Cleaning up previous binaries... $OUTPUT_DIR"
rm -rf "$OUTPUT_DIR"
mkdir -p "$OUTPUT_DIR"
cd "$OUTPUT_DIR"

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
  FILE_PATH="$OUTPUT_DIR/$CHECK_FILE"

  if [ ! -f "$FILE_PATH" ]; then
    echo "⛌ FAIL -> $CHECK_FILE does not exist."
    exit 1
  fi

  FILE_SIZE="$(stat -c%s "$FILE_PATH" 2>/dev/null || stat -f%z "$FILE_PATH")"

  if [ "$FILE_SIZE" -gt 240000 ]; then
    echo "✅ $CHECK_FILE exists and is larger than 240 KB. actual: $FILE_SIZE"
  else
    echo "⛌ FAIL -> $CHECK_FILE exists but is 240 KB or smaller. actual: $FILE_SIZE"
    exit 1
  fi
done

echo "All binaries processed and manifest saved to $INDEX_FILE_PATH"
