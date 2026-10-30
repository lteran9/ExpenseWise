# ADR 0007: Dedicated Terraform state storage for ExpenseWise

- Status: Accepted
- Date: 2026-10-29
- Supersedes: [ADR 0006](0006-terraform-infrastructure-as-code.md) if accepted

## Context

[ADR 0006](0006-terraform-infrastructure-as-code.md) records the accepted decision to store ExpenseWise state in a new container in an existing storage account shared with another project. The owner has since requested a dedicated ExpenseWise resource group and storage account instead. This proposal changes only Terraform state storage; the application environment remains in its own resource group as described in the [Azure deployment specification](../../product/azure-deployment.md).

The state account must exist before the main Terraform root can initialize its AzureRM backend. It therefore cannot be created by that same root without first having a backend. The account will be created by a separate, idempotent bootstrap script before the first remote-backend initialization. No ExpenseWise state has been created yet, so accepting this proposal before bootstrap avoids a state migration.

## Decision

- Create a dedicated state resource group named `rg-expensewise-tfstate` in `westus3`, containing only an ExpenseWise Terraform state storage account.
- Use a globally unique, generated storage account name and `Standard_GRS` redundancy. Keep account creation outside the application Terraform root.
- Disable shared-key authorization and public blob access; require HTTPS and TLS 1.2; use Microsoft Entra ID for state access.
- Enable blob versioning and 30-day blob and container soft delete. Apply a `CanNotDelete` lock to the state resource group so accidental deletion requires an explicit unlock.
- Store the Terraform state in the private `expensewise-tfstate` container with the `expensewise-dev.tfstate` key. Keep backend settings in the ignored `backend.hcl`.
- Restrict automation identities to `Storage Blob Data Contributor` on the state container. The owner identity bootstrapping the account needs the permissions required to create the account, container, and resource-group lock.
- Only the owner runs `terraform apply`, after reviewing the plan. Release automation updates images and runs jobs but does not apply Terraform.

## Consequences

- ExpenseWise state is isolated from other projects' storage accounts and access policies. The state still contains generated secrets, so its access and recovery controls remain security-critical.
- The dedicated account adds storage and transaction costs. Geo-redundancy, versioning, soft delete, and the resource-group lock protect recovery, but do not prevent authorized data-plane deletion of blobs.
- The one-time bootstrap is a separate Azure CLI operation and must report the generated account name and confirm the security settings before the owner proceeds to Terraform initialization.
- The state resource group and storage account are retained when the application resource group is destroyed. Deleting them requires removing the lock and intentionally removing the state backend.
- ADR 0006 is superseded. The Azure deployment specification reflects this dedicated state design.
