#!/usr/bin/env bash
# Generates a Sparkle appcast XML for a signed DMG.
#
# Usage:
#   ./publish-appcast.sh <dmg-path> <version> [output-path]
#
# Arguments:
#   dmg-path     - Path to the notarized .dmg file
#   version      - Release version string (e.g. 0.9.0-beta.0), without leading 'v'
#   output-path  - Where to write the appcast XML (default: appcast-macos.xml)
#
# Required env vars:
#   SPARKLE_PRIVATE_ED_KEY  - Base64-encoded Sparkle EdDSA private key file
set -euo pipefail

DMG_PATH="${1:?Usage: publish-appcast.sh <dmg-path> <version> [output-path]}"
VERSION="${2:?Usage: publish-appcast.sh <dmg-path> <version> [output-path]}"
OUTPUT_PATH="${3:-appcast-macos.xml}"
SPARKLE_VERSION="2.9.6"

if [[ ! -f "$DMG_PATH" ]]; then
  echo "Error: DMG not found: $DMG_PATH" >&2
  exit 1
fi

# --- Download Sparkle tools ---
SPARKLE_DIR="$(mktemp -d)"
PRIVATE_KEY_FILE="$(mktemp)"
trap 'rm -rf "$SPARKLE_DIR" "$PRIVATE_KEY_FILE"' EXIT

echo "==> Downloading Sparkle ${SPARKLE_VERSION}"
curl -fsSL \
  "https://github.com/sparkle-project/Sparkle/releases/download/${SPARKLE_VERSION}/Sparkle-${SPARKLE_VERSION}.tar.xz" \
  | tar -xJ -C "$SPARKLE_DIR"

# --- Write private key ---
printf '%s' "${SPARKLE_PRIVATE_ED_KEY:?SPARKLE_PRIVATE_ED_KEY is required}" \
  | base64 --decode > "$PRIVATE_KEY_FILE"

# --- Sign the DMG ---
echo "==> Signing DMG: $DMG_PATH"
ED_SIGNATURE="$("$SPARKLE_DIR/bin/sign_update" "$DMG_PATH" --ed-key-file "$PRIVATE_KEY_FILE")"

# --- Build appcast XML ---
FILE_SIZE="$(stat -f%z "$DMG_PATH")"
DMG_NAME="$(basename "$DMG_PATH")"
DOWNLOAD_URL="https://github.com/qdraw/starsky/releases/download/v${VERSION}/${DMG_NAME}"
PUB_DATE="$(date -u "+%a, %d %b %Y %H:%M:%S +0000")"

cat > "$OUTPUT_PATH" <<XML
<?xml version="1.0" encoding="utf-8"?>
<rss version="2.0" xmlns:sparkle="http://www.andymatuschak.org/xml-namespaces/sparkle">
  <channel>
    <title>Starsky</title>
    <item>
      <title>${VERSION}</title>
      <pubDate>${PUB_DATE}</pubDate>
      <sparkle:minimumSystemVersion>13.0</sparkle:minimumSystemVersion>
      <enclosure
        url="${DOWNLOAD_URL}"
        sparkle:version="${VERSION}"
        sparkle:shortVersionString="${VERSION}"
        sparkle:edSignature="${ED_SIGNATURE}"
        length="${FILE_SIZE}"
        type="application/octet-stream" />
    </item>
  </channel>
</rss>
XML

echo "==> Appcast written to: $OUTPUT_PATH"
cat "$OUTPUT_PATH"
