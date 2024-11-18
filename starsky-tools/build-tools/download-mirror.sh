#!/bin/bash

GH_SCRIPT_DIR="https://raw.githubusercontent.com/qdraw/starsky/refs/heads/feature/202411_ffmpeg/starsky-tools/build-tools/"

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
  bash $SCRIPT_FILE
done
