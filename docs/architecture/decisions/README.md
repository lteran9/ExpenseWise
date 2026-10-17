# Architecture decision records

These ADRs capture decisions visible in the current repository so that contributors and coding agents can understand their intent and trade-offs. They do not override the code or guarantee production readiness.

| ADR | Decision | Status |
| --- | --- | --- |
| [0004: Signed bearer tokens for API identity](0004-signed-api-bearer-tokens.md) | Derive protected API actor identity from short-lived signed tokens. | Accepted |
| [0001: Layered architecture with inward dependencies](0001-layered-architecture.md) | Separate domain, application, infrastructure, composition, and presentation concerns. | Accepted |
| [0002: MediatR use-case boundary](0002-mediatr-use-case-boundary.md) | Represent application operations as request/handler pairs with validation and wrapped results. | Accepted |
| [0003: Relational persistence with EF Core and MySQL](0003-efcore-mysql-persistence.md) | Keep SQL access behind application ports and implement it in infrastructure. | Accepted |

## Writing or changing a decision

Create a new, sequentially numbered ADR when a material design choice is made. Include status, context, decision, and consequences. Mark a superseded decision as superseded and link its replacement; do not rewrite history to make a newer decision appear original.

Only record rationale supported by project context or an explicit design discussion. If rationale is not known, state that the ADR documents the current implementation rather than inventing historical intent.
