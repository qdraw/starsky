# macOS Signing & Notarization: GitHub Secrets Setup

## 1. Create the Developer ID Application certificate

1. Open Xcode → Settings (⌘,) → Accounts
2. Sign in with the Apple ID enrolled in your Developer Program
3. Select your team → Manage Certificates
4. Click **+** → choose **Developer ID Application**
5. Xcode creates the certificate and private key and installs both into your login keychain automatically

## 2. Export the certificate as a .p12

1. Open **Keychain Access** → login keychain → **My Certificates**
2. Find `Developer ID Application: Your Name (TEAMID)`
3. Right-click → **Export**
4. Save as `.p12`, set an export password — this becomes `MACOS_CERTIFICATE_PWD`

## 3. Base64-encode the .p12

```bash
base64 -i ~/Desktop/YourCert.p12 -o ~/Desktop/cert_base64.txt
```

Open `cert_base64.txt` and copy its entire contents (one long string).

## 4. Add GitHub repository secrets

Go to your repo → **Settings** → **Secrets and variables** → **Actions** → **New repository secret** and create:

| Secret | Value |
|---|---|
| `STARSKY_MACOS_CERTIFICATE` | The base64 string from step 3 |
| `STARSKY_MACOS_CERTIFICATE_PWD` | The export password set in step 2 |
| `STARSKY_MACOS_KEYCHAIN_PASSWORD` | Any random string you make up (used to lock/unlock the CI keychain) |

## 5. Create an app-specific password for notarytool

1. Go to [appleid.apple.com](https://appleid.apple.com/) → Sign in → **Sign-In and Security** → **App-Specific Passwords**
2. Click **+** → give it a label (e.g. `starsky-notarytool`)
3. Copy the generated password — format is `xxxx-xxxx-xxxx-xxxx` (shown only once)
4. Save it as the `STARSKY_NOTARYTOOL_APP_PASSWORD` secret

Also ensure these secrets are set:

| Secret | Value |
|---|---|
| `STARSKY_APPLE_ID` | Your Apple ID email |
| `STARSKY_NOTARYTOOL_APP_PASSWORD` | The app-specific password from above |

## 6. Add the keychain import step to the workflow

In `.github/workflows/desktop-release-on-tag-net-electron.yml`, the certificate import step (from `mac/scripts/import-cert.sh`) must run **before** each `Archive` step in `build_mac_native`, `build_mac_arm64`, and `build_mac_x64`. Add the keychain cleanup step after each job's `Notarize and staple` step.

## 7. Delete the .p12 from your machine

Once the base64 secret is saved in GitHub, delete `YourCert.p12` and `cert_base64.txt` from your Desktop and empty the Trash. There is no reason for the raw certificate file to remain on disk once it is safely stored in GitHub Secrets.

## 8. Trigger a test run

Use `workflow_dispatch` (already enabled in this workflow) to verify signing before doing a real release:

1. Go to **Actions** tab → "Desktop Release on tag (.NET, Swift & WPF)"
2. Click **Run workflow** → pick `master`

This runs the full pipeline including the Mac jobs without pushing a `v*` tag.

## 9. Generate Sparkle EdDSA keys for auto-update

Sparkle 2 requires an EdDSA keypair to sign and verify update packages. Do this **once** and keep the private key safe — it never goes in the repo.

```bash
curl -fsSL https://github.com/sparkle-project/Sparkle/releases/download/2.9.6/Sparkle-2.9.6.tar.xz | tar -xJ
./bin/generate_keys
```

The tool prints a public key and saves the private key:

```
Private key saved to ~/Library/Preferences/Sparkle/Sparkle_private_key
Public key (add to Info.plist): <base64-string>
```

Base64-encode the **private key file** for CI:

```bash
base64 -i ~/Library/Preferences/Sparkle/Sparkle_private_key | pbcopy
```

Add both to GitHub secrets:

| Secret | Value |
|---|---|
| `STARSKY_MACOS_SPARKLE_PUBLIC_ED_KEY` | The base64 public key printed to the terminal by `generate_keys` |
| `STARSKY_MACOS_SPARKLE_PRIVATE_ED_KEY` | The base64 output of the `base64 -i ...` command above |

The public key is injected into `Info.plist` at build time so the app can verify updates. The private key is used by the `publish_appcast` CI job to sign the DMG and generate the appcast XML.

Do **not** put either key in the repository. The private key in particular must be kept secret — whoever holds it can sign malicious updates.

## Troubleshooting

- **"No signing certificate found"** during `xcodebuild archive` → the keychain import step did not run or the certificate was not imported correctly. Check `MACOS_CERTIFICATE` and `MACOS_CERTIFICATE_PWD`.
- **Notarization fails** → check that `STARSKY_APPLE_ID` and `STARSKY_NOTARYTOOL_APP_PASSWORD` are correctly set. Signing and notarizing are separate steps that can each fail independently.
- **`spctl` rejects the DMG** → the DMG was not signed after creation. Confirm the "Sign DMG" step in the workflow ran successfully.
