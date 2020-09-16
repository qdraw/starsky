#!/usr/bin/env sh

if [ -z "$E_ISDEMO" ]; then
    echo "NO PARAM PASSED"
else
    echo $E_ISDEMO
fi
