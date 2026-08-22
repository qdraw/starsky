#!/bin/bash
GEONAMES_DUMP="https://download.geonames.org/export/dump/"
BINARY_FOLDERNAME="mirror/geonames"
ADMIN1_CODES="admin1CodesASCII.txt"
CITIES1000="cities1000.zip"

CHECK_FILES=("$ADMIN1_CODES" "$CITIES1000")

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
      echo "                         Relative paths resolve under \$SCRIPT_DIR"
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

SCRIPT_DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" &> /dev/null && pwd )"
LAST_CHAR_SCRIPT_DIR=${SCRIPT_DIR:length-1:1}
[[ $LAST_CHAR_SCRIPT_DIR != "/" ]] && SCRIPT_DIR="$SCRIPT_DIR/"; :

LAST_CHAR_GEONAMES_DUMP=${GEONAMES_DUMP:length-1:1}
[[ $LAST_CHAR_GEONAMES_DUMP != "/" ]] && GEONAMES_DUMP="$GEONAMES_DUMP/"; :
LAST_CHAR_BINARY_FOLDERNAME=${BINARY_FOLDERNAME:length-1:1}
[[ $LAST_CHAR_BINARY_FOLDERNAME != "/" ]] && BINARY_FOLDERNAME="$BINARY_FOLDERNAME/"; :

if [[ "$BINARY_FOLDERNAME" = /* ]]; then
  OUTPUT_DIRECTORY="$BINARY_FOLDERNAME"
else
  OUTPUT_DIRECTORY="$SCRIPT_DIR$BINARY_FOLDERNAME"
fi

rm -rf "$OUTPUT_DIRECTORY"
mkdir -p "$OUTPUT_DIRECTORY"
cd "$OUTPUT_DIRECTORY"

echo "Downloading GeoNames files... ""$GEONAMES_DUMP$ADMIN1_CODES"
curl -L -O "$GEONAMES_DUMP$ADMIN1_CODES"

echo "Downloading GeoNames files... ""$GEONAMES_DUMP$CITIES1000"
curl -L -O "$GEONAMES_DUMP$CITIES1000"

for CHECK_FILE in "${CHECK_FILES[@]}"; do
  FULL_PATH="${OUTPUT_DIRECTORY}${CHECK_FILE}"
  
  # Check if the file exists
  if [ ! -f "$FULL_PATH" ]; then
    echo "$CHECK_FILE does not exist."
    echo "                 $FULL_PATH is missing."
    exit 1
  fi
  
  # Get file size
  FILE_SIZE=$(stat -c%s "$FULL_PATH" 2>/dev/null || stat -f%z "$FULL_PATH")

  # Check conditions
  if [ "$FILE_SIZE" -gt 142000 ] && [ "$CHECK_FILE" == "$ADMIN1_CODES" ]; then
    echo "✅ $CHECK_FILE contains $ADMIN1_CODES and is at least 142 KB /0.13mb."
  elif [ "$FILE_SIZE" -gt 9300000 ]; then
    echo "✅ $CHECK_FILE exists and is larger than 8 MB."
  else
    echo "$CHECK_FILE exists but does not meet size requirements."
    exit 1
  fi
done
