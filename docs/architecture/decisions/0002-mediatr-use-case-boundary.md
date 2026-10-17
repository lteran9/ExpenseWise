# ADR 0002: MediatR use-case boundary

- Status: Accepted
- Date: 2026-10-15

## Context

Application operations are represented as request types and handled through MediatR. The API and UI register the use-case handlers, while API controllers dispatch requests rather than invoking persistence implementations directly.

## Decision

Represent each application operation as a request and handler in `src/Application/UseCases`. Handlers follow the existing `BaseRequestHandler<TRequest, TResponse>` and `ResponseWrapper<T>` conventions. Validate input early with FluentValidation where a validator exists, and express persistence requirements through application ports.

The wrapper distinguishes successful results, validation messages, and execution failures. Presentation layers translate these outcomes to transport-specific responses; they should not duplicate business logic.

## Consequences

- Keep controllers thin and send request objects via `IMediator`.
- Keep validation close to the use case and add regression tests for behavioral changes.
- Preserve wrapper semantics when changing a handler; inspect the calling controller because HTTP error mapping varies.
- Do not assume every request currently has a validator or that every controller maps errors identically; check the implementation.
