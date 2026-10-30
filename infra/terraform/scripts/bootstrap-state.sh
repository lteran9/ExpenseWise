#!/usr/bin/env bash
set -euo pipefail

[[ $# -eq 0 ]] || { echo "Usage: $0" >&2; exit 2; }

state_resource_group=rg-expensewise-tfstate
location=westus3
container=expensewise-tfstate
account_prefix=stexpensewisetf
lock_name=expensewise-tfstate-protect
script_dir=$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" && pwd)
terraform_dir=$(cd -- "$script_dir/.." && pwd)

command -v az >/dev/null || { echo "Azure CLI (az) is required." >&2; exit 1; }
command -v python3 >/dev/null || { echo "Python 3 is required." >&2; exit 1; }
az account show >/dev/null 2>&1 || { echo "Run 'az login' and select the intended subscription first." >&2; exit 1; }

if [[ "$(az group exists --name "$state_resource_group")" != "true" ]]; then
  az group create \
    --name "$state_resource_group" \
    --location "$location" \
    --tags application=ExpenseWise environment=dev managed_by=Bootstrap owner=ExpenseWise \
    --output none
fi

group_location=$(az group show --name "$state_resource_group" --query location --output tsv)
group_location_lower=$(printf '%s' "$group_location" | tr '[:upper:]' '[:lower:]')
if [[ "$group_location_lower" != "$location" ]]; then
  echo "The dedicated state resource group exists in $group_location, expected $location." >&2
  exit 1
fi

account_names=$(az storage account list \
  --resource-group "$state_resource_group" \
  --query "[].name" \
  --output tsv)
accounts=()
while IFS= read -r account_name; do
  [[ -n "$account_name" ]] && accounts+=("$account_name")
done <<<"$account_names"

if (( ${#accounts[@]} > 1 )); then
  echo "The dedicated state resource group must contain only one storage account." >&2
  exit 1
fi

if (( ${#accounts[@]} == 1 )); then
  storage_account=${accounts[0]}
  if [[ ! "$storage_account" =~ ^${account_prefix}[a-z0-9]{8}$ ]]; then
    echo "The existing storage account name does not match the ExpenseWise state naming convention." >&2
    exit 1
  fi
else
  unexpected_resources=$(az resource list \
    --resource-group "$state_resource_group" \
    --query "[?type!='Microsoft.Authorization/locks'].id" \
    --output tsv)
  if [[ -n "$unexpected_resources" ]]; then
    echo "Refusing to create state storage in a resource group containing unrelated resources." >&2
    exit 1
  fi

  storage_account=
  for attempt in 1 2 3 4 5; do
    candidate=$(python3 -c 'import secrets, string; print("stexpensewisetf" + "".join(secrets.choice(string.ascii_lowercase + string.digits) for _ in range(8)))')
    name_available=$(az storage account check-name --name "$candidate" --query nameAvailable --output tsv)
    if [[ "$name_available" == "true" ]]; then
      storage_account=$candidate
      break
    fi
  done
  [[ -n "$storage_account" ]] || { echo "Could not generate an available globally unique storage account name." >&2; exit 1; }

  az storage account create \
    --name "$storage_account" \
    --resource-group "$state_resource_group" \
    --location "$location" \
    --sku Standard_GRS \
    --kind StorageV2 \
    --https-only true \
    --min-tls-version TLS1_2 \
    --allow-blob-public-access false \
    --allow-shared-key-access false \
    --tags application=ExpenseWise environment=dev managed_by=Bootstrap owner=ExpenseWise \
    --output none
fi

account_location=$(az storage account show \
  --name "$storage_account" \
  --resource-group "$state_resource_group" \
  --query location \
  --output tsv)
account_sku=$(az storage account show \
  --name "$storage_account" \
  --resource-group "$state_resource_group" \
  --query sku.name \
  --output tsv)
account_kind=$(az storage account show \
  --name "$storage_account" \
  --resource-group "$state_resource_group" \
  --query kind \
  --output tsv)
account_location_lower=$(printf '%s' "$account_location" | tr '[:upper:]' '[:lower:]')
if [[ "$account_location_lower" != "$location" || "$account_sku" != "Standard_GRS" || "$account_kind" != "StorageV2" ]]; then
  echo "The dedicated state storage account must be StorageV2, Standard_GRS, and located in $location." >&2
  exit 1
fi

unexpected_resources=$(az resource list \
  --resource-group "$state_resource_group" \
  --query "[?type!='Microsoft.Storage/storageAccounts' && type!='Microsoft.Authorization/locks'].id" \
  --output tsv)
if [[ -n "$unexpected_resources" ]]; then
  echo "The dedicated state resource group contains resources other than its state storage account." >&2
  exit 1
fi

az storage account update \
  --name "$storage_account" \
  --resource-group "$state_resource_group" \
  --https-only true \
  --min-tls-version TLS1_2 \
  --allow-blob-public-access false \
  --allow-shared-key-access false \
  --output none

az storage account blob-service-properties update \
  --account-name "$storage_account" \
  --resource-group "$state_resource_group" \
  --enable-versioning true \
  --enable-delete-retention true \
  --delete-retention-days 30 \
  --enable-container-delete-retention true \
  --container-delete-retention-days 30 \
  --output none

az lock create \
  --name "$lock_name" \
  --lock-type CanNotDelete \
  --resource-group "$state_resource_group" \
  --output none

subscription_id=$(az account show --query id --output tsv)
container_url="https://management.azure.com/subscriptions/$subscription_id/resourceGroups/$state_resource_group/providers/Microsoft.Storage/storageAccounts/$storage_account/blobServices/default/containers/$container?api-version=2023-05-01"
az rest \
  --method put \
  --url "$container_url" \
  --body '{"properties":{"publicAccess":"None"}}' \
  --output none

public_access=$(az rest \
  --method get \
  --url "$container_url" \
  --query properties.publicAccess \
  --output tsv)
if [[ -n "$public_access" && "$public_access" != "None" ]]; then
  echo "The Terraform state container must not allow public access (current setting: $public_access)." >&2
  exit 1
fi

cat >"$terraform_dir/backend.hcl" <<EOF
resource_group_name  = "$state_resource_group"
storage_account_name = "$storage_account"
container_name       = "$container"
key                  = "expensewise-dev.tfstate"
use_azuread_auth     = true
EOF

echo "Created or verified the dedicated ExpenseWise state backend."
echo "Resource group: $state_resource_group"
echo "Storage account: $storage_account"
echo "Container: $container (private)"
echo "The identity running Terraform needs Storage Blob Data Contributor on the container."
echo "Next: cd infra/terraform && terraform init -reconfigure -backend-config=backend.hcl"
