#!/bin/bash
# Deterministic verification gate — the final vote on any change.
# Green here = done; anything else = not done. No self-assessment overrides this.
#
# Includes a warning RATCHET: the analyzer warning count may go down, never up.
# When it goes down, the baseline tightens automatically (that's the ratchet).
set -euo pipefail
cd "$(dirname "$0")/.."

BASELINE_FILE="scripts/warnings-baseline.txt"
BUILD_LOG=$(mktemp)
trap 'rm -f "$BUILD_LOG"' EXIT

echo "[verify] 1/4 build (Release, full rebuild for a stable warning count)"
if ! dotnet build --configuration Release --no-incremental > "$BUILD_LOG" 2>&1; then
  grep -E "error|Build FAILED" "$BUILD_LOG" | head -20
  echo "[verify] FAIL: build"
  exit 1
fi

COUNT=$(grep -E "^ *[0-9]+ Warning\(s\)" "$BUILD_LOG" | grep -oE "[0-9]+" | head -1)
COUNT=${COUNT:-0}
if [ -f "$BASELINE_FILE" ]; then
  BASELINE=$(cat "$BASELINE_FILE")
  if [ "$COUNT" -gt "$BASELINE" ]; then
    echo "[verify] FAIL: warnings went UP: $COUNT > baseline $BASELINE — fix the new warnings, never raise the baseline"
    grep -E "warning [A-Z]+[0-9]+" "$BUILD_LOG" | sort -u | tail -20
    exit 1
  elif [ "$COUNT" -lt "$BASELINE" ]; then
    echo "$COUNT" > "$BASELINE_FILE"
    echo "[verify] ratchet tightened: $BASELINE -> $COUNT (baseline updated — commit it)"
  else
    echo "[verify] warnings: $COUNT (= baseline)"
  fi
else
  echo "$COUNT" > "$BASELINE_FILE"
  echo "[verify] warning baseline initialized: $COUNT"
fi

echo "[verify] 2/4 format gate: whitespace"
dotnet format whitespace --verify-no-changes

echo "[verify] 3/4 format gate: style"
dotnet format style --verify-no-changes --severity info

echo "[verify] 4/4 tests (same filter as CI)"
dotnet test --configuration Release --no-build --filter "Category!=ExternalE2E"

echo "[verify] ALL GREEN"
