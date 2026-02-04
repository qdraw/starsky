#!/bin/bash
SCRIPT_DIR="$( cd "$( dirname "$0" )" && pwd )"

cd $SCRIPT_DIR

chmod +x starsky/starsky/cleanup-build-tools.sh
./starsky/cleanup-build-tools.sh
