#!/usr/bin/env bash
set -euo pipefail

[[ $# -eq 2 ]] || { echo "Usage: $0 <resource-group> <container-app>" >&2; exit 2; }
resource_group=$1
app_name=$2
timeout_seconds=${WAIT_TIMEOUT_SECONDS:-600}
poll_seconds=${WAIT_POLL_SECONDS:-10}

[[ "$timeout_seconds" =~ ^[1-9][0-9]{0,5}$ ]] || { echo "WAIT_TIMEOUT_SECONDS must be a positive integer no greater than 999999." >&2; exit 2; }
[[ "$poll_seconds" =~ ^[1-9][0-9]{0,5}$ ]] || { echo "WAIT_POLL_SECONDS must be a positive integer no greater than 999999." >&2; exit 2; }

command -v az >/dev/null || { echo "Azure CLI (az) is required." >&2; exit 1; }

elapsed=0
while (( elapsed < timeout_seconds )); do
  running_state=$(az containerapp show \
    --name "$app_name" \
    --resource-group "$resource_group" \
    --query properties.runningStatus \
    --output tsv)

  if [[ "$running_state" == "Running" ]]; then
    echo "Container App $app_name is running."
    exit 0
  fi

  sleep "$poll_seconds"
  elapsed=$((elapsed + poll_seconds))
done

echo "Timed out waiting for Container App $app_name to run." >&2
exit 1
