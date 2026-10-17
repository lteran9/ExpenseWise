---
description: "Use when creating, editing, or refactoring CQRS use cases, MediatR handlers, FluentValidation rules, or port interfaces in src/Application/UseCases."
applyTo: "src/Application/UseCases/**/*.cs"
---

# Use Case Guidelines (src/Application/UseCases)

All application logic in ExpenseWise is encapsulated inside vertical slices within `src/Application/UseCases`.

## 1. Structure of a Use Case

Each use case typically contains the Handler, Request, and Response classes in a single file (or grouped together):

```csharp
namespace Application.UseCases
{
    public class SampleUseCase : BaseRequestHandler<SampleUseCaseRequest, SampleUseCaseResponse>
    {
        private readonly ISampleRepository _repository;
        private readonly AbstractValidator<SampleUseCaseRequest> _validator;

        public SampleUseCase(ISampleRepository repository)
        {
            _repository = repository;
            _validator = new SampleUseCaseRequestValidator();
        }

        public override async Task<ResponseWrapper<SampleUseCaseResponse>> Handle(
            SampleUseCaseRequest request,
            CancellationToken cancellationToken)
        {
            var validationResult = await _validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                return Invalid(validationResult.Errors.Select(e => e.ErrorMessage).ToList());
            }

            // Domain logic & repository calls
            var entity = await _repository.FindByKeyAsync(request.Key);
            if (entity == null)
            {
                return Invalid("Entity not found for the provided key.");
            }

            return Successful(new SampleUseCaseResponse { Succeeded = true });
        }
    }

    public class SampleUseCaseRequest : IRequest<ResponseWrapper<SampleUseCaseResponse>>
    {
        public Guid Key { get; set; }
    }

    public class SampleUseCaseResponse
    {
        public bool Succeeded { get; set; }
    }
}
```

## 2. Validation Conventions

- Define validators in `src/Application/UseCases/FluentValidation/` inheriting from `AbstractValidator<TRequest>`.
- Keep validation pure (string checks, range checks, format checks); do not execute database calls in FluentValidation rules.
- Check database entity existence inside the Handler and return `Invalid("descriptive message")`.

## 3. Ports & Boundaries

- Define repository and external communication interfaces in `src/Application/UseCases/Ports/` (e.g., `IExpenseRepository.cs`, `IGroupRepository.cs`).
- Never reference Entity Framework Core (`CoreContext`), Dapper, or presentation types inside `Application.UseCases`.
- Rely solely on domain entities in `Core.Entities`.
