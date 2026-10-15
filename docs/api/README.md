    # API

The HTTP API is the `Presentation/Api` ASP.NET Core application. Controllers are the transport boundary: they bind HTTP input, dispatch request objects through MediatR, and translate use-case results into HTTP responses.

## Current routes

Controller routes use the controller name followed by the action name (for example, `GET /User/Find`).

| Controller | Actions |
| --- | --- |
| `HealthController` | `GET /Health` |
| `UserController` | `GET /User/Find`, `POST /User/Create`, `POST /User/Authenticate`, `PUT /User/Update`, `DELETE /User/Delete` |
| `GroupController` | `GET /Group/Retrieve`, `GET /Group/List`, `POST /Group/Create`, `POST /Group/Update`, `POST /Group/AddMember`, `DELETE /Group/Delete` |
| `ExpenseController` | `POST /Expense/Create`, `POST /Expense/Split`, `POST /Expense/Pay`, `PUT /Expense/Update` |

Use the controller and contract types as the source of truth for binding details and response schemas. The API's OpenAPI document and Swagger UI are mapped in the Development environment.

## Request flow

1. ASP.NET Core binds the route, query, or body input to a request type.
2. The controller sends the request through `IMediator`.
3. A use case validates and handles the request, returning a `ResponseWrapper<T>`.
4. The controller maps a successful result to an HTTP success response and validation failures to a client error response.

Error translation is not fully uniform across the controllers. Inspect the relevant action before relying on a particular response envelope or error detail.

## Change guidance

- Keep controllers thin; put business rules in `Application/UseCases`.
- Keep HTTP contracts and status-code metadata aligned with the action's actual behavior.
- When adding or changing a route, update this route summary and verify the generated OpenAPI output in Development.
- Do not document real user data, credentials, internal hostnames, or private deployment URLs.
