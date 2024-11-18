#!/bin/bash

# feature: https://raw.githubusercontent.com/qdraw/starsky/refs/heads/feature/202411_ffmpeg/starsky-tools/build-tools/
GH_SCRIPT_DIR="https://raw.githubusercontent.com/qdraw/starsky/refs/heads/master/starsky-tools/build-tools/"

LAST_CHAR_GH_SCRIPT_DIR=${GH_SCRIPT_DIR:length-1:1}
[[ $LAST_CHAR_GH_SCRIPT_DIR != "/" ]] && GH_SCRIPT_DIR="$GH_SCRIPT_DIR/"; :

SCRIPT_FILES=("download-mirror-exiftool.sh" "download-mirror-ffmpeg.sh" "download-mirror-geonames.sh")

# Loop through the array
for SCRIPT_FILE in "${SCRIPT_FILES[@]}"; do
  echo "Processing $item..."
  # Example action: Check if the file exists
  if [ -f "$SCRIPT_FILE" ]; then
    echo "$SCRIPT_FILE exists."
  else
    echo "$SCRIPT_FILE does not exist."
    curl -o "$SCRIPT_FILE" "$GH_SCRIPT_DIR$SCRIPT_FILE"
  fi
  
  echo "Run $SCRIPT_FILE..."
  bash $SCRIPT_FILE

  EXIT_CODE=$?
  if [ $EXIT_CODE -ne 0 ]; then
    echo "ERROR: $SCRIPT_FILE exited with code $EXIT_CODE"
    exit $EXIT_CODE
  fi
done
