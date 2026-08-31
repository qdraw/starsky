#!/usr/bin/env bash
# Wipes the Xcode DerivedData for this project and triggers a clean rebuild.
# Use when Xcode shows "disk I/O error", "output file map not found", or similar
# build-database corruption errors.

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
PROJECT_NAME="starsky"

echo "Finding DerivedData for $PROJECT_NAME..."
DERIVED=$(find ~/Library/Developer/Xcode/DerivedData -maxdepth 1 -name "${PROJECT_NAME}-*" -type d 2>/dev/null | head -1)

if [ -n "$DERIVED" ]; then
    echo "Removing: $DERIVED"
    rm -rf "$DERIVED"
    echo "DerivedData cleared."
else
    echo "No DerivedData found for $PROJECT_NAME — nothing to remove."
fi

cd "$SCRIPT_DIR"
echo "Resolving package dependencies..."
xcodebuild -resolvePackageDependencies -scheme starsky -quiet
echo "Rebuilding..."
xcodebuild build -scheme starsky -destination 'platform=macOS' -quiet
echo "Done."
