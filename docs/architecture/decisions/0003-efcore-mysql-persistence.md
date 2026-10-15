# ADR 0003: Relational persistence with EF Core and MySQL

- Status: Accepted
- Date: 2026-10-15

## Context

The SQL database infrastructure project contains an EF Core `DbContext`, relational entity mappings, and database migrations. The application defines repository ports, with SQL infrastructure providing their implementations.

## Decision

Keep relational persistence in `Infrastructure/SqlDatabase`, behind ports declared in `Application/UseCases`. Use the existing EF Core/MySQL infrastructure and migration approach for changes to the relational model. Keep persistence entities and provider details out of domain and application contracts.

## Consequences

- Schema changes should be reflected in the infrastructure model and migrations, with appropriate infrastructure tests.
- Use cases should depend on application ports, not a `DbContext` or provider-specific API.
- Review configuration and migration behavior before changing database setup; do not add credentials or real connection strings to source or documentation.
- Verify the actual provider version and configuration in project files before relying on version-specific APIs.
