# ADR 0004: Signed bearer tokens for API identity

- Status: Accepted
- Date: 2026-10-17

## Context

The API serves protected group operations used by both the MVC UI and direct HTTP clients. A caller-supplied user key cannot establish identity. The browser UI must call the API without exposing an API credential in the rendered page or trusting identity fields from forms.

## Decision

- The API accepts signed bearer tokens and derives the acting account from the validated token subject.
- Successful API authentication issues a 20-minute HMAC-SHA256 token signed with the required `Authentication:SigningKey` configuration value. The signing key must contain at least 32 UTF-8 bytes and must not be committed.
- The MVC UI stores the token in its server-side session and separately uses an HTTP-only authentication cookie for protected UI routes. It forwards the token on its server-to-server API requests.
- Protected group handlers enforce membership for reads and organizer ownership for group writes. API controllers replace request actor fields with the validated token subject.
- Registration and authentication remain public API operations. Other user and group operations require bearer authentication; account deletion is rejected in Phase 1.

## Consequences

- Compose and direct API startup require a private signing key; all API instances validating the same token must share the same key.
- Tokens expire after 20 minutes, after which a user signs in again. Persistent login and token refresh are out of scope.
- Direct clients must send the bearer token. Browser routes retain antiforgery validation for state-changing forms.
- This local/trusted deployment design is not a public-deployment security claim.
