#!/bin/bash

# do not rename this file, this is referenced in a azure pipeline

# reset rights if those are wrong
/usr/bin/find . -type d -exec chmod 755 {} \;
/usr/bin/find . -type f -exec chmod 644 {} \;

files=(
  "starskygeocli"
  "starskyimportercli"
  "starskysynchronizecli"
  "starskythumbnailcli"
  "starskythumbnailmetacli"
  "starskywebftpcli"
  "starskywebhtmlcli"
  "starskyadmincli"
  "starskymountwatchercli"
  "starskydependenciesdownloadcli"
  "pm2-deploy-on-env.sh"
  "pm2-install-latest-release.sh"
  "pm2-new-instance.sh"
  "pm2-download-azure-devops.sh"
  "dependencies/exiftool-unix/exiftool"
  "pm2-restore-x-rights.sh"
  "pm2-warmup.sh"
  "service-deploy-systemd.sh"
  "starsky"
)

for file in "${files[@]}"; do
  if [[ -f "$file" ]]; then
    chmod +x "$file"
  fi
done