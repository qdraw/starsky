#!/bin/bash

# do not rename this file, this is referenced in a azure pipeline

# reset rights if those are wrong
/usr/bin/find . -type d -exec chmod 755 {} \;
/usr/bin/find . -type f -exec chmod 644 {} \;


# execute rights for specific files
if [[ -f starskygeocli ]]; then
    chmod +x ./starskygeocli
fi

if [[ -f starskyimportercli ]]; then
    chmod +x ./starskyimportercli
fi

if [[ -f starskysynchronizecli ]]; then
    chmod +x ./starskysynchronizecli
fi

if [[ -f starskythumbnailcli ]]; then
    chmod +x ./starskythumbnailcli
fi

if [[ -f starskythumbnailmetacli ]]; then
    chmod +x ./starskythumbnailmetacli
fi

if [[ -f starskywebftpcli ]]; then
    chmod +x ./starskywebftpcli
fi

if [[ -f starskywebhtmlcli ]]; then
    chmod +x ./starskywebhtmlcli
fi

if [[ -f starskyadmincli ]]; then
    chmod +x ./starskyadmincli
fi

if [[ -f pm2-deploy-on-env.sh ]]; then
    chmod +x ./pm2-deploy-on-env.sh
fi

if [[ -f pm2-install-latest-release.sh ]]; then
    chmod +x ./pm2-install-latest-release.sh
fi

if [[ -f pm2-new-instance.sh ]]; then
    chmod +x ./pm2-new-instance.sh
fi

if [[ -f pm2-download-azure-devops.sh ]]; then
    chmod +x ./pm2-download-azure-devops.sh
fi

if [[ -f dependencies/exiftool-unix/exiftool ]]; then
    chmod +x dependencies/exiftool-unix/exiftool
fi

if [[ -f pm2-restore-x-rights.sh ]]; then
    chmod +x ./pm2-restore-x-rights.sh
fi

if [[ -f pm2-warmup.sh ]]; then
    chmod +x ./pm2-warmup.sh
fi

if [[ -f service-deploy-systemd.sh ]]; then
    chmod +x ./service-deploy-systemd.sh
fi

if [[ -f starsky ]]; then
    chmod +x ./starsky
fi
