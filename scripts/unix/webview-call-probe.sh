#!/usr/bin/env bash

set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)"
PROJECT="$ROOT_DIR/tests/Chatly.WebViewCallProbe.IntegrationTests/Chatly.WebViewCallProbe.IntegrationTests.csproj"
CONFIGURATION="${CONFIGURATION:-Debug}"

dotnet test --project "$PROJECT" --configuration "$CONFIGURATION" --explicit only "$@"
