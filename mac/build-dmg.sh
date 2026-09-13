#!/usr/bin/env bash
# Build the Starsky macOS *.app.
# Mirrors the desktop-release-on-tag-native.yml GitHub Actions workflow.
#
# Usage:
#   ./build-dmg.sh [options]
#
# Options:
#   --arch <universal|arm64|x64>   Target architecture (default: native)
#   --sign                         Enable code signing (required for --mas)
#   --team-id <ID>                 Apple Team ID (or set APPLE_TEAM_ID env var)
#   --skip-backend                 Skip .NET backend build (use pre-built zips in ./starsky/)
#   --output-dir <path>            Output directory (default: ./build)
#   --mas                          Build for Mac App Store (uses MAS config + Apple Distribution)
#   --profile <specifier>          Provisioning profile specifier for MAS manual signing
#                                  (omit to use automatic signing)
#   -h, --help                     Show this help

if [[ -z "${BASH_VERSION:-}" ]]; then
    exec bash "$0" "$@"
fi

set -euo pipefail

REPO_ROOT="$(cd "$(dirname "$0")/.." && pwd)"
MAC_DIR="$REPO_ROOT/mac"
STARSKY_DIR="$REPO_ROOT/starsky"

# Defaults
ARCH="native"
SIGN=false
SKIP_BACKEND=false
OUTPUT_DIR="$REPO_ROOT/mac/dist"
TEAM_ID="${APPLE_TEAM_ID:-}"
SPARKLE_KEY="${SPARKLE_PUBLIC_ED_KEY:-}"
MAS=false
PROFILE=""

usage() {
    sed -n '/^# Usage/,/^$/p' "$0" | sed 's/^# \?//'
    exit 0
}

while [[ $# -gt 0 ]]; do
    case "$1" in
        --arch)         ARCH="$2";    shift 2 ;;
        --sign)         SIGN=true;    shift ;;
        --team-id)      TEAM_ID="$2"; shift 2 ;;
        --skip-backend) SKIP_BACKEND=true; shift ;;
        --output-dir)   OUTPUT_DIR="$2"; shift 2 ;;
        --mas)          MAS=true;     shift ;;
        --profile)      PROFILE="$2"; shift 2 ;;
        -h|--help)      usage ;;
        *) echo "Unknown option: $1" >&2; exit 1 ;;
    esac
done

# MAS always requires signing
if $MAS; then
    SIGN=true
fi

# Resolve native arch
if [[ "$ARCH" == "native" ]]; then
    case "$(uname -m)" in
        arm64)   ARCH="arm64" ;;
        x86_64)  ARCH="x64" ;;
        *) echo "Unsupported architecture: $(uname -m)" >&2; exit 1 ;;
    esac
fi

if $SIGN && [[ -z "$TEAM_ID" ]]; then
    echo "Error: --team-id / APPLE_TEAM_ID required for signing" >&2
    exit 1
fi

if $MAS; then
    XCODE_CONFIG="MAS"
    SIGN_IDENTITY="Apple Distribution"
else
    XCODE_CONFIG="Release"
    SIGN_IDENTITY="Developer ID Application"
fi

echo "==> Configuration"
echo "    Arch:          $ARCH"
echo "    Config:        $XCODE_CONFIG"
echo "    Sign:          $SIGN"
if $MAS && [[ -n "$PROFILE" ]]; then
echo "    Profile:       $PROFILE (manual)"
elif $MAS; then
echo "    Profile:       automatic"
fi
echo "    Skip backend:  $SKIP_BACKEND"
echo "    Output dir:    $OUTPUT_DIR"
echo ""

for tool in xcodebuild xcodegen; do
    if ! command -v "$tool" &>/dev/null; then
        echo "Error: '$tool' not found. Install xcodegen with: brew install xcodegen" >&2
        exit 1
    fi
done

mkdir -p "$OUTPUT_DIR"

# ── Step 1: .NET backend ──────────────────────────────────────────────────────

if ! $SKIP_BACKEND; then
    echo "==> Building .NET backend"
    case "$ARCH" in
        universal) RUNTIMES="osx-x64,osx-arm64" ;;
        arm64)     RUNTIMES="osx-arm64" ;;
        x64)       RUNTIMES="osx-x64" ;;
        *) echo "Unsupported architecture: $ARCH" >&2; exit 1 ;;
    esac

    pushd "$STARSKY_DIR" > /dev/null
    bash build.sh --runtime "$RUNTIMES" --no-unit-test --ready-to-run
    popd > /dev/null
else
    echo "==> Skipping .NET backend build (--skip-backend)"
fi

