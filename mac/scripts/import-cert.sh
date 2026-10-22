#!/usr/bin/env bash
# Imports a Developer ID Application certificate into a short-lived CI keychain
# so that xcodebuild can sign the app without user interaction.
#
# Required environment variables:
#   MACOS_CERTIFICATE     - Base64-encoded .p12 certificate bundle
#   MACOS_CERTIFICATE_PWD - Password that protects the .p12 file
#   MACOS_KEYCHAIN_PASSWORD     - Password for the temporary keychain created here
#   RUNNER_TEMP           - Temp directory (set automatically by GitHub Actions)
set -euo pipefail

# Validate required environment variables are present and non-empty
for var in MACOS_CERTIFICATE MACOS_CERTIFICATE_PWD MACOS_KEYCHAIN_PASSWORD RUNNER_TEMP; do
  if [[ -z "${!var:-}" ]]; then
    echo "Error: required environment variable '$var' is not set or empty" >&2
    exit 1
  fi
done

CERT_FILE="$RUNNER_TEMP/certificate.p12"
KEYCHAIN_NAME="build.keychain"

# Decode the base64-encoded certificate to a temp file
echo "$MACOS_CERTIFICATE" | base64 --decode > "$CERT_FILE"
if [[ ! -s "$CERT_FILE" ]]; then
  echo "Error: decoded certificate file is empty — check that MACOS_CERTIFICATE is valid base64" >&2
  exit 1
fi

# Create a temporary keychain that only lives for this CI run (6-hour timeout)
security create-keychain -p "$MACOS_KEYCHAIN_PASSWORD" "$KEYCHAIN_NAME"
security set-keychain-settings -lut 21600 "$KEYCHAIN_NAME"
security unlock-keychain -p "$MACOS_KEYCHAIN_PASSWORD" "$KEYCHAIN_NAME"

# Import the certificate; -T grants codesign access without an interactive prompt
security import "$CERT_FILE" -k "$KEYCHAIN_NAME" \
  -P "$MACOS_CERTIFICATE_PWD" -T /usr/bin/codesign

# Make the temporary keychain the first in the search list so xcodebuild finds the identity
security list-keychain -d user -s "$KEYCHAIN_NAME"

# Allow codesign to access the private key without a UI password prompt
security set-key-partition-list -S apple-tool:,apple:,codesign: \
  -s -k "$MACOS_KEYCHAIN_PASSWORD" "$KEYCHAIN_NAME"

# Verify the identity is visible to the toolchain before proceeding
if ! security find-identity -v -p codesigning "$KEYCHAIN_NAME" | grep -q "Developer ID Application"; then
  echo "Error: 'Developer ID Application' identity not found in keychain after import" >&2
  exit 1
fi

# Remove the decoded cert file; the private key now lives only in the keychain
rm -f "$CERT_FILE"

echo "Certificate imported successfully"
