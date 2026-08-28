#!/usr/bin/env bash
# Build the Starsky macOS *.app.
# Mirrors the desktop-release-on-tag-net-electron.yml GitHub Actions workflow.
#
# Usage:
#   ./build-dmg.sh [options]
#
# Options:
#   --arch <universal|arm64|x64>   Target architecture (default: native)
#   --sign                         Enable code signing (requires Developer ID cert)
#   --team-id <ID>                 Apple Team ID (or set APPLE_TEAM_ID env var)
#   --skip-backend                 Skip .NET backend build (use pre-built zips in ./starsky/)
#   --output-dir <path>            Output directory (default: ./build)
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
        -h|--help)      usage ;;
        *) echo "Unknown option: $1" >&2; exit 1 ;;
    esac
done

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

echo "==> Configuration"
echo "    Arch:          $ARCH"
echo "    Sign:          $SIGN"
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

echo "==> Archiving ($ARCH)"

case "$ARCH" in
    universal) ARCHS_VAL="arm64 x86_64"; ONLY_ACTIVE="NO" ;;
    arm64)     ARCHS_VAL="arm64";        ONLY_ACTIVE="NO" ;;
    x64)       ARCHS_VAL="x86_64";      ONLY_ACTIVE="NO" ;;
    *) echo "Unsupported architecture: $ARCH" >&2; exit 1 ;;
esac

if $SIGN; then
    xcodebuild archive \
        -project "$MAC_DIR/starsky.xcodeproj" \
        -scheme starsky \
        -configuration Release \
        -archivePath "$ARCHIVE_PATH" \
        DEVELOPMENT_TEAM="$TEAM_ID" \
        CODE_SIGN_IDENTITY="Developer ID Application" \
        ARCHS="$ARCHS_VAL" \
        ONLY_ACTIVE_ARCH="$ONLY_ACTIVE"
else
    xcodebuild archive \
        -project "$MAC_DIR/starsky.xcodeproj" \
        -scheme starsky \
        -configuration Release \
        -archivePath "$ARCHIVE_PATH" \
        CODE_SIGN_IDENTITY="" \
        CODE_SIGNING_REQUIRED=NO \
        CODE_SIGNING_ALLOWED=NO \
        ARCHS="$ARCHS_VAL" \
        ONLY_ACTIVE_ARCH="$ONLY_ACTIVE"
fi

# ── Step 4: Export ────────────────────────────────────────────────────────────

echo "==> Exporting archive"

EXPORT_PLIST="$MAC_DIR/ExportOptions.plist"
if ! $SIGN; then
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

# ── Done ──────────────────────────────────────────────────────────────────────

echo ""
echo "==> Done: $OUTPUT_DIR/starsky.app"
