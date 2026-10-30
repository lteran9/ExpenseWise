#!/usr/bin/env bash
set -euo pipefail

usage() {
  echo "Usage: $0 <full-commit-sha>" >&2
  exit 2
}

[[ $# -eq 1 ]] || usage
commit_sha=$1
[[ "$commit_sha" =~ ^[0-9a-f]{40}$ ]] || { echo "Provide a full 40-character lowercase commit SHA." >&2; exit 2; }

script_dir=$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" && pwd)
terraform_dir=$(cd -- "$script_dir/.." && pwd)
repo_root=$(git -C "$terraform_dir" rev-parse --show-toplevel)

command -v az >/dev/null || { echo "Azure CLI (az) is required." >&2; exit 1; }
command -v terraform >/dev/null || { echo "Terraform is required." >&2; exit 1; }
az account show >/dev/null 2>&1 || { echo "Run 'az login' and select the intended subscription first." >&2; exit 1; }

current_sha=$(git -C "$repo_root" rev-parse HEAD)
[[ "$current_sha" == "$commit_sha" ]] || { echo "The requested SHA must be the checked-out commit." >&2; exit 1; }
if [[ -n "$(git -C "$repo_root" status --porcelain)" ]]; then
  echo "Refusing to release from a dirty working tree." >&2
  exit 1
fi

resource_group=$(terraform -chdir="$terraform_dir" output -raw resource_group_name)
registry=$(terraform -chdir="$terraform_dir" output -raw container_registry_name)
api_app=$(terraform -chdir="$terraform_dir" output -raw container_app_api_name)
ui_app=$(terraform -chdir="$terraform_dir" output -raw container_app_ui_name)
database_user_job=$(terraform -chdir="$terraform_dir" output -raw database_user_job_name)
migration_job=$(terraform -chdir="$terraform_dir" output -raw migration_job_name)
registry_server=$(terraform -chdir="$terraform_dir" output -raw container_registry_login_server)

"$script_dir/wait-for-role-assignments.sh"

echo "Importing the pinned MySQL 8.0.44 client base image..."
az acr import \
  --name "$registry" \
  --source "docker.io/library/mysql@sha256:9c3380eac945af0736031b200027f581925927c81e010056214a4bd6b6693714" \
  --image "mysql-client-base:8.0.44" \
  --force \
  --output none

echo "Building immutable commit-tagged application images..."
az acr build \
  --registry "$registry" \
  --image "expensewise-api:$commit_sha" \
  --file src/Presentation/Api/Dockerfile \
  "$repo_root"
az acr build \
  --registry "$registry" \
  --image "expensewise-ui:$commit_sha" \
  --file src/Presentation/UI/Dockerfile \
  "$repo_root"
az acr build \
  --registry "$registry" \
  --image "expensewise-mysql-client:$commit_sha" \
  --file infra/terraform/mysql-client/Dockerfile \
  --build-arg "MYSQL_BASE_IMAGE=$registry_server/mysql-client-base:8.0.44" \
  "$repo_root"

mysql_client_digest=$(az acr repository show \
  --name "$registry" \
  --image "expensewise-mysql-client:$commit_sha" \
  --query digest \
  --output tsv)
[[ "$mysql_client_digest" =~ ^sha256:[0-9a-f]{64}$ ]] || {
  echo "Unable to resolve the immutable MySQL client image digest." >&2
  exit 1
}

echo "Updating the database-user job and waiting for it to succeed..."
az containerapp job update \
  --name "$database_user_job" \
  --resource-group "$resource_group" \
  --image "$registry_server/expensewise-mysql-client@$mysql_client_digest" \
  --output none
database_user_execution=$(az containerapp job start \
  --name "$database_user_job" \
  --resource-group "$resource_group" \
  --query name \
  --output tsv)
"$script_dir/wait-for-job.sh" "$resource_group" "$database_user_job" "$database_user_execution"

echo "Updating the migration job and waiting for it to succeed..."
az containerapp job update \
  --name "$migration_job" \
  --resource-group "$resource_group" \
  --image "$registry_server/expensewise-api:$commit_sha" \
  --output none
migration_execution=$(az containerapp job start \
  --name "$migration_job" \
  --resource-group "$resource_group" \
  --query name \
  --output tsv)
"$script_dir/wait-for-job.sh" "$resource_group" "$migration_job" "$migration_execution"

echo "Updating the API, waiting for its revision, then updating the UI..."
az containerapp update \
  --name "$api_app" \
  --resource-group "$resource_group" \
  --image "$registry_server/expensewise-api:$commit_sha" \
  --output none
"$script_dir/wait-for-app.sh" "$resource_group" "$api_app" "$registry_server/expensewise-api:$commit_sha"
az containerapp update \
  --name "$ui_app" \
  --resource-group "$resource_group" \
  --image "$registry_server/expensewise-ui:$commit_sha" \
  --output none
"$script_dir/wait-for-app.sh" "$resource_group" "$ui_app" "$registry_server/expensewise-ui:$commit_sha"

ui_url=$(terraform -chdir="$terraform_dir" output -raw container_app_ui_url)
echo "Release $commit_sha completed. Verify the public browser journey at $ui_url."