# Unzip backend zips into the expected directories
echo "==> Unpacking backend binaries"
pushd "$STARSKY_DIR" > /dev/null

if [[ ( "$ARCH" == "universal" || "$ARCH" == "x64" ) && ! -d "osx-x64" ]]; then
    [[ -f "starsky-osx-x64.zip" ]] || { echo "Error: starsky-osx-x64.zip not found in $STARSKY_DIR" >&2; exit 1; }
    unzip -q starsky-osx-x64.zip -d osx-x64
fi

if [[ ( "$ARCH" == "universal" || "$ARCH" == "arm64" ) && ! -d "osx-arm64" ]]; then
    [[ -f "starsky-osx-arm64.zip" ]] || { echo "Error: starsky-osx-arm64.zip not found in $STARSKY_DIR" >&2; exit 1; }
    unzip -q starsky-osx-arm64.zip -d osx-arm64
fi

popd > /dev/null

# ── Step 2: Generate Xcode project ───────────────────────────────────────────

echo "==> Generating Xcode project"
pushd "$MAC_DIR" > /dev/null
xcodegen generate
popd > /dev/null

# ── Step 3: Archive ───────────────────────────────────────────────────────────

ARCHIVE_PATH="$OUTPUT_DIR/starsky.xcarchive"

echo "==> Archiving ($ARCH, $XCODE_CONFIG)"

case "$ARCH" in
    universal) ARCHS_VAL="arm64 x86_64"; ONLY_ACTIVE="NO" ;;
    arm64)     ARCHS_VAL="arm64";        ONLY_ACTIVE="NO" ;;
    x64)       ARCHS_VAL="x86_64";      ONLY_ACTIVE="NO" ;;
    *) echo "Unsupported architecture: $ARCH" >&2; exit 1 ;;
esac

if $SIGN && ! $MAS; then
    xcodebuild archive \
        -project "$MAC_DIR/starsky.xcodeproj" \
        -scheme starsky \
        -configuration "$XCODE_CONFIG" \
        -archivePath "$ARCHIVE_PATH" \
        DEVELOPMENT_TEAM="$TEAM_ID" \
        CODE_SIGN_IDENTITY="$SIGN_IDENTITY" \
        ARCHS="$ARCHS_VAL" \
        ONLY_ACTIVE_ARCH="$ONLY_ACTIVE" \
        SPARKLE_PUBLIC_ED_KEY="$SPARKLE_KEY"
elif $SIGN; then
    # MAS: Apple Distribution signing, no Sparkle key.
    # Use manual signing when --profile is supplied, automatic otherwise.
    if [[ -n "$PROFILE" ]]; then
        xcodebuild archive \
            -project "$MAC_DIR/starsky.xcodeproj" \
            -scheme starsky \
            -configuration "$XCODE_CONFIG" \
            -archivePath "$ARCHIVE_PATH" \
            DEVELOPMENT_TEAM="$TEAM_ID" \
            CODE_SIGN_IDENTITY="$SIGN_IDENTITY" \
            CODE_SIGN_STYLE=Manual \
            PROVISIONING_PROFILE_SPECIFIER="$PROFILE" \
            ARCHS="$ARCHS_VAL" \
            ONLY_ACTIVE_ARCH="$ONLY_ACTIVE"
    else
        xcodebuild archive \
            -project "$MAC_DIR/starsky.xcodeproj" \
            -scheme starsky \
            -configuration "$XCODE_CONFIG" \
            -archivePath "$ARCHIVE_PATH" \
            DEVELOPMENT_TEAM="$TEAM_ID" \
            CODE_SIGN_IDENTITY="$SIGN_IDENTITY" \
            CODE_SIGN_STYLE=Automatic \
            ARCHS="$ARCHS_VAL" \
            ONLY_ACTIVE_ARCH="$ONLY_ACTIVE"
    fi
else
    xcodebuild archive \
        -project "$MAC_DIR/starsky.xcodeproj" \
        -scheme starsky \
        -configuration "$XCODE_CONFIG" \
        -archivePath "$ARCHIVE_PATH" \
        CODE_SIGN_IDENTITY="" \
        CODE_SIGNING_REQUIRED=NO \
        CODE_SIGNING_ALLOWED=NO \
        ARCHS="$ARCHS_VAL" \
        ONLY_ACTIVE_ARCH="$ONLY_ACTIVE" \
        SPARKLE_PUBLIC_ED_KEY="$SPARKLE_KEY"
fi

# ── Step 4: Export ────────────────────────────────────────────────────────────

echo "==> Exporting archive"

