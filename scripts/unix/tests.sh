#!/usr/bin/env bash

set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)"
CONFIGURATION="${CONFIGURATION:-Debug}"

SOURCE_PROJECTS=("$ROOT_DIR"/src/*/*.csproj)
TEST_PROJECTS=("$ROOT_DIR"/tests/*/*.csproj)

for project in "${SOURCE_PROJECTS[@]}" "${TEST_PROJECTS[@]}"; do
    dotnet build "$project" --configuration "$CONFIGURATION"
done

for project in "${TEST_PROJECTS[@]}"; do
    project_dir="$(dirname "$project")"
    if [[ -z "$(find "$project_dir" -type f -name '*.cs' \
        -not -path "$project_dir/bin/*" \
        -not -path "$project_dir/obj/*" \
        -print -quit)" ]]; then
        continue
    fi

    if [[ "${UNIT_TESTS_ONLY:-false}" == "true" && "$project" == *IntegrationTests.csproj ]]; then
        continue
    fi

    dotnet test "$project" \
        --configuration "$CONFIGURATION" \
        --no-build \
        "$@"
done
