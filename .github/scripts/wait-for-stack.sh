#!/usr/bin/env bash
# Waits until the Compose-hosted UI serves its home page and the API answers an anonymous request with 401.
# Usage: wait-for-stack.sh [ui-url] [api-url] [attempts]
set -euo pipefail

ui_url="${1:-http://localhost:8080}"
api_url="${2:-http://localhost:8081}"
attempts="${3:-60}"

for ((i = 1; i <= attempts; i++)); do
  ui_status=$(curl -s -o /dev/null -w '%{http_code}' "$ui_url/" || true)
  api_status=$(curl -s -o /dev/null -w '%{http_code}' "$api_url/Group/List" || true)
  if [[ "$ui_status" == "200" && "$api_status" == "401" ]]; then
    echo "Stack ready after $i attempt(s)."
    exit 0
  fi
  sleep 2
done

echo "Stack not ready: UI returned $ui_status and API returned $api_status." >&2
exit 1
