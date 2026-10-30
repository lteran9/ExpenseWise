#!/usr/bin/env bash
set -euo pipefail

script_dir=$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" && pwd)
terraform_dir=$(cd -- "$script_dir/.." && pwd)
timeout_seconds=${1:-300}
poll_seconds=${2:-10}

[[ $# -le 2 ]] || { echo "Usage: $0 [timeout-seconds] [poll-seconds]" >&2; exit 2; }
[[ "$timeout_seconds" =~ ^[1-9][0-9]{0,5}$ ]] || { echo "Timeout must be a positive integer no greater than 999999." >&2; exit 2; }
[[ "$poll_seconds" =~ ^[1-9][0-9]{0,5}$ ]] || { echo "Poll interval must be a positive integer no greater than 999999." >&2; exit 2; }

command -v az >/dev/null || { echo "Azure CLI (az) is required." >&2; exit 1; }
command -v python3 >/dev/null || { echo "Python 3 is required." >&2; exit 1; }
az account show >/dev/null 2>&1 || { echo "Run 'az login' and select the intended subscription first." >&2; exit 1; }

checks=$(terraform -chdir="$terraform_dir" output -json workload_role_assignments)
if [[ -z "$checks" || "$checks" == "null" ]]; then
  echo "Terraform has no workload role assignments to verify." >&2
  exit 1
fi

while IFS=$'\t' read -r principal_id scope role_name; do
  elapsed=0
  while (( elapsed < timeout_seconds )); do
    assignments=$(az role assignment list \
      --assignee "$principal_id" \
      --scope "$scope" \
      --query "[?roleDefinitionName=='$role_name'].roleDefinitionName" \
      --output tsv)
    if [[ "$assignments" == "$role_name" ]]; then
      break
    fi
    sleep "$poll_seconds"
    elapsed=$((elapsed + poll_seconds))
  done

  if (( elapsed >= timeout_seconds )); then
    echo "Timed out waiting for the $role_name assignment to appear for a workload identity." >&2
    exit 1
  fi
done < <(python3 -c '
import json, sys
for item in json.load(sys.stdin):
    print(item["principal_id"], item["scope"], item["role"], sep="\t")
' <<<"$checks")

echo "All configured workload role assignments are visible to Azure Resource Manager."
echo "Data-plane authorization can still take time to propagate; retry a failed image pull or Key Vault startup once."
