# Delivery roadmap

Created: 2026-10-16. A roadmap entry is not completed merely by writing this plan; record verification evidence when its exit gate passes.

The [product specification](README.md) defines the target. The [assessment](current-state.md) explains why the work is needed. Existing handlers/routes are reusable inputs, not evidence that a work package is done.

## Phase 1: A usable local Docker expense-sharing release

Deliver a complete vertical journey with accounts, organizer-managed groups, USD equal splits among selected members, reconciling balances, and manually recorded repayments. No real payment processing or public deployment.

Implement one numbered package at a time and attach evidence to its exit gate. Resolve proposed detailed rules before coding the package they affect. Do not estimate dates until the initial Docker/schema findings are resolved.

| Package | Depends on | Deliverable | Exit gate |
| --- | --- | --- | --- |
| 1.1 Reproducible Docker foundation | None | Inject DB configuration through the composition layer and existing repository/context wiring; use service DNS; fail clearly on missing values; wait for DB readiness; reconcile model/migration drift; add a repeatable migration step; retain persistent storage. Document local configuration and start/stop/logs. | Fresh-volume Compose startup creates a usable schema without manual SQL; both services are reachable; an account can be persisted and retrieved; restart retains it; existing data has a tested migration path. |
| 1.2 Identity, groups, and one integration path | 1.1 | Establish server-validated identity and organizer/member permissions; protect direct API reads/writes; connect account/group UI through the API; handle empty lists and duplicate membership; preserve validation and antiforgery; add useful navigation. Make group creation plus owner membership atomic. | Register three accounts; organizer creates a group and adds the others; each sees only their groups; nonmembers cannot access it and ordinary members cannot perform organizer writes, including direct HTTP requests. |
| 1.3 Expense ledger and exact equal allocations | 1.2 | Separate payer from recording actor; persist selected participants and exact cent allocations; implement atomic, retry-safe expense creation; add expense list/detail contracts and UI; fixed USD input/display; void/re-entry before repayments; consistent group summaries. Regenerate the API client. | Browser supports one payer and a selected subset; scenario B and expense/access/void portions of scenario C pass; expense/share totals reconcile on real MySQL; retries do not duplicate bills. |
| 1.4 Balances and manual repayments | 1.3 | Replace paid-split arithmetic with payer/share/repayment accounting; add deterministic who-owes-whom suggestions; implement explicit full/partial repayments, history, confirmation, atomic validation, and duplicate/concurrent-request protection. Retire the old settle-all behavior. | Scenario A passes with exact balances at every step; invalid/self/over/concurrent repayments are rejected without partial writes; repeat submissions do not change balances/history twice. |
| 1.5 Release proof and operating guide | 1.1, 1.2, 1.3, 1.4 | Complete Docker smoke and browser coverage; real-MySQL migration/constraint checks; failure-state checks; backup/restore rehearsal; remove misleading feature claims and dead/placeholder actions; update setup, API, and product docs. | All Phase 1 acceptance scenarios pass; project owner completes the main journey from the runbook unaided; the release gate below has evidence. |

### Package 1.1 verification (2026-10-16)

- Started the Compose stack with a new isolated project and volume. MySQL became healthy, the initial migration created the schema, and the migration service exited successfully.
- Verified the API OpenAPI endpoint and UI root returned HTTP 200. Created an account through the API, retrieved it, restarted MySQL and both applications, and retrieved the same account afterward.
- Re-ran the migration against the populated database; EF reported that it was already up to date. Confirmed the migration-history entry and legacy `expenses.settled` column remained present.
- `docker compose config --quiet`, `dotnet build ExpenseWise.sln` (0 errors), and `dotnet test --no-build` (94 passed) succeeded. The build reported existing NuGet vulnerability warnings.
- Removed the isolated test containers and volume after verification. The normal Compose named volume remains configured for local use.

### Package 1.2 verification (2026-10-17)

- Built the final API/UI images and started an isolated Compose project with a fresh MySQL volume. MySQL became healthy, the migration service exited successfully, and the API OpenAPI endpoint and UI root returned HTTP 200.
- Exercised the API with four disposable accounts: registration/sign-in, organizer group creation, two member additions, duplicate membership rejection, member-scoped group lists, outsider read denial, and organizer-only write rejection (HTTP 401/403 as expected).
- Exercised the browser UI through registration, cookie-backed sign-in, group creation, invitation, logout, sign-in again, expense creation, expense listing on the split screen, and the legacy split action; observed the corresponding authenticated API calls and persisted group summary.
- Verified the generated OpenAPI group inputs contain no caller-supplied actor/owner identity fields.
- Added and tested required `ActorKey` validation for authenticated application requests; anonymous account registration/sign-in remain actor-free.
- Audited all UI C# and project references: no MediatR/use-case/repository/domain access remains; the UI references the generated OpenAPI client and calls use cases only over HTTP.
- `dotnet build ExpenseWise.sln` succeeded with 0 warnings/errors; `dotnet test --no-build` passed 113 tests (72 Regression, 41 Infrastructure). Removed the isolated Compose containers and test volume afterward.

