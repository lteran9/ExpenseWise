# ADR 0001: Layered architecture with inward dependencies

- Status: Accepted
- Date: 2026-10-15

## Context

ExpenseWise separates domain entities, application use cases, persistence, composition, and two presentation applications into solution projects. Keeping business behavior independent of web and database frameworks supports isolated use-case testing and allows infrastructure or presentation details to evolve without moving business rules into those layers.

## Decision

Preserve the existing dependency direction:

- Core contains framework-independent domain concepts.
- Application depends on Core and declares use-case and persistence-port contracts.
- Infrastructure implements application ports and depends inward.
- Presentation dispatches work through application interfaces rather than implementing business rules.
- The dependency-configuration project is the composition point for binding ports to adapters.

Dependencies must not point from Core or Application toward outer layers.

## Consequences

- New business operations belong in Application and domain rules in Core, not in controllers or repositories.
- Infrastructure-specific types should not leak into Core or application contracts.
- New implementations must be registered in the composition layer.
- Project references and registrations should be reviewed when adding a project or moving a responsibility.
