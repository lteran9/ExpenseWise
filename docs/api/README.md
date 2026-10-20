    # API

The HTTP API is the `src/Presentation/Api` ASP.NET Core application. Controllers are the transport boundary: they bind HTTP input, dispatch request objects through MediatR, and translate use-case results into HTTP responses.

## Current routes

Controller routes use the controller name followed by the action name (for example, `GET /User/Find`).

| Controller | Actions |
| --- | --- |
| `HealthController` | `GET /Health` (public) |
| `UserController` | `POST /User/Create` and `POST /User/Authenticate` (public); `GET /User/Find` and `PUT /User/Update` (bearer required); `DELETE /User/Delete` (bearer required, always rejected with `409 Conflict` because account deletion is not supported in Phase 1) |
| `GroupController` | All actions require a bearer token. `GET /Group/List` and `GET /Group/Retrieve` are scoped to the authenticated member and include expensed and outstanding totals; `POST /Group/Create`, `POST /Group/Update`, `POST /Group/AddMember`, and `DELETE /Group/Delete` (archive) require the organizer for existing groups. |
| `ExpenseController` | All actions require a bearer token. `GET /Expense/List?groupKey=...` and `GET /Expense/Detail?expenseKey=...` are available to group members; `POST /Expense/Create` (equal split among selected members, idempotent by client expense key) and `POST /Expense/Void` are organizer-only. Expenses cannot be edited; a voided expense is excluded from balances, and voiding is rejected once the group has repayments. |
| `RepaymentController` | All actions require a bearer token. `GET /Repayment/Ledger?groupKey=...` returns member balances, the outstanding total, and repayment history to group members; `POST /Repayment/Record` is organizer-only, idempotent by client repayment key, and rejects self-repayments and amounts above the sender's current debt or the recipient's current credit. |

`POST /User/Authenticate` returns a signed 20-minute bearer token. Send it as `Authorization: Bearer <token>` for protected routes. A request-supplied user key is not an identity: the server derives the actor from the validated token. The token signing key is configured with `Authentication__SigningKey`; see [local operations](../operations/README.md).

Use the controller and contract types as the source of truth for binding details and response schemas. The API's OpenAPI document and Swagger UI are mapped in the Development environment.

## Request flow

1. ASP.NET Core binds the route, query, or body input to a request type.
2. The controller sends the request through `IMediator`.
3. A use case validates and handles the request, returning a `ResponseWrapper<T>`.
4. The controller maps a successful result to an HTTP success response and validation failures to a client error response.

Validation error translation is not fully uniform across the controllers. Inspect the relevant action before relying on a particular response envelope.

## Change guidance

- Keep controllers thin; put business rules in `src/Application/UseCases`.
- Keep HTTP contracts and status-code metadata aligned with the action's actual behavior.
- When adding or changing a route, update this route summary and verify the generated OpenAPI output in Development.
- The UI calls use cases only through the generated OpenAPI client; keep UI project references limited to the OpenAPI client and UI infrastructure.
- Do not document real user data, credentials, internal hostnames, or private deployment URLs.