if $MAS; then
    EXPORT_PLIST="$OUTPUT_DIR/ExportOptions-mas.plist"
    if [[ -n "$PROFILE" ]]; then
        MAS_SIGNING_STYLE="manual"
        MAS_PROFILE_ENTRY="
	<key>provisioningProfiles</key>
	<dict>
		<key>nl.qdraw.starsky</key>
		<string>$PROFILE</string>
	</dict>"
    else
        MAS_SIGNING_STYLE="automatic"
        MAS_PROFILE_ENTRY=""
    fi
    cat > "$EXPORT_PLIST" <<EOF
<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN" "http://www.apple.com/DTDs/PropertyList-1.0.dtd">
<plist version="1.0">
<dict>
	<key>method</key>
	<string>app-store-connect</string>
	<key>signingStyle</key>
	<string>$MAS_SIGNING_STYLE</string>
	<key>signingCertificate</key>
	<string>Apple Distribution</string>
	<key>teamID</key>
	<string>$TEAM_ID</string>$MAS_PROFILE_ENTRY
	<key>stripSwiftSymbols</key>
	<true/>
</dict>
</plist>
EOF
elif $SIGN; then
    EXPORT_PLIST="$OUTPUT_DIR/ExportOptions-signed.plist"
    cat > "$EXPORT_PLIST" <<EOF
<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN" "http://www.apple.com/DTDs/PropertyList-1.0.dtd">
<plist version="1.0">
<dict>
	<key>method</key>
	<string>developer-id</string>
	<key>signingStyle</key>
	<string>manual</string>
	<key>signingCertificate</key>
	<string>Developer ID Application</string>
	<key>teamID</key>
	<string>$TEAM_ID</string>
	<key>stripSwiftSymbols</key>
	<true/>
</dict>
</plist>
EOF
else
    EXPORT_PLIST="$OUTPUT_DIR/ExportOptions-unsigned.plist"
    cat > "$EXPORT_PLIST" <<'EOF'
<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN" "http://www.apple.com/DTDs/PropertyList-1.0.dtd">
<plist version="1.0">
<dict>
    <key>method</key>
    <string>mac-application</string>
    <key>signingStyle</key>
    <string>manual</string>
    <key>stripSwiftSymbols</key>
    <true/>
</dict>
</plist>
EOF
fi

xcodebuild -exportArchive \
    -archivePath "$ARCHIVE_PATH" \
    -exportPath "$OUTPUT_DIR/" \
    -exportOptionsPlist "$EXPORT_PLIST"

# ── Step 5: DMG (Developer ID only) / upload hint (MAS) ──────────────────────

if $MAS; then
    PKG_PATH="$OUTPUT_DIR/starsky.pkg"
    echo ""
    echo "==> Done: $PKG_PATH"
    echo ""
    echo "    Upload to App Store Connect with:"
    echo "      xcrun altool --upload-package '$PKG_PATH' \\"
    echo "                   --type macos \\"
    echo "                   --apple-id <APP_APPLE_ID> \\"
    echo "                   --bundle-version <VERSION> \\"
    echo "                   --bundle-short-version-string <VERSION> \\"
    echo "                   --bundle-id nl.qdraw.starsky \\"
    echo "                   --apiKey <API_KEY> --apiIssuer <ISSUER_ID>"
    echo ""
    echo "    Or drag '$PKG_PATH' into the Transporter app."
else
    case "$ARCH" in
        universal) DMG_NAME="starsky-mac-universal-desktop.dmg" ;;
        arm64)     DMG_NAME="starsky-mac-arm64-desktop.dmg" ;;
        x64)       DMG_NAME="starsky-mac-x64-desktop.dmg" ;;
    esac

    if command -v create-dmg &>/dev/null; then
        echo "==> Creating DMG ($DMG_NAME)"
        create-dmg \
            --overwrite \
            --volname "Starsky" \
            --window-pos 200 120 \
            --window-size 600 400 \
            --icon-size 100 \
            --icon "starsky.app" 175 190 \
            --app-drop-link 425 190 \
            "$OUTPUT_DIR/$DMG_NAME" \
            "$OUTPUT_DIR/starsky.app"
        if $SIGN; then
            echo "==> Signing DMG"
            codesign --force --sign "Developer ID Application" "$OUTPUT_DIR/$DMG_NAME"
        fi
    else
        echo "==> Skipping DMG creation: 'create-dmg' not found (install with: brew install create-dmg)"
    fi

    echo ""
    if command -v create-dmg &>/dev/null; then
        echo "==> Done: $OUTPUT_DIR/$DMG_NAME"
    else
        echo "==> Done: $OUTPUT_DIR/starsky.app"
    fi
fi
