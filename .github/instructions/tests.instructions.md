---
description: "Use when creating, editing, or refactoring unit tests, regression tests, integration tests, mocks, or validators in the Tests project."
applyTo: "src/Tests/**/*.cs"
---

# Testing Guidelines (src/Tests/)

The solution uses xUnit, AutoFixture (`AutoFixture.Xunit2`), and Moq (`AutoMoq`) for fast, decoupled unit testing.

## 1. Project Organization

- **`src/Tests/Regression/UseCases/`**: Tests for MediatR use case handlers.
- **`src/Tests/Regression/Validators/`**: Tests for FluentValidation validators.
- **`src/Tests/Infrastructure/`**: Data access and AutoMapper mapping tests.
- **`src/Tests/Acceptance/`**: Playwright browser tests against the Docker Compose stack. These reference no application projects, use `[AcceptanceFact]` so they skip without `EXPENSEWISE_UI_URL`/`EXPENSEWISE_API_URL`, and are grouped by the `Category` trait (`Scenario`, `AfterRestart`). Drive pages through `UserSession`. See the [operations guide](../../docs/operations/README.md#docker-acceptance-tests).

## 2. Test Conventions

- **Naming**: `MethodName_Condition_ExpectedResult` (e.g., `CreateExpense_WithEmptyDescription_ShouldFail`).
- **Structure**: Arrange, Act, Assert (AAA).
- Use `[Theory]` and `[AutoMoq]` to automatically inject dependencies and instantiate the System Under Test (SUT).
- Always use `CancellationToken.None` when calling `Handle(...)` in unit tests.

### Example: Use Case Test

```csharp
[Theory]
[AutoMoq]
public async Task SampleUseCase_WhenEntityNotFound_ShouldReturnInvalid(
    [Frozen] Mock<ISampleRepository> repositoryMock,
    SampleUseCase useCase)
{
    // Arrange
    repositoryMock
        .Setup(x => x.FindByKeyAsync(It.IsAny<Guid>()))
        .ReturnsAsync((SampleEntity?)null);

    var request = new SampleUseCaseRequest { Key = Guid.NewGuid() };

    // Act
    var response = await useCase.Handle(request, CancellationToken.None);

    // Assert
    Assert.False(response.Succeeded);
    Assert.NotNull(response.ValidationMessages);
    Assert.Contains("Entity not found", response.ValidationMessages.First());
}
```

### Example: Validator Test

```csharp
[Theory]
[AutoMoq]
public async Task SampleValidator_WithEmptyField_ShouldHaveValidationError(
    SampleUseCaseRequestValidator validator)
{
    // Arrange
    var request = new SampleUseCaseRequest { Name = string.Empty };

    // Act
    var result = await validator.ValidateAsync(request);

    // Assert
    Assert.False(result.IsValid);
    Assert.Contains(result.Errors, e => e.PropertyName == nameof(SampleUseCaseRequest.Name));
}
```
