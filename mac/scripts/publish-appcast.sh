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

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_FILE="$SCRIPT_DIR/../project.yml"

SPARKLE_VERSION="$(awk '
  $0 == "  Sparkle:" { in_sparkle = 1; next }
  in_sparkle && /^[^[:space:]]/ { exit }
  in_sparkle && /^[[:space:]]+from:[[:space:]]*/ {
    sub(/^[[:space:]]+from:[[:space:]]*/, "")
    print
    exit
  }
' "$PROJECT_FILE")"

if [[ -z "$SPARKLE_VERSION" ]]; then
  echo "Error: Sparkle version not found in $PROJECT_FILE." >&2
  SPARKLE_VERSION="2.9.6"
fi

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
  > "$PRIVATE_KEY_FILE"

# --- Sign the DMG ---
echo "==> Signing DMG: $DMG_PATH"
ED_SIGNATURE_AND_LENGTH="$("$SPARKLE_DIR/bin/sign_update" "$DMG_PATH" --ed-key-file "$PRIVATE_KEY_FILE")"
if [[ ! "$ED_SIGNATURE_AND_LENGTH" =~ ^sparkle:edSignature=\"[A-Za-z0-9+/]+={0,2}\"[[:space:]]+length=\"[0-9]+\"$ ]]; then
  echo "Error: sign_update returned an invalid EdDSA signature and file length." >&2
  exit 1
fi

# --- Build appcast XML ---
DMG_NAME="$(basename "$DMG_PATH")"
DOWNLOAD_URL="https://github.com/qdraw/starsky/releases/download/v${VERSION}/${DMG_NAME}"
PUB_DATE="$(LC_ALL=C date -u "+%a, %d %b %Y %H:%M:%S +0000")"

echo "$ED_SIGNATURE_AND_LENGTH"
echo "--"

cat > "$OUTPUT_PATH" <<XML
<?xml version="1.0" encoding="utf-8"?>
<rss version="2.0" xmlns:sparkle="http://www.andymatuschak.org/xml-namespaces/sparkle">
  <channel>
    <title>Starsky</title>
    <item>
      <title>${VERSION}</title>
      <pubDate>${PUB_DATE}</pubDate>
      <sparkle:version>${VERSION}</sparkle:version>
      <sparkle:shortVersionString>${VERSION}</sparkle:shortVersionString>
      <sparkle:minimumSystemVersion>13.0.0</sparkle:minimumSystemVersion>
      <enclosure
        url="${DOWNLOAD_URL}"
        ${ED_SIGNATURE_AND_LENGTH}
        type="application/octet-stream" />
    </item>
  </channel>
</rss>
XML

echo "==> Appcast written to: $OUTPUT_PATH"
cat "$OUTPUT_PATH"

rm -rf "$SPARKLE_DIR" "$PRIVATE_KEY_FILE"
echo "done"