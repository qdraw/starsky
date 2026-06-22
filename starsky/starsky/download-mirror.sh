#!/bin/bash

# Download helper with automatic mirror fallback.
#
# Usage:
#   ./download-mirror.sh --source <url> --mirror <url> --output <file>
# Optional:
#   --retry-seconds <seconds>   (default 3)
#   --timeout-seconds <seconds> (default 30)

set -u

SOURCE_URL=""
MIRROR_URL=""
OUTPUT_FILE=""
RETRY_SECONDS=3
TIMEOUT_SECONDS=30

ARGUMENTS=("$@")
for ((i = 1; i <= $#; i++)); do
  CURRENT=$((i - 1))
  PREV=$((i - 2))

  if [[ ${ARGUMENTS[CURRENT]} == "--help" ]]; then
    echo "--source https://example.com/file.tgz"
    echo "--mirror https://mirror.example.com/file.tgz"
    echo "--output /tmp/file.tgz"
    echo "--retry-seconds 3"
    echo "--timeout-seconds 30"
    exit 0
  fi

  if [[ $i -gt 1 ]]; then
    if [[ ${ARGUMENTS[PREV]} == "--source" ]]; then
      SOURCE_URL="${ARGUMENTS[CURRENT]}"
    fi
    if [[ ${ARGUMENTS[PREV]} == "--mirror" ]]; then
      MIRROR_URL="${ARGUMENTS[CURRENT]}"
    fi
    if [[ ${ARGUMENTS[PREV]} == "--output" ]]; then
      OUTPUT_FILE="${ARGUMENTS[CURRENT]}"
    fi
    if [[ ${ARGUMENTS[PREV]} == "--retry-seconds" ]]; then
      RETRY_SECONDS="${ARGUMENTS[CURRENT]}"
    fi
    if [[ ${ARGUMENTS[PREV]} == "--timeout-seconds" ]]; then
      TIMEOUT_SECONDS="${ARGUMENTS[CURRENT]}"
    fi
  fi
done

if [[ -z "$SOURCE_URL" || -z "$MIRROR_URL" || -z "$OUTPUT_FILE" ]]; then
  echo "FAIL: missing required arguments. Use --help"
  exit 1
fi

OUTPUT_DIR=$(dirname "$OUTPUT_FILE")
mkdir -p "$OUTPUT_DIR"

download_url() {
  local url=$1
  local output_file=$2

  if command -v curl >/dev/null 2>&1; then
    curl -fL --retry 2 --retry-delay "$RETRY_SECONDS" --connect-timeout "$TIMEOUT_SECONDS" \
      --silent --show-error "$url" -o "$output_file"
    return $?
  fi

  if command -v wget >/dev/null 2>&1; then
    wget -q --timeout="$TIMEOUT_SECONDS" --tries=3 "$url" -O "$output_file"
    return $?
  fi

  echo "FAIL: neither curl nor wget is available"
  return 1
}

TEMP_FILE="${OUTPUT_FILE}.tmp"
rm -f "$TEMP_FILE"

echo "Try source: $SOURCE_URL"
if download_url "$SOURCE_URL" "$TEMP_FILE" && [[ -s "$TEMP_FILE" ]]; then
  mv "$TEMP_FILE" "$OUTPUT_FILE"
  echo "SUCCESS: downloaded from source"
  exit 0
fi

echo "WARN: source download failed, try mirror: $MIRROR_URL"
rm -f "$TEMP_FILE"

if download_url "$MIRROR_URL" "$TEMP_FILE" && [[ -s "$TEMP_FILE" ]]; then
  mv "$TEMP_FILE" "$OUTPUT_FILE"
  echo "SUCCESS: downloaded from mirror"
  exit 0
fi

rm -f "$TEMP_FILE"
echo "FAIL: source and mirror download failed"
exit 1

