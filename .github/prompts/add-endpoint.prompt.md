---
description: "Add a new API controller endpoint connected to a MediatR use case in Presentation/Api."
argument-hint: "Endpoint details (e.g. SettleUp in ExpenseController)"
agent: "agent"
---

Add a new API action endpoint for ExpenseWise according to the specified request: `${input}`.

Follow the controller conventions:
1. Locate the appropriate controller in `Presentation/Api/Controllers/`.
2. Follow the standard action structure:
   - Route and HTTP verb attributes (e.g. `[HttpPost(nameof(ActionName))]`).
   - OpenAPI metadata: `[ProducesResponseType(typeof(ResponseDto), StatusCodes.Status200OK)]`, `[ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]`, `[ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]`.
   - Send the request to MediatR via `await _mediator.Send(request)`.
   - Return `Ok(response.Result)` when `response.Succeeded` is true, otherwise return `BadRequest(response)`.
   - Wrap the execution in a `try/catch` returning `Problem(detail: ex.Message)`.
3. Verify compilation by building the project with `dotnet build ExpenseWise.sln`.
