#!/usr/bin/env bash

set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
PROJECT="$ROOT_DIR/src/Chatly.Desktop/Chatly.Desktop.csproj"
CONFIGURATION="${CONFIGURATION:-Debug}"
FRAMEWORK="net10.0"
APP_DLL="$ROOT_DIR/src/Chatly.Desktop/bin/$CONFIGURATION/$FRAMEWORK/Chatly.Desktop.dll"

PROFILE_ONE="${CHATLY_PROFILE_ONE:-primary}"
PROFILE_TWO="${CHATLY_PROFILE_TWO:-secondary}"
CALLBACK_ONE="${CHATLY_CALLBACK_ONE:-http://127.0.0.1:7890/callback/}"
CALLBACK_TWO="${CHATLY_CALLBACK_TWO:-http://127.0.0.1:7891/callback/}"

dotnet build "$PROJECT" --configuration "$CONFIGURATION"

DesktopProfileOption__Name="$PROFILE_ONE" \
Auth0Option__RedirectUri="$CALLBACK_ONE" \
Auth0Option__Prompt="login" \
dotnet "$APP_DLL" &
PID_ONE=$!

DesktopProfileOption__Name="$PROFILE_TWO" \
Auth0Option__RedirectUri="$CALLBACK_TWO" \
Auth0Option__Prompt="login" \
dotnet "$APP_DLL" &
PID_TWO=$!

cleanup() {
    kill "$PID_ONE" "$PID_TWO" 2>/dev/null || true
}

trap cleanup EXIT INT TERM

printf 'Chatly client %s PID: %s\n' "$PROFILE_ONE" "$PID_ONE"
printf 'Chatly client %s PID: %s\n' "$PROFILE_TWO" "$PID_TWO"
printf 'Attach your debugger to either PID. Press Ctrl+C to stop both clients.\n'

wait "$PID_ONE" "$PID_TWO"
