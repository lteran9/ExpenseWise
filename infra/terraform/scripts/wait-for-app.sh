#!/usr/bin/env bash
set -euo pipefail

[[ $# -eq 3 ]] || { echo "Usage: $0 <resource-group> <container-app> <expected-image>" >&2; exit 2; }
resource_group=$1
app_name=$2
expected_image=$3
timeout_seconds=${WAIT_TIMEOUT_SECONDS:-600}
poll_seconds=${WAIT_POLL_SECONDS:-10}

[[ "$timeout_seconds" =~ ^[1-9][0-9]{0,5}$ ]] || { echo "WAIT_TIMEOUT_SECONDS must be a positive integer no greater than 999999." >&2; exit 2; }
[[ "$poll_seconds" =~ ^[1-9][0-9]{0,5}$ ]] || { echo "WAIT_POLL_SECONDS must be a positive integer no greater than 999999." >&2; exit 2; }

command -v az >/dev/null || { echo "Azure CLI (az) is required." >&2; exit 1; }

elapsed=0
while (( elapsed < timeout_seconds )); do
  latest_revision=$(az containerapp show \
    --name "$app_name" \
    --resource-group "$resource_group" \
    --query properties.latestRevisionName \
    --output tsv)

  if [[ -n "$latest_revision" && "$latest_revision" != "None" ]]; then
    revision_status=$(az containerapp revision show \
      --name "$app_name" \
      --resource-group "$resource_group" \
      --revision "$latest_revision" \
      --query "[properties.provisioningState, properties.runningState, properties.template.containers[0].image]" \
      --output tsv)
    IFS=$'\t' read -r provisioning_state running_state revision_image <<< "$revision_status"

    if [[ "$provisioning_state" == "Provisioned" && "$running_state" == "Running" && "$revision_image" == "$expected_image" ]]; then
      echo "Latest revision $latest_revision of Container App $app_name is running the expected image."
      exit 0
    fi
  fi

  sleep "$poll_seconds"
  elapsed=$((elapsed + poll_seconds))
done

echo "Timed out waiting for Container App $app_name to run the expected image." >&2
exit 1
