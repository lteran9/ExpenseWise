---
description: "Scaffold a complete CQRS use case vertical slice including Request, Response, Handler, and FluentValidation validator."
argument-hint: "Name of the use case (e.g. SettleUpExpense, ArchiveGroup)"
agent: "agent"
---

Scaffold a complete new CQRS use case vertical slice for ExpenseWise named `${input}`.

Follow the repository's conventions:

1. **Handler & Models in `Application/UseCases/${input}.cs`**:
   - Class `${input}` inherits from `BaseRequestHandler<${input}Request, ${input}Response>`.
   - Inject repository ports from `Application.UseCases.Ports`.
   - Instantiate the validator `${input}RequestValidator`.
   - Request class `${input}Request` implementing `IRequest<ResponseWrapper<${input}Response>>`.
   - Response class `${input}Response`.
   - Use `Successful(...)`, `Invalid(...)`, or `Failed(...)` methods of `BaseRequestHandler`.

2. **Validator in `Application/UseCases/FluentValidation/${input}RequestValidator.cs`**:
   - Class `${input}RequestValidator` inherits from `AbstractValidator<${input}Request>`.
   - Add validation rules for required fields, lengths, or numeric ranges.

3. **Verify compilation**:
   - Run `dotnet build ExpenseWise.sln` to ensure all projects build cleanly.
