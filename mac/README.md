# Starsky Desktop — macOS

Native macOS desktop app wrapping the Starsky photo-management backend inside a WKWebView.  
See [SPEC.md](SPEC.md) for the full architecture specification.

---

## Getting Started (local development)

### Prerequisites

- macOS 13 or later
- Xcode 15 or later (`xcode-select -p` should point to `/Applications/Xcode.app/…`)
- [xcodegen](https://github.com/yonaskolb/XcodeGen) — generates the Xcode project from `project.yml`

```bash
brew install xcodegen
```

### First-time setup

```bash
# From the repo root
cd mac
xcodegen generate        # creates starsky.xcodeproj
open starsky.xcodeproj   # or double-click in Finder
```

Run `xcodegen generate` again any time you add or remove Swift source files, or after pulling changes that modify `project.yml`.

### Build & run from the command line

```bash
cd mac
xcodebuild build \
  -project starsky.xcodeproj \
  -scheme starsky \
  -configuration Debug \
  -destination 'platform=macOS' \
  CODE_SIGN_IDENTITY="" CODE_SIGNING_REQUIRED=NO CODE_SIGNING_ALLOWED=NO
```

### Run the tests

```bash
cd mac
xcodebuild test \
  -project starsky.xcodeproj \
  -scheme starskyTests \
  -destination 'platform=macOS' \
  CODE_SIGN_IDENTITY="" CODE_SIGNING_REQUIRED=NO CODE_SIGNING_ALLOWED=NO
```

Expected output: `** TEST SUCCEEDED **` (57 tests, 0 failures).

### Bundled backend

The app looks for the Starsky ASP.NET Core binary at:

```
starsky.app/Contents/MacOS/runtime-starsky-osx-arm64/starsky   # Apple Silicon
starsky.app/Contents/MacOS/runtime-starsky-osx-x64/starsky     # Intel
```

These are copied at build time from `starskydesktop/runtime-starsky-mac-arm64/` and `starskydesktop/runtime-starsky-mac-x64/` (if present). A build warning is emitted when they are missing; Local mode will not work without them.

---

## Before Releasing

### 1. Generate a Sparkle EdDSA keypair

Sparkle 2 requires an EdDSA key to sign update packages. Do this **once** and store the private key securely (it never goes in the repo).

```bash
# Download the Sparkle release and extract generate_keys
curl -L https://github.com/sparkle-project/Sparkle/releases/latest/download/Sparkle-2.x.x.tar.xz | tar -xJ
./bin/generate_keys
```

The tool prints:

```
Private key saved to ~/Library/Preferences/Sparkle/Sparkle_private_key
Public key (add to Info.plist): <base64-string>
```

Open [`starsky/Info.plist`](starsky/Info.plist) and set:

```xml
<key>SUPublicEDKey</key>
<string>PASTE_PUBLIC_KEY_HERE</string>
```

### 2. Set your Apple Developer Team ID

In [`project.yml`](project.yml), replace the empty `DEVELOPMENT_TEAM` value:

```yaml
DEVELOPMENT_TEAM: "XXXXXXXXXX"   # your 10-character Team ID
```

Also fill in the `teamID` field in [`ExportOptions.plist`](ExportOptions.plist):

```xml
<key>teamID</key>
<string>XXXXXXXXXX</string>
```

### 3. Install the Developer ID certificate

Ensure "Developer ID Application: \<your name\> (\<team-id\>)" is installed in Keychain Access.  
Download it from [developer.apple.com/account → Certificates](https://developer.apple.com/account/resources/certificates/list) if needed.

### 4. Set up GitHub secrets for CI

Go to **Settings → Secrets and variables → Actions** in your GitHub repo and add:

| Secret name | Value |
|---|---|
| `APPLE_ID` | Your Apple ID email (e.g. `you@example.com`) |
| `APPLE_TEAM_ID` | Your 10-character Team ID |
| `NOTARYTOOL_APP_PASSWORD` | An app-specific password from [appleid.apple.com](https://appleid.apple.com) → App-Specific Passwords |

### 5. Create the Sparkle appcast

After your first signed build, generate an appcast XML file and host it at the `SUFeedURL` configured in [`starsky/Info.plist`](starsky/Info.plist):

```
https://qdraw.nl/special/starsky/appcast-macos.xml
```

Use `sparkle-generate-appcast` (included in the Sparkle distribution):

```bash
./bin/generate_appcast /path/to/release/folder/
```

Upload the resulting `appcast.xml` to your web server at the URL above.

### 6. Release build (manual)

```bash
cd mac
xcodegen generate

# Archive
xcodebuild archive \
  -project starsky.xcodeproj \
  -scheme starsky \
  -configuration Release \
  -archivePath ../build/starsky.xcarchive \
  ARCHS="arm64 x86_64"

# Export signed app
xcodebuild -exportArchive \
  -archivePath ../build/starsky.xcarchive \
  -exportPath ../build/ \
  -exportOptionsPlist ExportOptions.plist

# Create DMG
brew install create-dmg
create-dmg \
  --volname "Starsky" \
  --window-size 600 400 \
  --icon-size 100 \
  --icon "starsky.app" 175 190 \
  --app-drop-link 425 190 \
  ../build/starsky.dmg \
  "../build/starsky.app"

# Notarize
xcrun notarytool submit ../build/starsky.dmg \
  --apple-id "$APPLE_ID" \
  --team-id "$APPLE_TEAM_ID" \
  --password "$NOTARYTOOL_APP_PASSWORD" \
  --wait

# Staple
xcrun stapler staple ../build/starsky.dmg

# Verify
spctl -a -vvv ../build/starsky.app
```

Release builds on tagged commits are automated via [`.github/workflows/desktop-macos-pr-build.yml`](../.github/workflows/desktop-macos-pr-build.yml).

---

## Project structure

```
mac/
├── SPEC.md                    full architecture specification
├── README.md                  this file
├── project.yml                xcodegen spec (source of truth for the Xcode project)
├── ExportOptions.plist        Developer ID export settings for notarization
├── starsky/
│   ├── App/
│   │   ├── AppDelegate.swift          startup / shutdown / menu bar
│   │   └── ApplicationInfo.swift      version string from bundle
│   ├── Models/                        Codable data types
│   ├── Services/                      all business logic (no UI)
│   ├── Windows/                       NSWindowController subclasses + WKWebView
│   ├── WindowManager.swift            manages open MainWindowController instances
│   └── Resources/Assets.xcassets     AppIcon (populate before release)
└── starskyTests/
    ├── Helpers/FakeURLProtocol.swift  offline HTTP testing
    ├── FakeCreateAn/                  fake backend binary helper
    ├── Models/                        model tests
    └── Services/                      service tests (57 tests total)
```
