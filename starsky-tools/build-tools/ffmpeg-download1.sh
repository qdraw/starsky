#!/bin/bash
FFBINARIES_API="https://ffbinaries.com/api/v1/version/6.1"
OSX_ARM64="https://www.osxexperts.net/ffmpeg71arm.zip"

# Fetch the JSON data
json=$(curl -s $FFBINARIES_API)

# Create a directory to store the binaries
mkdir -p ffmpeg_binaries
cd ffmpeg_binaries

# Initialize JSON output
output_json="{\"binaries\":["

urls=$(echo "$json" | grep -o '"ffmpeg":"[^"]*"' | sed 's/"ffmpeg":"//;s/"//')

for url in $urls; do
  # Extract architecture and URL
  architecture=$(echo "$url" | sed -n 's/.*-\([a-z0-9-]*-[a-z0-9]*\)\.zip/\1/p')
  filename=$(basename "$url")

  # Download the binary
  echo "Downloading $url for $architecture $filename..."
  curl -L -O "$url"

  # Add to output JSON
  output_json="${output_json}{\"architecture\":\"$architecture\",\"url\":\"$filename\"},"
done


# Add osx-arm64 explicitly
custom_arch="osx-arm64"
echo "Adding custom architecture $custom_arch with URL $custom_url..."
curl -L -O "$FFBINARIES_API"
output_json="${output_json}{\"architecture\":\"$custom_arch\",\"url\":\"$custom_url\"},"

# Finalize JSON
output_json="${output_json%,}]}" # Remove trailing comma and close the JSON array

# Write JSON to file
echo "$output_json" > ffmpeg_urls.json

echo "All ffmpeg binaries downloaded successfully."
echo "URLs with architectures saved to ffmpeg_binaries/ffmpeg_urls.json"
