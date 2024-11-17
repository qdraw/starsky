#!/bin/bash
FFBINARIES_API="https://ffbinaries.com/api/v1/version/6.1"
OSX_ARM64_URL="https://www.osxexperts.net/ffmpeg71arm.zip"
OSX_ARM64_NAME="osx-arm64"
SCRIPT_DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" &> /dev/null && pwd )"
BINARY_FOLDERNAME="mirror/ffmpeg"
INDEX_FILE="index.json"

LAST_CHAR_SCRIPT_DIR=${SCRIPT_DIR:length-1:1}
[[ $LAST_CHAR_SCRIPT_DIR != "/" ]] && SCRIPT_DIR="$SCRIPT_DIR/"; :

LAST_CHAR_BINARY_FOLDERNAME=${BINARY_FOLDERNAME:length-1:1}
[[ $LAST_CHAR_BINARY_FOLDERNAME != "/" ]] && BINARY_FOLDERNAME="$BINARY_FOLDERNAME/"; :

INDEX_FILE_PATH=$SCRIPT_DIR$BINARY_FOLDERNAME$INDEX_FILE


# Fetch the JSON data
FFBINARIES_JSON=$(curl -s $FFBINARIES_API)

# Create a directory to store the binaries
echo "Cleaning up previous binaries... $SCRIPT_DIR$BINARY_FOLDERNAME"
rm -rf $SCRIPT_DIR$BINARY_FOLDERNAME
mkdir -p $SCRIPT_DIR$BINARY_FOLDERNAME
cd $SCRIPT_DIR$BINARY_FOLDERNAME

# Initialize JSON output
OUTPUT_JSON="{\"binaries\":["

ARCHITECTURES=$(echo "$FFBINARIES_JSON" | grep -o '"[^"]\+":{"ffmpeg"' | sed 's/"\([^"]\+\)".*/\1/')
BINARY_URLS=$(echo "$FFBINARIES_JSON" | grep -o '"ffmpeg":"[^"]*"' | sed 's/"ffmpeg":"//;s/"//')

MAP_ARCHITECTURE_NAME () {
  if [ "$1" == "windows-64" ]; then
    echo "windows-x64"
  elif [ "$1" == "linux-64" ]; then
    echo "linux-x64"
  elif [ "$1" == "linux-armhf" ]; then
    echo "linux-arm"
  elif [ "$1" == "osx-64" ]; then
    echo "osx-x64"
  else
    echo $1
  fi
}

# populate_array_from_variable() {
#   local variable_content="$1"
#   local array_name="$2"

#   while IFS= read -r line; do
#     eval "$array_name+=(\"\$line\")"
#   done < <(echo "$variable_content")
# }

populate_array_from_variable() {
  local variable_content="$1"
  local array_name="$2"

  IFS=$'\n' read -rd '' -a temp_array <<< "$variable_content"
  eval "$array_name=(\"\${temp_array[@]}\")"
}

# Initialize arrays
ARCHITECTURES_ARRAY=()
BINARY_URLS_ARRAY=()

# Populate arrays using the function
populate_array_from_variable "$ARCHITECTURES" ARCHITECTURES_ARRAY
populate_array_from_variable "$BINARY_URLS" BINARY_URLS_ARRAY


for i in "${!ARCHITECTURES_ARRAY[@]}"; do

  ARCHITECTURE="${ARCHITECTURES_ARRAY[$i]}"
  URL="${BINARY_URLS_ARRAY[$i]}"

  # Extract architecture and URL
  FILENAME=$(basename "$URL")
  NEW_VERSION=""
  FILENAME_UPDATED=$(echo "$FILENAME" | sed -E "s/(-|\.)([0-9]+\.[0-9]+)(-|\.|$)/\1$NEW_VERSION\3/")

  CURRENT_ARCHITECTURE=$(echo "$ARCHITECTURE" | sed -n 's/.*"\([^"]*\)":{.*ffmpeg.*/\1/p')

  # skip if linux-armel or linux-32
  if [ "$CURRENT_ARCHITECTURE" == "linux-32" ] || [ "$CURRENT_ARCHITECTURE" == "linux-armel" ]; then
    continue
  fi

  CURRENT_ARCHITECTURE="$(MAP_ARCHITECTURE_NAME $CURRENT_ARCHITECTURE)"

  # Download the binary
  echo "Downloading $URL for $CURRENT_ARCHITECTURE $FILENAME..."
  curl -L -O "$URL"

  FILE_HASH=$(openssl dgst -sha512 "$FILENAME" | awk '{print $2}')

  mv "$FILENAME" "$FILENAME_UPDATED"

  # Add to output JSON
  OUTPUT_JSON="${OUTPUT_JSON}{\"architecture\":\"$CURRENT_ARCHITECTURE\",\"url\":\"$FILENAME_UPDATED\",\"sha512\":\"$FILE_HASH\"},"
done

# Add osx-arm64 explicitly
echo "Adding custom architecture $OSX_ARM64_NAME with URL $OSX_ARM64_URL..."
curl -L -O "$OSX_ARM64_URL"

OSX_ARM64_FILENAME=$(basename "$OSX_ARM64_URL")
OSX_ARM64_HASH=$(openssl dgst -sha512 "$OSX_ARM64_FILENAME" | awk '{print $2}')
OUTPUT_JSON="${OUTPUT_JSON}{\"architecture\":\"$OSX_ARM64_NAME\",\"url\":\"$OSX_ARM64_FILENAME\",\"sha512\":\"$OSX_ARM64_HASH\"},"

# Finalize JSON
OUTPUT_JSON="${OUTPUT_JSON%,}]}" # Remove trailing comma and close the JSON array

# Write JSON to file
echo "$OUTPUT_JSON" > $INDEX_FILE_PATH

echo "All ffmpeg binaries downloaded successfully."

node -e "console.log(JSON.stringify(JSON.parse(require('fs') \
      .readFileSync(process.argv[1])), null, 4));" $INDEX_FILE_PATH > $INDEX_FILE_PATH.bak
mv $INDEX_FILE_PATH.bak $INDEX_FILE_PATH

echo "URLs with architectures saved to $INDEX_FILE_PATH"
