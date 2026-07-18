#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
cd "$ROOT_DIR"

BANNERLORD_DIR="${BANNERLORD_DIR:-}"
if [ -z "$BANNERLORD_DIR" ]; then
  echo "Error: BANNERLORD_DIR environment variable is not set." >&2
  echo "Usage: BANNERLORD_DIR=\"/path/to/game\" ./build-linux.sh" >&2
  exit 1
fi

HARMONY_VERSION="2.2.2"
HARMONY_DIR="$ROOT_DIR/.build-tools/harmony"
HARMONY_DLL="$HARMONY_DIR/0Harmony.dll"

if [ ! -f "$HARMONY_DLL" ]; then
  echo "Downloading Harmony v$HARMONY_VERSION for build..."
  mkdir -p "$HARMONY_DIR"
  curl -sSL -o "$HARMONY_DIR/harmony.zip" "https://www.nuget.org/api/v2/package/Lib.Harmony/$HARMONY_VERSION"
  unzip -q -o "$HARMONY_DIR/harmony.zip" "lib/net472/0Harmony.dll" -d "$HARMONY_DIR"
  mv "$HARMONY_DIR/lib/net472/0Harmony.dll" "$HARMONY_DLL"
  rm -rf "$HARMONY_DIR/lib" "$HARMONY_DIR/harmony.zip"
fi

BUILD_TOOL="msbuild"
if ! command -v msbuild &> /dev/null; then
    BUILD_TOOL="xbuild"
fi

$BUILD_TOOL LordCaptor.csproj /t:Rebuild /p:Configuration=Release "/p:BannerlordInstallDir=$BANNERLORD_DIR"

rm -rf dist
mkdir -p dist/LordCaptor/bin/Win64_Shipping_Client dist/LordCaptor/ModuleData
cp "bin/Release/LordCaptor.dll" "dist/LordCaptor/bin/Win64_Shipping_Client/"

cp SubModule.xml dist/LordCaptor/
cp -R ModuleData/. dist/LordCaptor/ModuleData/

echo "Built self-contained dist/LordCaptor"
