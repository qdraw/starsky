#!/usr/bin/env bash
# Submits a DMG to Apple's notarization service and staples the ticket.
#
# Usage:
#   ./notarize.sh <dmg> <apple-id> <team-id> <notarytool-password>
#
# Arguments:
#   dmg                  - Path to the .dmg file to notarize
#   apple-id             - Your Apple ID email (e.g. you@example.com)
#   team-id              - Your 10-character Apple Developer Team ID
#   notarytool-password  - App-specific password from appleid.apple.com
set -euo pipefail

DMG="${1:-}"
APPLE_ID="${2:-}"
APPLE_TEAM_ID="${3:-}"
NOTARYTOOL_PASSWORD="${4:-}"

if [[ -z "$DMG" || -z "$APPLE_ID" || -z "$APPLE_TEAM_ID" || -z "$NOTARYTOOL_PASSWORD" ]]; then
  echo "Usage: $0 <dmg> <apple-id> <team-id> <notarytool-password>" >&2
  exit 1
fi

if [[ ! -f "$DMG" ]]; then
  echo "Error: file not found: $DMG" >&2
  exit 1
fi

echo "==> Submitting for notarization: $DMG"
xcrun notarytool submit "$DMG" \
  --apple-id "$APPLE_ID" \
  --team-id "$APPLE_TEAM_ID" \
  --password "$NOTARYTOOL_PASSWORD" \
  --wait

echo "==> Stapling ticket to: $DMG"
xcrun stapler staple "$DMG"

echo "==> Verifying Gatekeeper acceptance"
spctl -a -vvv -t install "$DMG"

echo "==> Done: $DMG"
