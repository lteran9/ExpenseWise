# ADR 0005: Azure Container Apps hosting for the demo environment

- Status: Accepted
- Date: 2026-10-27

## Context

ExpenseWise runs locally as two ASP.NET Core containers (API and MVC UI), a one-shot migration container, and MySQL 8.0, defined in Docker Compose. The owner wants a single public `dev` environment on Azure as a portfolio demo. It must be inexpensive, keep the database off the internet, and reuse the existing Dockerfiles and migration command rather than introduce a second build or migration path. The [Phase 3 specification](../../product/azure-deployment.md) records the owner's requirements.

Options considered:

- **Azure App Service (Web App for Containers):** mature, but private database access needs VNet integration on a paid plan, and one-shot migration jobs and an internal-only API need extra apps or workarounds.
- **Azure Kubernetes Service:** supports everything, but adds a cluster to operate and costs more than a demo justifies.
- **Azure Container Apps:** runs the existing images, supports external and internal ingress in the same environment, manual jobs for migrations, managed identity for registry and Key Vault access, and VNet integration on the Consumption workload profile.

For the database, Azure Database for MySQL Flexible Server keeps the MySQL 8.0 provider and migrations described in [ADR 0003](0003-efcore-mysql-persistence.md) unchanged. Its Burstable tier is the lowest-cost managed option with private VNet access and automated backups.

## Decision

- Host the UI and API as Azure Container Apps in a new, VNet-integrated Container Apps environment dedicated to ExpenseWise, using the existing Dockerfiles.
- Expose only the UI publicly, over HTTPS with the Azure-generated hostname. Give the API internal ingress only; the UI calls it within the environment.
- Run the existing `--migrate` command as a Container Apps manual job before each application update. Create the least-privileged application database user with a separate idempotent job that runs a committed SQL script in a pinned MySQL client image.
- Use Azure Database for MySQL Flexible Server (Burstable B1ms, MySQL 8.0) with private access through a delegated subnet and private DNS zone, and require TLS.
- Store images in Azure Container Registry (Basic) and secrets in Azure Key Vault. Give each app and job its own user-assigned managed identity with `AcrPull` and read access only to the Key Vault secrets it needs; the public UI can read none. Send container logs to a Log Analytics workspace.
- Run exactly one UI replica and one API replica, keeping the existing in-memory session and data-protection keys.

## Consequences

- The application images, configuration keys, and migration path stay the same as in Compose. Package 3.1 must make the images safe to run in Production mode behind a TLS-terminating proxy.
- Because the API is internal, external tests cannot call it directly. Browser tests can run against Azure; direct API checks run against Compose.
- Treat the MySQL networking mode as fixed at creation. Moving away from private VNet access later is, at best, an offline, one-way conversion with downtime.
- Anyone who can start or change the administrator jobs can run arbitrary code with administrator database credentials, so the deployment identity is a database-administrator trust boundary.
- UI restarts and deployments sign users out. Scaling the UI beyond one replica needs shared session and data-protection key storage and a new decision.
- Key Vault and Container Registry keep public endpoints protected by Microsoft Entra ID and Azure RBAC; private endpoints are deferred for cost.
- This decision applies to the demo environment only. It is not a production hosting decision.
