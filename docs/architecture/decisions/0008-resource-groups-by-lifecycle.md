# ADR 0008: Resource groups by lifecycle category

- Status: Accepted
- Date: 2026-11-13
- Amends: the single application resource group and resource-group budget in the [Azure deployment specification](../../product/azure-deployment.md). Related: [ADR 0005](0005-azure-container-apps-hosting.md) and [ADR 0007](0007-dedicated-expensewise-terraform-state.md).

## Context

The accepted Azure deployment specification placed every demo resource in one `rg-expensewise-dev` resource group and attached the monthly cost budget to that group. The deployment was restarted in a new subscription dedicated to ExpenseWise before any application resources were created, so the layout can change without moving resources or state.

The owner observed that, in a dedicated subscription, one group containing everything adds little beyond the subscription view, and that the virtual network and the Container Apps do not belong in the same group. Resource groups are Azure's lifecycle, role-assignment, lock, and cost-reporting boundaries; resources with different lifecycles and owners benefit from separate groups.

## Decision

- Create four application resource groups in `westus3`, named `rg-expensewise-<category>-<environment>`:

  | Group | Contents | Lifecycle |
  | --- | --- | --- |
  | `rg-expensewise-network-dev` | Virtual network, both delegated subnets, the MySQL private DNS zone and its virtual network link | Changes rarely; other groups depend on it. |
  | `rg-expensewise-data-dev` | MySQL Flexible Server, the ExpenseWise database, and server configuration | Holds persistent data; must survive application redeployment. |
  | `rg-expensewise-shared-dev` | Container Registry, Key Vault and its secrets, Log Analytics workspace | Shared by the workloads; holds images, credentials, and logs that must survive application redeployment. |
  | `rg-expensewise-apps-dev` | Container Apps environment, UI and API apps, manual jobs, and workload managed identities | Changes with each release; can be recreated without touching data or network. |

- Keep the dedicated `rg-expensewise-tfstate` group from ADR 0007 unchanged. The Azure-managed `me_expensewise_dev` infrastructure group created by the Container Apps environment remains owned by the platform.
- Manage all four groups from the existing single Terraform root and state. Cross-group dependencies use resource IDs; no resource is shared through name lookups.
- Replace the resource-group budget with one subscription-scoped monthly budget with the same amount and 80% and 100% actual-cost alerts. Because the subscription is dedicated to ExpenseWise, it covers the four application groups, the state group, and the managed infrastructure group.
- Scope the package 3.3 deployment identity to the apps group plus the registry permissions needed to build and import images. It receives no role on the network or data groups or on other shared resources.

## Consequences

- Application resources can be replaced without affecting the database, secrets, registry, or network. Use Terraform for these changes; deleting a group outside Terraform leaves the state inconsistent.
- The deployment pipeline's privileges are narrower than a role over every resource. It remains a database-administrator trust boundary, as described in ADR 0005, because it can change jobs that read the administrator password.
- Cost Management can report network, data, shared, and application costs separately by grouping on resource group.
- Operators must target the correct group. Terraform exposes `resource_group_names` by category and `apps_resource_group_name` for the release scripts.
- The subscription budget alerts on all spend in the subscription. If other workloads are added to the subscription later, the budget must be filtered or rescoped.
- No resource locks are added to the data group, so `terraform destroy` teardown remains possible. Locking it is a separate, later decision.
