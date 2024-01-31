#!bin/bash
cd "$(dirname "$0")"

codesign --force --deep -s - libhostfxr.dylib
xattr -d com.apple.quarantine libcoreclr.dylib
xattr -d com.apple.quarantine libclrjit.dylib
xattr -d com.apple.quarantine libSystem.Native.dylib

# Array of executable files
executables=(
  "starskyadmincli"
  "starskygeocli"
  "starskyimportercli"
  "starskysynchronizecli"
  "starskythumbnailcli"
  "starskywebftpcli"
)

# Remove quarantine extended attributes
for exec in "${executables[@]}"
do
  xattr -rd com.apple.quarantine "$exec"
done

# Codesign the executables
for exec in "${executables[@]}"
do
  codesign --force --deep -s - "$exec"
done
