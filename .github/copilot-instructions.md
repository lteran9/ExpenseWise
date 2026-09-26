# ExpenseWise Copilot Instructions

ExpenseWise is a .NET 10 solution for tracking and splitting expenses and balances for groups. It is structured around Clean Architecture with CQRS, MediatR, and FluentValidation.

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
