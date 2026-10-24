    # API

The HTTP API is the `src/Presentation/Api` ASP.NET Core application. Controllers are the transport boundary: they bind HTTP input, dispatch request objects through MediatR, and translate use-case results into HTTP responses.

## Current routes

Controller routes use the controller name followed by the action name (for example, `GET /User/Find`).

| Controller | Actions |
| --- | --- |
| `HealthController` | `GET /Health` (public) |
| `UserController` | `POST /User/Create` and `POST /User/Authenticate` (public); `GET /User/Find` and `PUT /User/Update` (bearer required); `DELETE /User/Delete` (bearer required, currently rejected) |
| `GroupController` | All actions require a bearer token. List scope comes from the authenticated user; reads require membership; group updates, membership changes, and archival (`DELETE /Group/Delete`, a soft archive that keeps ledger rows) require the organizer. Archived groups are not returned by reads in Phase 1. |
| `ExpenseController` | All actions require a bearer token. `GET /Expense/List?groupKey=...` and `GET /Expense/Detail` return expenses and their persisted shares to group members. `POST /Expense/Create` and `POST /Expense/Void` are organizer-only; voiding is rejected once the group has any repayment. `PUT /Expense/Update` is always rejected with "Expense editing is not available yet." |
| `RepaymentController` | All actions require a bearer token. `GET /Repayment/Ledger?groupKey=...` returns balances, suggested repayments, and repayment history to group members. `POST /Repayment/Record` is organizer-only and idempotent per `idempotencyKey`; self-repayments, overpayments, and non-USD amounts are rejected without writing. |

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
