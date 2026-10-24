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
- Exit-gate follow-up: the full scenario B and C browser journey passed on 2026-10-24; see the [release-gate verification](#phase-1-release-gate-verification-2026-10-24).

### Package 1.4 verification (2026-10-20)

- `dotnet build ExpenseWise.sln` succeeded with 0 warnings/errors. `dotnet test --no-build` passed the Regression suite (93 tests).
- Infrastructure tests ran against the local MySQL container on a dedicated `expensewise_test` database, with migrations applied: 42 passed, 0 skipped. This includes real-MySQL checks that concurrent same-key expense and repayment requests store one row, and that an overpayment is rejected without writing.
- Applied the additive `AddRepayments` migration with the Compose migrate service. Rebuilt the API and UI images; the UI root returned HTTP 200, and the unauthenticated repayment ledger API request returned HTTP 401.
- Not yet verified: the browser journey for scenario A (including the confirmation dialog and retry behavior), the restart check, and real-MySQL tests for void-versus-repayment locking. These remain open for the exit gate.
- Exit-gate follow-up: all three passed on 2026-10-24; see the [release-gate verification](#phase-1-release-gate-verification-2026-10-24).
- Behavior change from 1.3, confirmed by the spec: voiding is blocked after any repayment exists, and member removal is blocked when the member has expense or repayment history.

### Implementation guidance

- Keep business rules in the Core and Application layers and persistence behind Application ports; wire configuration and infrastructure in the composition layer.
- Add only the domain/storage changes needed for payer, immutable allocations, repayments, and the agreed correction policy. Reuse existing entities where their meaning remains valid; do not preserve the paid-split experiment at the cost of incorrect accounting.
- All Phase 1 UI operations should use API contracts. Update OpenAPI metadata/contracts and regenerate the client, rather than maintaining two application execution paths.
- Use migrations that preserve existing data by default. Package 1.3 is the explicit owner-approved exception: its migration resets all pre-1.3 application data because paid splits cannot be reliably mapped to shares or repayments. Disclose and test that reset; preserve data in later migrations.
- Keep legacy/unimplemented operations unavailable or explicitly rejected until supported. Do not leave `NotImplementedException` behind advertised release actions.
- Add tests with each package, not just at the final gate. Keep existing tests passing; intentionally changed behavior needs updated expectations.
- No framework/provider replacement unless a reproduced compatibility issue justifies it.

### Phase 1 release-gate verification (2026-10-24)

Manual verification on branch `feature/phase-1.5-release-proof`, using an isolated Compose project (`ewgate`) with a fresh volume, non-default ports, and private generated secrets. Disposable accounts: Alice (organizer), Bob, Cara, Dan, and Eve (nonmember).

- **Build and tests:** `dotnet build ExpenseWise.sln` succeeded with 0 warnings/errors. `dotnet test --no-build` passed 143 tests: 94 Regression and 49 Infrastructure, 0 skipped. The Infrastructure run used a migrated real-MySQL `expensewise_test` database.
- **Clean install:** `docker compose up --build -d` on an empty volume made MySQL healthy, and the migration service applied all three migrations and exited 0. The API OpenAPI document and UI root returned HTTP 200. No manual SQL or source edits were needed.
- **Scenario A in the Docker-hosted browser:** each step matched the specification exactly:
  - balances +60/−30/−30, then +45/−15/−30 with total 120 and outstanding 45;
  - suggestions Bob→Alice 15 and Cara→Alice 30;
  - Bob→Alice 5.00 through the confirmation dialog, giving +40/−10/−30 and outstanding 40;
  - replaying the same submission key changed nothing;
  - the remaining repayments brought every balance and the outstanding amount to zero, with total spent still 120.
  
  `docker compose down`/`up -d` kept every expense, share, repayment, and the zero balances. Migration reported "No migrations were applied".
- **Scenario B in the browser:**
  - 10.00 split 3.34/3.33/3.33 and stayed the same after reload.
  - Adding a fourth member left the existing expense unchanged.
  - An excluded payer received full credit.
  - A solo payer-participant netted to zero.
  - Voiding an expense kept it, labelled "Voided" with its shares, and every balance recalculated exactly.
- **Scenario C in the browser and over direct HTTP:**
  - **Rejected with visible errors and no writes:** invalid amounts (excess precision, zero, negative, out of range); no participants; duplicate or nonmember participants; nonmember payers; EUR expenses and repayments; duplicate membership; self-repayments; overpayments; and voids after a repayment.
  - **Access:** anonymous API calls returned 401. Eve saw no groups and was refused every direct URL and API read. Bob saw no organizer actions, and his forged UI posts and direct API writes were rejected server-side.
  - **Unsupported actions:** expense editing and account deletion are explicitly rejected.
- **Scenario D:**
  - **Restore:** a `mysqldump` backup was restored into a separate disposable MySQL container. Record counts, expense/share/repayment sums, and data rows matched the source exactly. A drop-and-restore of the Compose database followed by `up -d` served the same ledger (33.00 spent, 22.67 outstanding, identical member balances).
  - **API stopped:** UI pages showed "Unable to load … right now" with no empty-looking ledger or group list.
  - **MySQL stopped:** the API returned 500 problem details and logged the connection error, the UI showed the error, and the stack recovered once MySQL was healthy.
  - **Missing `JWT_SIGNING_KEY`:** Compose refused to start and named the variable.
- **Migrations:**
  - **Pre-1.3 reset:** a database created at the initial migration and seeded with legacy users, groups, memberships, passwords, expenses, and splits was upgraded to the current schema. All legacy rows were removed and `splits` was dropped, as documented.
  - **Upgrade from 1.3:** a populated database migrated down to the package 1.3 migration and back up kept every user, group, membership, password, expense, and share row byte-identical.
- **Contracts:** the API serves the checked-in `docs/api/open-api.json`, and regenerating the client with the documented NSwag command reproduces `ExpenseWiseApiClient.generated.cs` exactly. The UI references only the OpenAPI client project.
- **Defects fixed during verification:**
  - Updating a group or user reset its stored `created_at` to `0001-01-01`.
  - The UI showed `¤` instead of `$` because its culture was not pinned to en-US.
  - The group list showed an empty state beside a load error.
  - The Void button still appeared after repayments existed. It is now hidden and replaced by a note.
  - The amount error appeared twice.
  - The Invite page lost its heading after an error.
  - A nonmember saw the expense form under "Group not found".
  - Repayment times had no UTC label.
  - Removed: the placeholder forgot-password page, the template privacy text, and Home page claims for unsupported split types.
- **New real-MySQL tests:** a void is rejected after a repayment; a concurrent void and repayment apply exactly one operation; foreign keys reject a share for a nonexistent user; the maximum amount persists to the exact cent.
- **Spec decisions (owner, 2026-10-24):** the rounding-remainder and suggestion-order rules were reworded to match the implemented, tested behavior. Read access to archived group ledgers is an accepted Phase 1 limitation.

Known limitations of this release:
- Archived group ledgers are retained in the database but cannot be read.
- The UI does not persist data-protection keys, so users must sign in again after the UI container is recreated.
- Use `down`/`up` rather than `docker compose restart` (see the [operations guide](../operations/README.md)).
- Browser acceptance is manual and not run in CI.

### Phase 1 release gate

The checklist is initially unchecked intentionally. Link test output, release/commit references, or a dated manual verification record when each item passes. A planned feature or passing unit test alone does not satisfy a Docker/browser gate. Checked items below cite the [2026-10-24 verification](#phase-1-release-gate-verification-2026-10-24).

- [ ] Detailed rules for each delivered package have been reviewed; no unresolved decision affects balances or permissions. *(The open rounding, suggestion-order, and archival questions were decided on 2026-10-24; awaiting the owner's final rules sign-off.)*
- [x] A clean documented Compose install works without manual SQL or source edits.
- [x] The documented pre-1.3 data reset and migration are verified; subsequent migrations from the supported package 1.3 state are data-preserving.
- [x] All UI Phase 1 operations go through the API; client/contracts match the server.
- [x] Registration, sign-in/out, groups, membership, and organizer/member permissions work.
- [x] Exact USD equal splits among selected people are persisted and remain stable.
- [x] Per-member balances, repayment suggestions, partial/full repayments, and history reconcile.
- [x] Duplicate/concurrent requests cannot double count or overpay.
- [x] Correction restrictions and unavailable features are explicit in the UI.
- [x] Specification scenarios A-D pass, including the Docker-hosted browser journey.
- [x] Real-MySQL tests verify migrations, foreign keys, unique constraints, decimal persistence, and atomic writes.
- [x] Existing regression/infrastructure tests and solution build pass.
- [ ] CI runs the necessary automated checks, including the Docker acceptance checks required for release. *(CI builds, migrates MySQL, and runs all tests; it has no Docker/browser acceptance job.)*
- [x] Data survives restart; a backup has been restored and reconciled in a separate disposable environment.
- [x] Configuration errors and API/database outages produce useful errors/logs without false success.
- [x] Setup, recovery, limitations, and API documentation describe the delivered behavior; UI claims match it.
- [ ] A versioned release or recorded release commit identifies the verified result and its known limitations. *(Pending the commit of this verification.)*
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

**Next action:** Add a CI job for the Docker acceptance checks (or the owner records that manual Docker/browser verification satisfies that gate item), sign off the detailed rules, approve and commit the package 1.5 release-proof changes as the recorded release commit, then complete scenario A unaided using the [operations guide](../operations/README.md).
