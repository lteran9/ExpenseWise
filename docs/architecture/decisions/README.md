# Architecture decision records

These ADRs capture decisions visible in the current repository so that contributors and coding agents can understand their intent and trade-offs. They do not override the code or guarantee production readiness.

| ADR | Decision | Status |
| --- | --- | --- |
| [0004: Signed bearer tokens for API identity](0004-signed-api-bearer-tokens.md) | Derive protected API actor identity from short-lived signed tokens. | Accepted |
| [0001: Layered architecture with inward dependencies](0001-layered-architecture.md) | Separate domain, application, infrastructure, composition, and presentation concerns. | Accepted |
| [0002: MediatR use-case boundary](0002-mediatr-use-case-boundary.md) | Represent application operations as request/handler pairs with validation and wrapped results. | Accepted |
| [0003: Relational persistence with EF Core and MySQL](0003-efcore-mysql-persistence.md) | Keep SQL access behind application ports and implement it in infrastructure. | Accepted |
| [0005: Azure Container Apps hosting for the demo environment](0005-azure-container-apps-hosting.md) | Host the UI and an internal-only API on Container Apps with private MySQL Flexible Server. | Accepted |
| [0006: Terraform infrastructure as code with shared remote state](0006-terraform-infrastructure-as-code.md) | Provision Azure with Terraform, storing state in a dedicated container of a shared storage account. | Superseded by ADR 0007 |
| [0007: Dedicated Terraform state storage for ExpenseWise](0007-dedicated-expensewise-terraform-state.md) | Isolate Terraform state in an ExpenseWise-owned resource group and storage account. | Accepted; amended by ADR 0009 |
| [0008: Resource groups by lifecycle category](0008-resource-groups-by-lifecycle.md) | Split the demo into network, data, shared, and apps resource groups with a subscription-scoped budget. | Accepted |
| [0009: Terraform bootstrap and environment roots](0009-terraform-bootstrap-and-environment-roots.md) | Bootstrap state with its own Terraform root, keep per-environment inputs in `infra/azure/environments`, and check Terraform in CI. | Accepted; amended by ADR 0010 |
| [0010: Commit-SHA images and RBAC waits for the first deployment](0010-commit-sha-image-rollout.md) | Grant Terraform's identity Key Vault secret write access, wait for RBAC, and create workloads from commit-tagged images in targeted stages. | Accepted |

## Writing or changing a decision

Create a new, sequentially numbered ADR when a material design choice is made. Include status, context, decision, and consequences. Mark a superseded decision as superseded and link its replacement; do not rewrite history to make a newer decision appear original.

Only record rationale supported by project context or an explicit design discussion. If rationale is not known, state that the ADR documents the current implementation rather than inventing historical intent.
