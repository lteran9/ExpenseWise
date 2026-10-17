---
description: "Use when creating, editing, or refactoring ASP.NET Core Web API controllers, endpoints, routes, or request/response contracts in src/Presentation/Api."
applyTo: "src/Presentation/Api/**/*.cs"
---

# API Controller Guidelines (src/Presentation/Api)

Controllers in `src/Presentation/Api` are thin presentation adapters that delegate application logic to MediatR handlers.

## 1. Controller Design Rules

- Keep controllers clean: no business logic, EF Core queries, or direct repository access.
- Inject `IMediator` to dispatch commands and queries.
- Controllers can inherit from `Controller` or `ApiController` base class.

## 2. Endpoint Pattern

Follow this pattern when adding or modifying endpoints:

```csharp
[HttpPost(nameof(ActionName))]
[ProducesResponseType(typeof(ActionResponse), StatusCodes.Status200OK)]
[ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
[ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
public async Task<IActionResult> ActionName([FromBody] ActionRequest request)
{
    try
    {
        var response = await _mediator.Send(request);
        if (response.Succeeded)
        {
            return Ok(response.Result);
        }

        return BadRequest(response);
    }
    catch (Exception ex)
    {
        return Problem(detail: ex.Message);
    }
}
```

## 3. OpenAPI / Swagger Annotations

- Annotate each action with `[ProducesResponseType]` for all potential HTTP statuses (200 OK, 400 Bad Request, 500 Internal Server Error).
- Specify explicit response models where applicable (e.g., `typeof(CreateExpenseResponse)`).
- Use `[FromBody]` for POST/PUT payloads containing complex objects and `[FromRoute]` / `[FromQuery]` for simple identifier lookups.
