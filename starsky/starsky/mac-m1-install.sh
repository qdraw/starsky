#!bin/bash

codesign --force --deep -s - starsky
codesign --force --deep -s - libhostfxr.dylib

xattr -d com.apple.quarantine libcoreclr.dylib
xattr -d com.apple.quarantine libclrjit.dylib
xattr -d com.apple.quarantine libSystem.Native.dylib


codesign --force --deep -s - starskyadmincli
codesign --force --deep -s - starskygeocli
codesign --force --deep -s - starskyimportercli
codesign --force --deep -s - starskysynchronizecli
codesign --force --deep -s - starskythumbnailcli
codesign --force --deep -s - starskywebftpcli