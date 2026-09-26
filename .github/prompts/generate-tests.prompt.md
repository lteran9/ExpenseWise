---
description: "Generate unit and regression tests for a use case handler or validator using xUnit, AutoFixture, and AutoMoq."
argument-hint: "Target use case or validator (e.g. CreateExpense or CreateExpenseValidator)"
agent: "agent"
---

Generate comprehensive unit and regression tests for `${input}` in the `Tests/Regression` project.

Follow the repository testing patterns:
1. Locate or create the test file under:
   - `Tests/Regression/UseCases/${input}Tests.cs` for MediatR use cases.
   - `Tests/Regression/Validators/${input}ValidatorTests.cs` for FluentValidation validators.
2. Structure tests using:
   - `[Theory]` and `[AutoMoq]` attributes.
   - `[Frozen] Mock<IRepository>` parameters for mocking dependencies when testing use cases.
   - Test method naming: `MethodName_Condition_ExpectedResult`.
   - AAA (Arrange, Act, Assert) sections.
   - Pass `CancellationToken.None` when invoking `Handle(request, CancellationToken.None)`.
3. Include test coverage for:
   - Successful execution paths.
   - Validation failure handling.
   - Missing entity / null repository result conditions.
   - Any boundary or error conditions.
4. Run tests with `dotnet test --no-build` to verify all tests pass.
