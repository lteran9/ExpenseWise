# ADR 0009: Terraform bootstrap and environment roots

- Status: Accepted; the staged-workload rollout and tests amended by [ADR 0010](0010-commit-sha-image-rollout.md)
- Date: 2026-11-13
- Amends: [ADR 0007](0007-dedicated-expensewise-terraform-state.md) (state bootstrap mechanism, container name, and backend configuration) and replaces the `infra/terraform` layout from [ADR 0006](0006-terraform-infrastructure-as-code.md). Related: [ADR 0008](0008-resource-groups-by-lifecycle.md).

## Context

Package 3.2 first placed one Terraform root in `infra/terraform`. A Bash script created the ADR 0007 state storage with the Azure CLI and wrote an ignored `backend.hcl`, the dev variables lived beside the configuration, and only `terraform fmt` and `validate` were run, manually.

The owner asked to model the infrastructure on another of their projects. That project bootstraps its state storage with a separate Terraform root, keeps per-environment inputs in an `environments` directory, tests its Terraform with mock providers, and runs formatting, validation, tests, and a Checkov scan from Make targets in CI.

No application resources existed, and the state blob created by the bootstrap script was empty, so the state backend could be recreated without a migration.

## Decision

- **`infra/bootstrap`** is a Terraform root with local state that replaces the bootstrap script. It creates the `rg-expensewise-tfstate` group, a generated state storage account with the ADR 0007 security and recovery settings, one private `tfstate-<environment>` container per environment, a container-scoped `Storage Blob Data Contributor` assignment for the identity that runs it, and the `CanNotDelete` resource-group lock. Its state file is ignored by Git; the owner keeps a private copy.
- **`infra/azure`** is the application root, split into files by concern (`versions.tf`, `main.tf`, `network.tf`, `database.tf`, `identity.tf`, `keyvault.tf`, `registry.tf`, `container_apps.tf`, `budget.tf`, `variables.tf`, and `outputs.tf`). The release scripts and the database user job image live beside it in `scripts/` and `mysql-client/`. It remains one root and one state per environment, without reusable modules or workspaces.
- Environment inputs are committed in `infra/azure/environments/<environment>.tfvars` and `<environment>.backend.hcl`. The backend file omits the storage account name, which is supplied at initialization; the subscription comes from `ARM_SUBSCRIPTION_ID`. Private inputs, such as the budget contact, go in the ignored `infra/azure/local.auto.tfvars`. The dev state uses the `expensewise.tfstate` key in the `tfstate-dev` container.
- The Terraform CLI is not pinned in either root or CI: both roots use the Terraform executable on `PATH` (overridable via `TERRAFORM=<path>` in Make targets), and CI installs the latest release. Exact versions of the `azurerm`, `random`, and `time` providers remain pinned, and both roots commit their dependency lock files.
- The subscription budget starts on the first day of the month in which it is first applied, recorded by a `time_static` resource, and has no end date. This replaces hard-coded budget dates.
- A root `Makefile` provides `tf-fmt`, `tf-validate`, `tf-test`, `tf-scan`, and `tf-check` for credential-free checks, and `tf-init`, `tf-plan`, and `tf-apply` for the owner. `tf-plan` passes `IMAGE_TAG`, accepts optional `TF_TARGETS` for the first deployment, and saves a private plan file that `tf-apply` applies unchanged.
- `infra/azure/tests` contains mock-provider Terraform tests for resource-group placement, tags, the budget, private MySQL access, ingress exposure, RBAC propagation, and commit-tagged workloads.
- The `infrastructure` CI job runs `make tf-check` without Azure credentials. The Checkov image is pinned by digest. Accepted trade-offs are recorded as `#checkov:skip` comments that state their reason, so any new finding fails the build.
- Only the owner runs `terraform apply`, after reviewing the plan.

## Consequences

- The state bootstrap is reviewable, repeatable Terraform instead of imperative CLI calls. Its local state is not shared: if it is lost, the existing resources must be imported before the bootstrap root can change them. The resource-group lock still guards against accidental deletion.
- The state account created by the bootstrap script was deleted and recreated by `infra/bootstrap`, so its generated name changed. The ADR 0007 security posture, state isolation, and owner-only apply remain in force.
- Adding an environment requires adding it to the bootstrap `environments` variable and adding the environment's `.tfvars` and `.backend.hcl` files.
- Pull requests and pushes to `master` catch formatting, validation, placement, and security-scan regressions without Azure access. These checks do not prove that a plan applies; the owner's reviewed plan remains the gate.
- Removing an accepted trade-off, such as adding private endpoints or subnet network security groups, means removing the matching Checkov skip.
- Operators need the pinned Terraform version. The Make targets accept `TERRAFORM=<path>` to select it.
