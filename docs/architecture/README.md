# Architecture

ExpenseWise is a .NET 10 solution organized around inward dependencies and separated presentation, application, domain, and infrastructure concerns.

```mermaid
flowchart LR
    UI["src/Presentation/UI"] --> DI["src/Configuration/DependencyConfiguration"]
    API["src/Presentation/Api"] --> DI
    DI --> App["src/Application/UseCases"]
    DI --> Infra["src/Infrastructure/SqlDatabase"]
    App --> Core["src/Core/Entites"]
    Infra --> Core
    Infra --> App
    UI --> Client["src/Infrastructure/OpenAPI"]
    Client -->|HTTP| API
```

`src/Configuration/DependencyConfiguration` is the composition layer that wires use cases and persistence adapters. The UI calls the API through the generated OpenAPI client; both application entry points register application use cases.

## Project responsibilities

| Project | Responsibility |
| --- | --- |
| `src/Core/Entites` | Domain entities and value objects; keep domain types independent of frameworks and infrastructure. |
| `src/Application/UseCases` | MediatR requests/handlers, validation, response wrappers, and repository ports. |
| `src/Infrastructure/SqlDatabase` | SQL persistence adapters, EF Core context/migrations, queries, and entity mappings. |
| `src/Infrastructure/OpenAPI` | Generated API client and its configuration. |
| `src/Configuration/DependencyConfiguration` | Dependency-injection composition and implementation bindings. |
| `src/Presentation/Api` | HTTP controllers, API contracts, and API startup. |
| `src/Presentation/UI` | MVC application, views, and API-client integration. |
| `Tests` | Regression/use-case and infrastructure test suites. |

## Change guidance

- Dependencies point inward: Core should not depend on Application, Infrastructure, or Presentation.
- Application declares ports; Infrastructure implements them. Wire implementations in the composition layer rather than from use cases.
- Keep port interfaces in `Application/UseCases/Ports` and shared application supporting types at the `Application/UseCases` root. `GroupMembershipConflictException` is a framework-independent signal from a repository's write-time membership/history check; `CreateExpense` and `DeleteMember` translate it into a validation response when state changes after their initial checks. Unexpected persistence failures are not classified as membership conflicts.
- Keep controllers and views focused on presentation concerns. Put business decisions in use cases and domain concepts.
- Follow the existing request/response and validation conventions described in [ADR 0002](decisions/0002-mediatr-use-case-boundary.md).
- Check [the decision index](decisions/README.md) before changing an established architectural choice.

The diagram and responsibilities describe the current project structure; verify project references and registrations when working on architecture-sensitive changes.
