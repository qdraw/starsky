#!/bin/bash

SCRIPT_DIR=$( cd -- "$( dirname -- "${BASH_SOURCE[0]}" )" &> /dev/null && pwd )
cd $SCRIPT_DIR

pushd build-tools
    npm run update:install
popd
pushd dropbox-import
    npm run update:install
popd
pushd end2end
    npm run update:install
popd
# pushd import
#     npm run update
# popd
pushd localtunnel
    npm run update:install
popd
pushd mail
    npm run update:install
popd
pushd mock
    npm run update:install
popd
# pushd release-tools
#     npm run update:install
# popd
pushd slack-notification
    npm run update:install
popd
# pushd socket
#     npm run update:install
# popd
pushd sync
    npm run update:install
popd
pushd thumbnail
    npm run update:install
popd