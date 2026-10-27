# ADR 0006: Terraform infrastructure as code with shared remote state

- Status: Accepted
- Date: 2026-10-27

## Context

The owner chose Terraform to provision the Azure demo environment described in the [Phase 3 specification](../../product/azure-deployment.md) and [ADR 0005](0005-azure-container-apps-hosting.md). Terraform state must be stored remotely so that it survives a single workstation and can be used by a deployment pipeline later.

The subscription already holds a Terraform state storage account that another of the owner's projects created and manages with its own Terraform. It disables shared-key access, enables blob versioning and 30-day soft delete for blobs and containers, uses geo-redundant storage, and has a delete lock on its resource group. The owner chose to reuse it instead of creating a dedicated account.

## Decision

- Define the environment in `infra/terraform` as one root module, split into files by concern, with pinned Terraform and `azurerm` and `random` provider versions and a committed `.terraform.lock.hcl`. Do not introduce reusable modules or workspaces until a second environment exists.
- Store ExpenseWise state in a new, dedicated container in the shared storage account, with its own state key. Create the container with the Azure CLI as a documented one-time bootstrap step, not from either project's Terraform.
- Use the `azurerm` backend with Microsoft Entra ID authentication (`use_azuread_auth`), because shared-key access is disabled. Supply backend settings through an uncommitted `backend.hcl`.
- Grant any ExpenseWise automation identity `Storage Blob Data Contributor` on the ExpenseWise state container only, never on the storage account.
- Let Terraform generate application secrets, store them in Key Vault, and accept that they also appear in state.
- Only the owner runs `terraform apply`, after reviewing the plan. The deployment pipeline updates container images and runs jobs but does not apply Terraform.

## Consequences

- ExpenseWise depends on infrastructure owned by another project:
  - If that project's Terraform later restricts network access, moves or replaces the storage account, or changes its authorization settings, ExpenseWise `init`, `plan`, and pipeline access can break. The resource group's delete lock blocks control-plane deletion of the storage account, so a replacement fails instead of deleting it.
  - The ExpenseWise container is created outside that project's Terraform. It is not deleted by that project's plans, but it is also undocumented there, so the other project's documentation should mention it.
  - The delete lock does not protect containers or blobs deleted through the Storage data plane. Blob versioning and soft delete, not the lock, are what make ExpenseWise state recoverable. Removing role assignments later may require temporarily lifting the lock.
  - Storage costs and tags are attributed to the other project. The cost is negligible.
- Anyone with data access to the whole storage account, including the owner's existing account-level role, can read both projects' state, and therefore both projects' generated secrets. Container-scoped assignments for automation limit this exposure but do not remove it for account-level principals.
- Versioning and 30-day soft delete on the shared account allow recovery of an overwritten or deleted ExpenseWise state file.
- Moving to a dedicated state account later requires `terraform init -migrate-state` and a new decision; it does not affect deployed resources.
- Generated secrets can be rotated by replacing their `random` resources (`terraform apply -replace=...`) and running the full [scripted release sequence](../../product/azure-deployment.md#release-sequence). Ensure the affected workloads consume the updated Key Vault secrets first: an administrator password change must reach both jobs before they run, and an application password change must reach the database user job and be successfully applied in MySQL before the API is updated. Restart affected apps to consume their updated secrets.
