#!/usr/bin/env bash
set -euo pipefail

# Default to dry-run. Set DRY_RUN=0 to uninstall.
DRY_RUN="${DRY_RUN:-1}"

TMP_DIR="$(mktemp -d /tmp/dotnet-cleanup.XXXXXX)"
trap 'rm -rf "$TMP_DIR"' EXIT

# Check prerequisites.
for cmd in curl python3 tar awk sort find; do
    if ! command -v "$cmd" >/dev/null 2>&1; then
        echo "Error: required command '$cmd' was not found." >&2
        exit 1
    fi
done

if ! command -v dotnet >/dev/null 2>&1; then
    echo "Error: dotnet was not found in PATH." >&2
    exit 1
fi

# Locate or download dotnet-core-uninstall.
if command -v dotnet-core-uninstall >/dev/null 2>&1; then
    UNINSTALL="$(command -v dotnet-core-uninstall)"
else
    case "$(uname -m)" in
        arm64) ARCH="arm64" ;;
        x86_64) ARCH="x64" ;;
        *)
            echo "Unsupported architecture: $(uname -m)" >&2
            exit 1
            ;;
    esac

    echo "Downloading .NET uninstall tool..."

    RELEASE_JSON="$TMP_DIR/release.json"
    # --proto/--proto-redir keep -L from following a redirect down to plain http.
    curl -fsSL --retry 3 --proto '=https' --proto-redir '=https' \
        https://api.github.com/repos/dotnet/cli-lab/releases/latest \
        -o "$RELEASE_JSON"

    ASSET_URL="$(
        python3 - "$RELEASE_JSON" "$ARCH" <<'PY'
import json
import sys

release_file, arch = sys.argv[1], sys.argv[2]

with open(release_file, encoding="utf-8") as f:
    data = json.load(f)

suffixes = (
    "-osx-" + arch + ".tar.gz",
    "osx-" + arch + ".tar.gz",
)

assets = data.get("assets", [])
matches = [
    asset["browser_download_url"]
    for asset in assets
    if any(asset.get("name", "").endswith(s) for s in suffixes)
]

if not matches:
    sys.exit(
        "Error: no matching macOS archive was found in the latest "
        "dotnet/cli-lab release. Check its release assets."
    )

print(matches[0])
PY
    )"

    ARCHIVE="$TMP_DIR/uninstaller.tar.gz"
    # ASSET_URL comes from the API response, so constrain it the same way.
    curl -fL --retry 3 --proto '=https' --proto-redir '=https' \
        "$ASSET_URL" -o "$ARCHIVE"

    mkdir "$TMP_DIR/tool"
    tar -xzf "$ARCHIVE" -C "$TMP_DIR/tool"

    UNINSTALL="$(
        find "$TMP_DIR/tool" -type f \
            -name dotnet-core-uninstall -print -quit
    )"

    if [[ -z "$UNINSTALL" ]]; then
        echo "Error: uninstall executable not found in archive." >&2
        exit 1
    fi

    chmod +x "$UNINSTALL"
fi

# Save installed SDK versions to files to avoid unsafe loop input.
SDK_FILE="$TMP_DIR/sdks.txt"
LATEST_FILE="$TMP_DIR/latest.txt"

dotnet --list-sdks | awk '{print $1}' | sort -V > "$SDK_FILE"

if [[ ! -s "$SDK_FILE" ]]; then
    echo "No .NET SDKs found."
    exit 0
fi

# Keep the highest installed version for every major version.
# sort -V sorts version numbers; awk groups them by major.
awk -F. '{ print $1, $0 }' "$SDK_FILE" |
    sort -k1,1n -k2,2V |
    awk '
        NR == 1 {
            previous_major = $1
            previous_version = $2
            next
        }
        $1 != previous_major {
            print previous_version
            previous_major = $1
        }
        {
            previous_version = $2
        }
        END {
            if (NR > 0) {
                print previous_version
            }
        }
    ' > "$LATEST_FILE"

echo
echo "Installed SDKs:"
cat "$SDK_FILE"

echo
echo "Keeping:"
cat "$LATEST_FILE"

echo
echo "Planned actions:"

# Read the SDK list from a file, not an expanded here-document.

# Load SDK versions into a Bash array (compatible with macOS Bash 3.2).
SDK_LIST=()
while IFS= read -r sdk; do
    SDK_LIST[${#SDK_LIST[@]}]="$sdk"
done < "$SDK_FILE"

FAILED_REMOVALS=()

for sdk in "${SDK_LIST[@]}"; do
    major="${sdk%%.*}"
    latest="$(
        awk -F. -v m="$major" '$1 == m { print; exit }' "$LATEST_FILE"
    )"

    if [[ "$sdk" == "$latest" ]]; then
        echo "KEEP   $sdk"
    else
        echo "REMOVE $sdk"

        if [[ "$DRY_RUN" == "0" ]] &&
            ! "$UNINSTALL" remove --sdk "$sdk" --yes </dev/null; then
            echo "Error: failed to remove SDK $sdk." >&2
            FAILED_REMOVALS[${#FAILED_REMOVALS[@]}]="$sdk"
        fi
    fi
done

if [[ ${#FAILED_REMOVALS[@]} -gt 0 ]]; then
    echo >&2
    echo "Cleanup completed with failed removals:" >&2
    printf '  %s\n' "${FAILED_REMOVALS[@]}" >&2
    exit 1
fi

if [[ "$DRY_RUN" == "1" ]]; then
    echo
    echo "Dry run only. Nothing was removed."
    echo "Review the list, then run:"
    echo "  DRY_RUN=0 $0"
else
    echo
    echo "Cleanup finished. Remaining SDKs:"
    dotnet --list-sdks
fi