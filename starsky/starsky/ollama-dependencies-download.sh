#!/bin/bash

# Download and prepare Ollama runtime + model dependencies.
# Uses download-mirror.sh to retry mirror when source fails.

set -u

CURRENT_DIR=$(dirname "$0")
DEPENDENCIES_DIR="$CURRENT_DIR/dependencies"
MODEL_NAME="gemma3:4b"
OLLAMA_VERSION="v0.6.8"

ARGUMENTS=("$@")
for ((i = 1; i <= $#; i++)); do
  CURRENT=$((i - 1))
  PREV=$((i - 2))

  if [[ ${ARGUMENTS[CURRENT]} == "--help" ]]; then
    echo "--model gemma3:4b"
    echo "--ollama-version v0.6.8"
    echo "--dependencies-dir ./dependencies"
    exit 0
  fi

  if [[ $i -gt 1 ]]; then
    if [[ ${ARGUMENTS[PREV]} == "--model" ]]; then
      MODEL_NAME="${ARGUMENTS[CURRENT]}"
    fi
    if [[ ${ARGUMENTS[PREV]} == "--ollama-version" ]]; then
      OLLAMA_VERSION="${ARGUMENTS[CURRENT]}"
    fi
    if [[ ${ARGUMENTS[PREV]} == "--dependencies-dir" ]]; then
      DEPENDENCIES_DIR="${ARGUMENTS[CURRENT]}"
    fi
  fi
done

if [[ ! -f "$CURRENT_DIR/download-mirror.sh" ]]; then
  echo "FAIL: download-mirror.sh is missing"
  exit 1
fi

chmod +x "$CURRENT_DIR/download-mirror.sh"

OS_NAME=$(uname)
ARCH_NAME=$(uname -m)
RUNTIME_KEY=""

if [[ "$OS_NAME" == "Linux" && "$ARCH_NAME" == "x86_64" ]]; then
  RUNTIME_KEY="linux-amd64"
elif [[ "$OS_NAME" == "Linux" && "$ARCH_NAME" == "aarch64" ]]; then
  RUNTIME_KEY="linux-arm64"
elif [[ "$OS_NAME" == "Darwin" && "$ARCH_NAME" == "x86_64" ]]; then
  RUNTIME_KEY="darwin-amd64"
elif [[ "$OS_NAME" == "Darwin" && "$ARCH_NAME" == "arm64" ]]; then
  RUNTIME_KEY="darwin-arm64"
fi

if [[ -z "$RUNTIME_KEY" ]]; then
  echo "FAIL: unsupported runtime $OS_NAME/$ARCH_NAME"
  exit 1
fi

ARCHIVE_FILE="ollama-$RUNTIME_KEY.tgz"
SOURCE_URL="https://github.com/ollama/ollama/releases/download/$OLLAMA_VERSION/$ARCHIVE_FILE"
MIRROR_URL="https://qdraw.nl/special/mirror/ollama/$OLLAMA_VERSION/$ARCHIVE_FILE"

mkdir -p "$DEPENDENCIES_DIR"
RUNTIME_DIR="$DEPENDENCIES_DIR/ollama-$RUNTIME_KEY"
mkdir -p "$RUNTIME_DIR"

ARCHIVE_PATH="$RUNTIME_DIR/$ARCHIVE_FILE"

echo "Download Ollama runtime: $RUNTIME_KEY"
bash "$CURRENT_DIR/download-mirror.sh" \
  --source "$SOURCE_URL" \
  --mirror "$MIRROR_URL" \
  --output "$ARCHIVE_PATH"

if [[ $? -ne 0 ]]; then
  echo "FAIL: unable to download Ollama runtime"
  exit 1
fi

tar -xzf "$ARCHIVE_PATH" -C "$RUNTIME_DIR"
if [[ $? -ne 0 ]]; then
  echo "FAIL: unable to extract $ARCHIVE_PATH"
  exit 1
fi

OLLAMA_BIN=$(find "$RUNTIME_DIR" -type f -name "ollama" | head -n 1)
if [[ -z "$OLLAMA_BIN" ]]; then
  echo "FAIL: ollama binary not found in $RUNTIME_DIR"
  exit 1
fi

chmod +x "$OLLAMA_BIN"
export OLLAMA_MODELS="$DEPENDENCIES_DIR/ollama-models"
mkdir -p "$OLLAMA_MODELS"

STARTED_BY_SCRIPT=false
if ! "$OLLAMA_BIN" list >/dev/null 2>&1; then
  echo "Start local ollama service for model bootstrap"
  "$OLLAMA_BIN" serve >/dev/null 2>&1 &
  OLLAMA_PID=$!
  STARTED_BY_SCRIPT=true
  sleep 4
fi

echo "Pull model: $MODEL_NAME"
"$OLLAMA_BIN" pull "$MODEL_NAME"
MODEL_RESULT=$?

if [[ "$STARTED_BY_SCRIPT" == true ]]; then
  kill "$OLLAMA_PID" >/dev/null 2>&1 || true
fi

if [[ $MODEL_RESULT -ne 0 ]]; then
  echo "FAIL: model pull failed: $MODEL_NAME"
  exit 1
fi

echo "SUCCESS: Ollama runtime + model are ready"
exit 0

