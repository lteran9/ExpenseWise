#!/usr/bin/env bash
set -euo pipefail

usage() {
  echo "Usage: $0 <full-commit-sha> [images|database-jobs|all]" >&2
  echo "  images         Build the commit-tagged images in the registry." >&2
  echo "  database-jobs  Run the database-user job, then the migration job." >&2
  echo "  all            Both of the above, then update the API and UI (default)." >&2
  exit 2
}

[[ $# -ge 1 && $# -le 2 ]] || usage
commit_sha=$1
stage=${2:-all}
[[ "$commit_sha" =~ ^[0-9a-f]{40}$ ]] || { echo "Provide a full 40-character lowercase commit SHA." >&2; exit 2; }
case "$stage" in
  images | database-jobs | all) ;;
  *) usage ;;
esac

script_dir=$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" && pwd)
terraform_dir=$(cd -- "$script_dir/.." && pwd)
repo_root=$(git -C "$terraform_dir" rev-parse --show-toplevel)
terraform_bin=${TERRAFORM:-terraform}

command -v az >/dev/null || { echo "Azure CLI (az) is required." >&2; exit 1; }
command -v "$terraform_bin" >/dev/null || { echo "Terraform is required on PATH; set TERRAFORM to use another binary." >&2; exit 1; }
az account show >/dev/null 2>&1 || { echo "Run 'az login' and select the intended subscription first." >&2; exit 1; }

current_sha=$(git -C "$repo_root" rev-parse HEAD)
[[ "$current_sha" == "$commit_sha" ]] || { echo "The requested SHA must be the checked-out commit." >&2; exit 1; }
if [[ -n "$(git -C "$repo_root" status --porcelain)" ]]; then
  echo "Refusing to release from a dirty working tree." >&2
  exit 1
fi

tf_output() {
  "$terraform_bin" -chdir="$terraform_dir" output -raw "$1"
}

registry=$(tf_output container_registry_name)
registry_server=$(tf_output container_registry_login_server)

build_images() {
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
    --platform linux/amd64 \
    --file src/Presentation/Api/Dockerfile \
    "$repo_root"
  az acr build \
    --registry "$registry" \
    --image "expensewise-ui:$commit_sha" \
    --platform linux/amd64 \
    --file src/Presentation/UI/Dockerfile \
    "$repo_root"
  az acr build \
    --registry "$registry" \
    --image "expensewise-mysql-client:$commit_sha" \
    --platform linux/amd64 \
    --file infra/azure/mysql-client/Dockerfile \
    --build-arg "MYSQL_BASE_IMAGE=$registry_server/mysql-client-base:8.0.44" \
    "$repo_root"
}

run_database_jobs() {
  local resource_group database_user_job migration_job mysql_client_digest execution
  resource_group=$(tf_output apps_resource_group_name)
  database_user_job=$(tf_output database_user_job_name)
  migration_job=$(tf_output migration_job_name)

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
  execution=$(az containerapp job start \
    --name "$database_user_job" \
    --resource-group "$resource_group" \
    --query name \
    --output tsv)
  "$script_dir/wait-for-job.sh" "$resource_group" "$database_user_job" "$execution"

  echo "Updating the migration job and waiting for it to succeed..."
  az containerapp job update \
    --name "$migration_job" \
    --resource-group "$resource_group" \
    --image "$registry_server/expensewise-api:$commit_sha" \
    --output none
  execution=$(az containerapp job start \
    --name "$migration_job" \
    --resource-group "$resource_group" \
    --query name \
    --output tsv)
  "$script_dir/wait-for-job.sh" "$resource_group" "$migration_job" "$execution"
}

update_apps() {
  local resource_group api_app ui_app
  resource_group=$(tf_output apps_resource_group_name)
  api_app=$(tf_output container_app_api_name)
  ui_app=$(tf_output container_app_ui_name)

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

  echo "Release $commit_sha completed. Verify the public browser journey at $(tf_output container_app_ui_url)."
}

case "$stage" in
  images) build_images ;;
  database-jobs) run_database_jobs ;;
  all)
    build_images
    run_database_jobs
    update_apps
    ;;
esac