### Implementation boundaries

- Keep business rules in the Core and Application layers and persistence behind Application ports; wire configuration and infrastructure in the composition layer.
- Add only the domain/storage changes needed for payer, immutable allocations, repayments, and the agreed correction policy. Reuse existing entities where their meaning remains valid; do not preserve the paid-split experiment at the cost of incorrect accounting.
- All Phase 1 UI operations should use API contracts. Update OpenAPI metadata/contracts and regenerate the client, rather than maintaining two application execution paths.
- Use migrations that preserve existing data. Determine whether historical paid splits can be mapped reliably; if not, require an explicit owner-approved reconciliation plan. Never silently invent repayment recipients or wipe a database.
- Keep legacy/unimplemented operations unavailable or explicitly rejected until supported. Do not leave `NotImplementedException` behind advertised release actions.
- Add tests with each package, not just at the final gate. Keep existing tests passing; intentionally changed behavior needs updated expectations.
- No framework/provider replacement unless a reproduced compatibility issue justifies it.

### Phase 1 release gate

The checklist is initially unchecked intentionally. Link test output, release/commit references, or a dated manual verification record when each item passes. A planned feature or passing unit test alone does not satisfy a Docker/browser gate.

- [ ] Detailed rules for each delivered package have been reviewed; no unresolved decision affects balances or permissions.
- [ ] A clean documented Compose install works without manual SQL or source edits.
- [ ] Migration from the supported existing database state is tested and data-preserving.
- [ ] All UI Phase 1 operations go through the API; client/contracts match the server.
- [ ] Registration, sign-in/out, groups, membership, and organizer/member permissions work.
- [ ] Exact USD equal splits among selected people are persisted and remain stable.
- [ ] Per-member balances, repayment suggestions, partial/full repayments, and history reconcile.
- [ ] Duplicate/concurrent requests cannot double count or overpay.
- [ ] Correction restrictions and unavailable features are explicit in the UI.
- [ ] Specification scenarios A-D pass, including the Docker-hosted browser journey.
- [ ] Real-MySQL tests verify migrations, foreign keys, unique constraints, decimal persistence, and atomic writes.
- [ ] Existing regression/infrastructure tests and solution build pass.
- [ ] CI runs the necessary automated checks, including the Docker acceptance checks required for release.
- [ ] Data survives restart; a backup has been restored and reconciled in a separate disposable environment.
- [ ] Configuration errors and API/database outages produce useful errors/logs without false success.
- [ ] Setup, recovery, limitations, and API documentation describe the delivered behavior; UI claims match it.
- [ ] A versioned release or recorded release commit identifies the verified result and its known limitations.
- [ ] The project owner completes scenario A unaided and accepts the release.

When these gates pass, stop and call Phase 1 done. Do not expand it to accommodate every unused handler or marketing claim.

## Phase 2: Optional convenience and richer splitting

Start only after Phase 1 is accepted. Select features based on actual usage; this is an option list, not a promise or prerequisite for completion.

- Exact custom amounts, then percentages/weighted shares, with independent rounding rules and tests.
- Guest participants without accounts and better invitation/onboarding flows.
- Expense editing, post-repayment corrections, repayment reversals, and an explicit audit trail.
- Member departure and group archival workflows that preserve balances/history.
- Password recovery, contact verification, search/history filters, and export.
- Receipt attachments, line items, tax/tip allocation, or reminders only if needed.

Exit gate for any selected feature: written scope, data/migration impact, acceptance examples, end-to-end verification, and updated documentation. Unselected items remain deferred.

## Phase 3: Optional public/self-hosted service

This is a separate deployment product, not an automatic next step.

Before exposing the system publicly, define an operating owner and deployment requirements, then address production authentication/session handling, TLS/reverse proxy, secret management, database least privilege, health/readiness, backups/restore objectives, observability, upgrade/rollback, account recovery, data retention, and an appropriate security review.

Cloud/Azure hosting, mobile clients, multi-currency conversion, and payment-provider integration need separate specs if pursued. They do not belong on the critical path to the agreed local release.

## Maintaining the specification

1. Keep [product scope](README.md) as the source of truth for intended behavior.
2. Record technical decisions in [ADRs](../architecture/decisions/README.md) when implementation changes an architectural choice.
3. For each work package, create an issue or change request with its scope, prerequisites, acceptance scenarios, and evidence. Issue creation is not part of this documentation change.
4. Update this roadmap when work is accepted, not merely when coding starts.
5. Treat new ideas as deferred by default. Add them to Phase 1 only by explicitly changing the specification and acknowledging the added scope.

**Next action:** Review and accept the Phase 1.1 verification, then implement package 1.2 on its own feature branch. Do not wire more settlement screens to the current split handler first.
