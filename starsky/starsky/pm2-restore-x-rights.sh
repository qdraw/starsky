#!/bin/bash

# reset rights if those are wrong
/usr/bin/find . -type d -exec chmod 755 {} \;
/usr/bin/find . -type f -exec chmod 644 {} \;


# execute rights for specific files
if [ -f starskygeocli ]; then
    chmod +rwx ./starskygeocli
fi

if [ -f starskyimportercli ]; then
    chmod +rwx ./starskyimportercli
fi

if [ -f starskysynccli ]; then
    chmod +rwx ./starskysynccli
fi

if [ -f starskysynchronizecli ]; then
    chmod +rwx ./starskysynchronizecli
fi

if [ -f starskythumbnailcli ]; then
    chmod +rwx ./starskythumbnailcli
fi

if [ -f starskywebftpcli ]; then
    chmod +rwx ./starskywebftpcli
fi

if [ -f starskywebhtmlcli ]; then
    chmod +rwx ./starskywebhtmlcli
fi

if [ -f starskyadmincli ]; then
    chmod +rwx ./starskyadmincli
fi

if [ -f pm2-deploy-on-env.sh ]; then
    chmod +rwx ./pm2-deploy-on-env.sh
fi

if [ -f pm2-warmup.sh ]; then
    chmod +rwx ./pm2-warmup.sh
fi

if [ -f starsky ]; then
    chmod +rwx ./starsky
fi