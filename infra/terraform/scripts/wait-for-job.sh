#!/usr/bin/env bash
set -euo pipefail

[[ $# -eq 3 ]] || { echo "Usage: $0 <resource-group> <job-name> <execution-name>" >&2; exit 2; }
resource_group=$1
job_name=$2
execution_name=$3
timeout_seconds=${WAIT_TIMEOUT_SECONDS:-900}
poll_seconds=${WAIT_POLL_SECONDS:-10}

[[ "$timeout_seconds" =~ ^[1-9][0-9]{0,5}$ ]] || { echo "WAIT_TIMEOUT_SECONDS must be a positive integer no greater than 999999." >&2; exit 2; }
[[ "$poll_seconds" =~ ^[1-9][0-9]{0,5}$ ]] || { echo "WAIT_POLL_SECONDS must be a positive integer no greater than 999999." >&2; exit 2; }

command -v az >/dev/null || { echo "Azure CLI (az) is required." >&2; exit 1; }

elapsed=0
while (( elapsed < timeout_seconds )); do
  status=$(az containerapp job execution show \
    --name "$job_name" \
    --resource-group "$resource_group" \
    --job-execution-name "$execution_name" \
    --query properties.status \
    --output tsv)

  case "$status" in
    Succeeded)
      echo "Job $job_name completed successfully."
      exit 0
      ;;
    Failed|Stopped|Degraded)
      echo "Job $job_name ended with status $status; release stopped before app updates." >&2
      exit 1
      ;;
  esac

  sleep "$poll_seconds"
  elapsed=$((elapsed + poll_seconds))
done

echo "Timed out waiting for job $job_name; release stopped before app updates." >&2
exit 1
