# ADR 0012: On-demand lifecycle for the Azure demo

- Status: Accepted
- Date: 2026-11-18
- Related: [ADR 0005](0005-azure-container-apps-hosting.md), [ADR 0007](0007-dedicated-expensewise-terraform-state.md), and [ADR 0008](0008-resource-groups-by-lifecycle.md)

## Context

The Azure environment is a portfolio demonstration rather than a continuously available service. Azure Container Apps requires a platform-managed Standard Load Balancer and static public IP for the external UI ingress in the VNet-integrated environment. The load balancer and IP are created in the Azure-managed `me_expensewise_dev` resource group; deleting either resource directly would break or drift from the Container Apps environment.

Live Cost Management data on 2026-11-18 measured the managed load balancer at USD 0.60 per day and its Standard public IP at USD 0.12 per day, approximately USD 21.60 for a 30-day month before application, database, registry, DNS, and state-storage charges. Stopping only MySQL does not remove the managed load balancer, and Azure restarts a stopped Flexible Server automatically after 30 days.

## Decision

- Operate the Azure demo on demand. The architecture from ADR 0005 remains unchanged while the environment is deployed.
- Treat the `infra/azure` environment root as disposable. Tear it down with a saved, owner-reviewed Terraform destroy plan and require an environment-specific confirmation before applying that plan.
- Keep the separate `infra/bootstrap` state backend while the demo is down. Its resource group, storage account, state container, and deletion lock continue to support the next deployment and are not part of routine teardown.
- Treat application data and images as disposable during full teardown. Destroying `infra/azure` removes the active MySQL server and database, Container Registry and its images, Key Vault, the Container Apps environment, and the platform-managed load balancer and public IP. Azure retains a deleted Key Vault in soft-deleted state for the configured seven-day retention period.
- Recreate the demo through the existing staged deployment runbook. The next deployment may receive new generated resource names, secrets, Container Apps domains, and demo data because the random resources and all application resources start from empty state.
- Do not destroy resources directly in the Azure portal or Azure-managed infrastructure group; reconcile the full lifecycle through Terraform.

## Consequences

- Routine teardown eliminates the Container Apps managed ingress charge and all application-root resource charges instead of leaving the demo mostly running for no revenue or user traffic.
- Only the separately protected state backend remains billed; its observed cost is small, but is not guaranteed to remain free.
- There is no public demo URL between deployments. Recreating it requires the staged registry, database-job, and application sequence because applications need images and an initialized database before they can become ready.
- A full teardown loses the active demo's MySQL data and registry contents. Back up and restore data separately before teardown if a particular demo dataset must survive; the normal CV workflow may recreate disposable demonstration data.
- The environment is intentionally not production-managed. There is no automatic teardown schedule or cost cap; the owner remains responsible for generating, reviewing, and applying teardown and recreation plans.
