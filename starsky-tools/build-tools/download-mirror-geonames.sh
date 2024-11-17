#!/bin/bash
GEONAMES_DUMP="https://download.geonames.org/export/dump/"
BINARY_FOLDERNAME="mirror/exiftool"
ADMIN1_CODES="admin1CodesASCII.txt"
CITIES1000="cities1000.zip"

SCRIPT_DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" &> /dev/null && pwd )"

LAST_CHAR_GEONAMES_DUMP=${GEONAMES_DUMP:length-1:1}
[[ $LAST_CHAR_GEONAMES_DUMP != "/" ]] && GEONAMES_DUMP="$GEONAMES_DUMP/"; :

rm -rf $SCRIPT_DIR$BINARY_FOLDERNAME
mkdir -p $SCRIPT_DIR$BINARY_FOLDERNAME
cd $SCRIPT_DIR$BINARY_FOLDERNAME


curl -L -O "$GEONAMES_DUMP$ADMIN1_CODES"
curl -L -O "$GEONAMES_DUMP$CITIES1000"