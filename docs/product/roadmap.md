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

### Package 1.3 verification (2026-10-19)

- `dotnet build ExpenseWise.sln --no-restore` succeeded with 0 warnings/errors; `dotnet test ExpenseWise.sln --no-build` passed 99 tests (61 Regression, 38 Infrastructure).
- Generated the idempotent MySQL migration script and applied the migration to a fresh, isolated Compose MySQL volume. The migration service exited successfully; the API OpenAPI endpoint and UI root returned HTTP 200.
- In an API smoke journey, registered an organizer and member, created a group, and added the member. Created a USD 10.01 expense with the member as payer and the organizer as recorder; shares were USD 5.01 and USD 5.00. Repeating the same request key returned the existing expense, and voiding retained its shares.
- In a UI HTTP-form smoke journey, signed in, created an expense with a different payer and recorder, confirmed the rendered payer and USD 3.25 shares, then voided it and confirmed the ledger retained the expense and shares. This exercised the antiforgery-protected UI endpoints but was not browser automation.
- The isolated test volume was removed. The migration's pre-1.3 data reset was exercised only on this disposable database.
- These checks are smoke evidence, not completion of the package exit gate: the full scenario B/C browser journey, including a selected-subset browser flow, remains to be verified.

### Package 1.4 verification (2026-10-20)

- `dotnet build ExpenseWise.sln` succeeded with 0 warnings/errors. `dotnet test --no-build` passed the Regression suite (93 tests).
- Infrastructure tests ran against the local MySQL container on a dedicated `expensewise_test` database, with migrations applied: 42 passed, 0 skipped. This includes real-MySQL checks that concurrent same-key expense and repayment requests store one row, and that an overpayment is rejected without writing.
- Applied the additive `AddRepayments` migration with the Compose migrate service. Rebuilt the API and UI images; the UI root returned HTTP 200, and the unauthenticated repayment ledger API request returned HTTP 401.
- Not yet verified: the browser journey for scenario A (including the confirmation dialog and retry behavior), the restart check, and real-MySQL tests for void-versus-repayment locking. These remain open for the exit gate.
- Behavior change from 1.3, confirmed by the spec: voiding is blocked after any repayment exists, and member removal is blocked when the member has expense or repayment history.

### Package 1.5 verification (2026-10-20)

Toolchain: .NET SDK 10.0.401, Docker 28.0.4, Docker Compose 2.38.2, MySQL 8.0.46, Chromium 155.

