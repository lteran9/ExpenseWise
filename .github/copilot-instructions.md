# ExpenseWise Copilot Instructions

ExpenseWise is a .NET 10 solution for tracking and splitting expenses and balances for groups. It is structured around Clean Architecture with CQRS, MediatR, and FluentValidation.

## Agent Workflow and Human Approval

- Before changing files, inspect the current branch and working tree. Preserve unrelated and uncommitted human changes.
- Implement each [roadmap package](../docs/product/roadmap.md) on its own feature branch, created before package changes begin (for example, `feature/phase-1.1-docker-foundation`). Reuse that branch when resuming the same package; do not combine separate packages on one branch.
- Make changes outside roadmap packages on a dedicated feature branch as well. If the starting branch or uncommitted changes make the branch base unclear, ask the human before proceeding.
- Never commit directly to `main` or `master` unless the human explicitly authorizes that action. Do not merge into or push to either branch without explicit authorization.
- Before every commit, summarize the intended staged changes and present the exact proposed commit message, including any trailers, for human approval. Wait for explicit approval before running the commit. A request to implement a package or to commit changes is not approval of an unseen message.
- If the proposed message or the intended commit scope changes after approval, obtain approval again. Do not amend an existing commit without explicit human authorization and approval of the resulting message.
- When approval has not been given, leave the changes uncommitted and report the branch, validation results, and remaining work.

## Engineering Principles

- Follow Clean Architecture: dependencies point inward, business rules belong in Core/Application, infrastructure implements Application ports, and the composition layer wires implementations. Keep controllers and views thin and framework/database details out of Core/Application.
- Apply SOLID pragmatically: give components focused responsibilities, depend on appropriate abstractions at boundaries, and keep contracts cohesive and substitutable. Do not add an interface or abstraction without a concrete need.
- Apply DRY to shared knowledge and business rules: look for existing helpers and patterns before adding logic. Extract shared behavior when it prevents meaningful duplication, not merely because code looks similar.
- Apply KISS: choose the simplest complete implementation that meets the accepted requirements. Avoid speculative extensibility, unnecessary layers, and unrelated refactoring.
- Preserve intended behavior, add tests for changed behavior and failure paths, and verify package acceptance criteria rather than treating a successful build as proof of completion.
- Read applicable file-specific instructions and relevant [architecture decisions](../docs/architecture/decisions/README.md) before editing. Update directly affected documentation; propose a new ADR rather than silently replacing an accepted architectural decision.

## Architecture & Project Boundaries

Dependencies must point strictly inward:

1. **[Core/Entites/](Core/Entites)** (`Core.Entities`): Domain models and entities (e.g., `Expense`, `Group`, `User`, `Split`). Zero external dependencies.
2. **[Application/UseCases/](Application/UseCases)** (`Application.UseCases`): Business logic, use cases, CQRS commands/queries, FluentValidation validators, and port interfaces (`Application.UseCases.Ports`). Depends only on `Core`.
3. **[Infrastructure/](Infrastructure)**:
   - `Infrastructure/SqlDatabase`: EF Core MySQL context (`CoreContext`), Dapper queries, repository implementations, and AutoMapper profiles.
   - `Infrastructure/OpenAPI`: OpenAPI specification / generated contract components.
4. **[Configuration/DependencyConfiguration/](Configuration/DependencyConfiguration)**: Service registration and dependency injection wiring.
5. **[Presentation/](Presentation)**:
   - `Presentation/Api`: ASP.NET Core Web API controllers dispatching requests via MediatR (`IMediator`).
   - `Presentation/UI`: ASP.NET Core MVC UI application with Razor views and Bootstrap.
6. **[Tests/](Tests)**:
   - `Tests/Regression`: Unit and regression tests for UseCases and FluentValidation validators.
   - `Tests/Infrastructure`: Data access, repository, and AutoMapper mapping tests.

## Technology Stack

- **Framework**: .NET 10.0 (C# 13+) with `<Nullable>enable</Nullable>`.
- **Mediation**: MediatR CQRS handlers inheriting from `BaseRequestHandler<TRequest, TResponse>`.
- **Validation**: FluentValidation with `AbstractValidator<TRequest>`.
- **Data Access**: Entity Framework Core with MySQL (`Pomelo.EntityFrameworkCore.MySql`) and Dapper.
- **Mapping**: AutoMapper for entity/DTO mapping.
- **Testing**: xUnit, AutoFixture (`AutoFixture.Xunit2`), and Moq (`AutoMoq`).

## Build and Test Commands

Always verify changes using:

```bash
# Build the solution
dotnet build ExpenseWise.sln

# Run all test suites without rebuilding
dotnet test --no-build
```

## Key Conventions

### CQRS & Use Cases
- Every usecase in `Application/UseCases` implements `BaseRequestHandler<TRequest, TResponse>`.
- Requests implement `IRequest<ResponseWrapper<TResponse>>`.
- Always return a `ResponseWrapper<T>` using helper methods:
  - `Successful(response)` for success.
  - `Invalid(messages)` or `Invalid(string)` for validation/domain errors.
  - `Failed(default)` for execution failures.
- Instantiate or inject the corresponding `AbstractValidator<TRequest>` and validate early in `Handle(...)`.

### Controllers
- Controllers reside in `Presentation/Api/Controllers/`.
- Keep controllers thin: validate route parameters and dispatch to `IMediator.Send(request)`.
- Always annotate actions with `[ProducesResponseType]` for documentation.
- Handle exceptions and wrap responses consistently using `ResponseWrapper<T>`.

### Testing
- Place usecase logic tests in `Tests/Regression/UseCases/`.
- Place validator tests in `Tests/Regression/Validators/`.
- Use `[Theory]` with `[AutoMoq]` for injecting mocks and SUT (System Under Test).
- Follow the Arrange-Act-Assert (AAA) pattern and naming format `MethodName_Condition_ExpectedResult`.
- Pass `CancellationToken.None` in unit tests when calling `Handle(...)`.
