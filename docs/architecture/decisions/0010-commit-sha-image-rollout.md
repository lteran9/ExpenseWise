# ADR 0010: Commit-SHA images and RBAC waits for the first deployment

- Status: Accepted
- Date: 2026-11-13
- Amends: the placeholder-image staged rollout in the [Azure deployment specification](../../product/azure-deployment.md) and the `tf-plan` rollout flags and staged-workload tests in [ADR 0009](0009-terraform-bootstrap-and-environment-roots.md).

## Context

Package 3.2 planned to create the infrastructure without workloads (`enable_workloads = false`), wait for role assignments with a script, create the apps and jobs from a public placeholder image without probes, release the real images, and then enable probes (`enable_probes = true`). `make tf-plan` required both flags so that a plan never fell back to defaults that would remove running workloads.

Reviewing the first plan before it was applied showed that the identity running Terraform holds only Owner on the subscription. Owner grants no Key Vault data actions, so creating the generated secrets in the RBAC-mode vault would fail with HTTP 403.

The owner chose to follow their other Terraform project for both problems: grant the identity running Terraform `Key Vault Secrets Officer` on the vault, wait for RBAC propagation with `time_sleep`, and create workloads from registry images tagged with the Git commit SHA instead of placeholders.

The API readiness probe checks the database as the least-privilege application user, which exists only after the database user job runs. Creating the API from the real image before that job would leave its revision unready.

## Decision

- Terraform assigns `Key Vault Secrets Officer` on the vault to the identity that runs it, waits 60 seconds, and then writes secrets.
- A 60-second `time_sleep` follows all workload `AcrPull` and Key Vault secret role assignments; every Container App and job depends on it. The role-assignment polling script and its Terraform output are removed.
- The `image_tag` variable, validated as a full lowercase Git commit SHA, selects the registry images that apps and jobs are created with. Terraform still ignores later image changes. `enable_workloads` and `enable_probes` are removed; the apps, jobs, and probes are always defined.
- `make tf-plan` passes `IMAGE_TAG`, which defaults to the checked-out commit, and accepts `TF_TARGETS` for the first deployment. The first deployment runs these steps from one clean commit, as described in the specification's runbook:
  1. A targeted apply creates the budget and registry.
  2. The images are built in the registry.
  3. A targeted apply creates the two jobs and their dependencies.
  4. The database user and migration jobs run.
  5. A full apply creates the API and UI.
- `release.sh` takes an optional stage (`images`, `database-jobs`, or `all`) and honors `TERRAFORM`. The `make azure-images`, `azure-database-jobs`, and `azure-release` targets run it for `IMAGE_TAG`.

## Consequences

- The identity running Terraform can read and change every secret in the vault. Workload identities keep read access to individual secrets only.
- A fixed wait does not guarantee propagation. If a create still fails for authorization reasons, the owner waits and plans and applies again.
- Two targeted applies are part of the first deployment, so Terraform warns that targeting is in effect and those plans are incomplete by design. After the full apply, plans are untargeted, and a second plan must show no changes.
- A plan for a different commit does not change running images, because Terraform ignores them. Recreating an app or job after its images are deleted requires building that commit's images first.
- Probes are active on the first revision, so a broken image or missing database user fails visibly when an app is created rather than after a later probe change.
