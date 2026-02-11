#!/bin/bash
# Recursively run `npm run update:install` in all child folders containing package.json with the script
# Change MAX_DEPTH to control how many levels deep to search
MAX_DEPTH=2

SCRIPT_DIR=$( cd -- "$( dirname -- "${BASH_SOURCE[0]}" )" &> /dev/null && pwd )
cd "$SCRIPT_DIR"

find . -maxdepth $MAX_DEPTH -name package.json | while read -r pkg; do
  dir=$(dirname "$pkg")
  if grep -q '"update:install"' "$pkg"; then
    echo "Running update:install in $dir"
    (cd "$dir" && npm run update:install)
  fi
done