- **Cleanup.** Removed the unimplemented `PUT /Expense/Update` route and `UpdateExpense` stub, the placeholder forgot-password UI, and commented-out API code; regenerated the OpenAPI document and client. Rewrote the home and privacy pages to describe only the delivered behavior. `GET /Group/Retrieve` now returns the same expensed/outstanding totals as the group list.
- **Defects found and fixed during verification.** In the container, amounts rendered as `¤90.00` because the UI used the invariant culture; the UI now formats with en-US. The groups page reported "not a member of any groups" when the API call failed; it now shows an error instead.
- **Build and tests.** `dotnet build ExpenseWise.sln` succeeded with 0 warnings/errors. `dotnet test --no-build` passed 95 Regression tests and 49 Infrastructure tests with 0 skipped, using a migrated `expensewise_test` database on the Compose MySQL service and the application database user. New real-MySQL tests cover void-after-repayment rejection, a concurrent void-versus-repayment race (exactly one applies; stable over five runs), 0.01 and 9,999,999,999,999,999.99 decimal round-trips, duplicate repayment keys and membership (MySQL 1062), and an unknown repayment sender (MySQL 1452).
- **Fresh install.** `docker compose down -v` followed by `docker compose up --build -d`: MySQL became healthy, the migrate service applied all three migrations and exited 0, and the UI root returned HTTP 200. No manual SQL or source edits were used.
- **HTTP acceptance.** [`scripts/acceptance-api.sh`](../../scripts/acceptance-api.sh) passed all 26 checks against the stack: scenario A steps 1-8 with exact balances, suggestions, totals, and outstanding values at each step (step 8 restarts `mysql` and `webapi`); scenario B allocations, reload stability, fourth member, excluded payer, and single participant; scenario C invalid money and currency, nonmember payer/participant, duplicate or empty participants and duplicate membership (all without writes), anonymous/nonmember/member-write rejection, idempotent expense and repayment retries, voiding before repayments, self-repayment and overpayment rejection, two racing repayments that each settle the same debt (one stored), void-after-repayment rejection, account-deletion rejection, and the removed update route. The new `acceptance` job in [the CI workflow](../../.github/workflows/build.yml) runs this script against a freshly built Compose stack.
- **Browser journey.** A scripted Chromium walkthrough against the Compose UI passed 15 checks: three registrations, group creation and member invitations, scenario A steps 1-8 entirely in the browser (selected-subset expense, balances and suggestions, the repayment confirmation step, resubmitting the same repayment form including its antiforgery token without a second record, settlement to zero, and the same ledger after restarting every Compose service and signing in again), browser rejection of an overpayment and of voiding after repayments, and Bob's member view without organizer actions.
- **Failure states.** With the API stopped, the groups, expenses, and balances pages showed error alerts rather than empty data. With MySQL stopped, the API logged `Unable to connect to any of the specified MySQL hosts`, the UI showed a failed request and "Unable to load balances", and no action reported success. Missing `DB_PASSWORD`/`JWT_SIGNING_KEY` stops `docker compose` with a named-variable error, and a short signing key stops the API with a clear message. Searching all Compose logs for the database password found no occurrences.
- **Backup and restore.** Took a `mysqldump --single-transaction` backup from the running stack, restored it into a disposable Compose project (`expensewise-restore-test`) with its own volume, and the migrate service reported no migrations to apply. `CHECKSUM TABLE` output for all eight tables matched the source, row counts matched (25 users, 5 groups, 15 memberships, 8 expenses, 21 shares, 9 repayments), and API group lists and ledgers for all five groups were identical between the source and restored stacks. The disposable project and volume were removed. The procedure is in the [operations guide](../operations/README.md#backup-and-restore).
- **Documentation.** Updated the [operations guide](../operations/README.md) (acceptance checks, real-MySQL tests, backup/restore, troubleshooting, limitations), [API routes](../api/README.md), [current-state assessment](current-state.md), and the repository README.
- **Not verified here.** The CI `acceptance` job has not yet run on GitHub. The browser walkthrough was run locally and is not committed. The project owner has not yet completed scenario A unaided, and no release has been tagged.


- Keep business rules in the Core and Application layers and persistence behind Application ports; wire configuration and infrastructure in the composition layer.
- Add only the domain/storage changes needed for payer, immutable allocations, repayments, and the agreed correction policy. Reuse existing entities where their meaning remains valid; do not preserve the paid-split experiment at the cost of incorrect accounting.
- All Phase 1 UI operations should use API contracts. Update OpenAPI metadata/contracts and regenerate the client, rather than maintaining two application execution paths.
- Use migrations that preserve existing data by default. Package 1.3 is the explicit owner-approved exception: its migration resets all pre-1.3 application data because paid splits cannot be reliably mapped to shares or repayments. Disclose and test that reset; preserve data in later migrations.
- Keep legacy/unimplemented operations unavailable or explicitly rejected until supported. Do not leave `NotImplementedException` behind advertised release actions.
- Add tests with each package, not just at the final gate. Keep existing tests passing; intentionally changed behavior needs updated expectations.
- No framework/provider replacement unless a reproduced compatibility issue justifies it.

### Phase 1 release gate

The checklist is initially unchecked intentionally. Link test output, release/commit references, or a dated manual verification record when each item passes. A planned feature or passing unit test alone does not satisfy a Docker/browser gate.

- [x] Detailed rules for each delivered package have been reviewed; no unresolved decision affects balances or permissions. Evidence: rules for packages 1.2-1.4 are confirmed in the [specification](README.md); no open rule decisions remain.
- [x] A clean documented Compose install works without manual SQL or source edits. Evidence: package 1.5 fresh-install check (2026-10-20).
- [x] The documented pre-1.3 data reset and migration are verified; subsequent migrations from the supported package 1.3 state are data-preserving. Evidence: package 1.3 and 1.4 verification; the only later migration, `AddRepayments`, is additive.
- [x] All UI Phase 1 operations go through the API; client/contracts match the server. Evidence: package 1.2 UI audit (the UI references only the OpenAPI client project); client regenerated from the server's OpenAPI document in package 1.5.
- [x] Registration, sign-in/out, groups, membership, and organizer/member permissions work. Evidence: package 1.5 HTTP acceptance and browser journey.
- [x] Exact USD equal splits among selected people are persisted and remain stable. Evidence: scenario B in the package 1.5 HTTP acceptance; real-MySQL decimal tests.
- [x] Per-member balances, repayment suggestions, partial/full repayments, and history reconcile. Evidence: scenario A in the package 1.5 HTTP acceptance and browser journey.
- [x] Duplicate/concurrent requests cannot double count or overpay. Evidence: package 1.4 and 1.5 real-MySQL concurrency tests; idempotency and concurrent-repayment acceptance checks; browser form resubmission.
- [x] Correction restrictions and unavailable features are explicit in the UI. Evidence: package 1.5 cleanup; browser checks of void-after-repayment rejection and member view; sign-in page states password recovery is unavailable.
- [x] Specification scenarios A-D pass, including the Docker-hosted browser journey. Evidence: package 1.5 HTTP acceptance (A-C), browser journey (A), fresh install, restart, restore, and failure-state checks (D).
- [x] Real-MySQL tests verify migrations, foreign keys, unique constraints, decimal persistence, and atomic writes. Evidence: package 1.5 Infrastructure run, 49 passed and 0 skipped against migrated MySQL.
- [x] Existing regression/infrastructure tests and solution build pass. Evidence: package 1.5 build and tests.
- [ ] CI runs the necessary automated checks, including the Docker acceptance checks required for release. Added the `acceptance` job in package 1.5; check this item after it passes on GitHub Actions.
- [x] Data survives restart; a backup has been restored and reconciled in a separate disposable environment. Evidence: scenario A step 8 (HTTP and browser) and the package 1.5 backup/restore rehearsal.
- [x] Configuration errors and API/database outages produce useful errors/logs without false success. Evidence: package 1.5 failure-state checks.
- [x] Setup, recovery, limitations, and API documentation describe the delivered behavior; UI claims match it. Evidence: package 1.5 documentation and cleanup.
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

**Next action:** Confirm the CI `acceptance` job passes on GitHub Actions, have the project owner complete scenario A unaided from the [operations guide](../operations/README.md), then tag a versioned release that records the verified commit and the [known limitations](../operations/README.md#limitations). Phase 1 is done when the remaining release-gate items are checked; do not start Phase 2 work before then.
