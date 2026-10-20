# Current-state assessment

Assessed: 2026-10-16, against baseline commit `fd3cdc7`. Capability rows and gaps updated 2026-10-20 after package 1.5; the verified-baseline table below records the original assessment, and later verification is recorded in the [roadmap](roadmap.md).

This is an implementation inventory and gap assessment, not a claim that the application runs end to end. Product requirements are in the [scope specification](README.md); implementation work is in the [roadmap](roadmap.md).

## Summary

ExpenseWise now delivers the Phase 1 journey: accounts, groups of registered members, equal-split USD expenses, voiding, balances, and manual repayment recording, verified on the Docker stack with a real MySQL database through HTTP acceptance checks and a browser walkthrough. Owner acceptance and a versioned release remain open in the [release gate](roadmap.md#phase-1-release-gate).

### Verified baseline

| Check | Result |
| --- | --- |
| Installed SDK | .NET SDK 10.0.300. |
| Solution build | `dotnet build ExpenseWise.sln --no-restore` passed with zero warnings/errors. Existing restored dependencies were used; this does not prove fresh restore or container build. |
| Tests | Editor test runner: 91 passed. `dotnet test --no-build`: 55 regression and 36 infrastructure tests passed, zero failures/skips. |
| Docker tooling | Docker CLI and Compose installed. |
| Docker execution | `docker info` could not connect to the daemon. No images built, stack started, migrations applied, or browser journey verified during this assessment. |

Passing unit/in-memory tests does not establish real MySQL schema compatibility, Docker readiness, access enforcement, or browser usability.

## Capability inventory

Here, **implemented** means code exists, **partial** means important behavior is missing or disconnected, and **stub** means explicitly unimplemented. None of these labels implies Docker/browser verification.

| Capability | State | Evidence and remaining work |
| --- | --- | --- |
| Layered solution and DI | Implemented | [Architecture overview](../architecture/README.md), [DI registrations](../../src/Configuration/DependencyConfiguration/Extensions/DependencyInjection.cs). Retain inward dependencies and repository ports. |
| Registration and password checking | Implemented; package 1.2 locally verified | [CreateUser](../../src/Application/UseCases/CreateUser.cs), [AuthenticateUser](../../src/Application/UseCases/AuthenticateUser.cs), and the [auth UI](../../src/Presentation/UI/Controllers/AuthController.cs) use the API for registration and sign-in. |
| Session identification | Implemented; package 1.2 locally verified | The [API startup](../../src/Presentation/Api/Program.cs) validates signed bearer tokens, and [ApiController](../../src/Presentation/Api/Controllers/ApiController.cs) derives the actor from the authenticated subject claim. The UI uses a cookie-backed session. |
| Group creation/listing/editing | Implemented; package 1.2 locally verified | The [Group UI controller](../../src/Presentation/UI/Controllers/GroupController.cs) uses the generated API client. Package 1.2 verification covers member-scoped lists and organizer-only writes. |
| Add registered members | Implemented; package 1.2 locally verified | [AddMemberToGroup](../../src/Application/UseCases/AddMemberToGroup.cs) looks up an existing user by phone and checks country code. This does not send an invitation or create guests. |
| Member removal/group deletion | Partial | [DeleteMember](../../src/Application/UseCases/DeleteMember.cs) exists but has no member-removal API action; [GroupController](../../src/Presentation/Api/Controllers/GroupController.cs) exposes group deletion. Complete only the agreed safe lifecycle rules, not unrestricted deletion. |
| Expense creation | Implemented; package 1.3 smoke-tested | [CreateExpense](../../src/Application/UseCases/CreateExpense.cs) validates the organizer, payer, selected active members, USD precision/range, and request key; it calculates stable cent shares. The [Expense UI](../../src/Presentation/UI/Controllers/ExpenseController.cs) submits through the generated API client. |
| Expense listing | Implemented; package 1.3 smoke-tested | [ListExpenses](../../src/Application/UseCases/ListExpenses.cs) and the [expense API](../../src/Presentation/Api/Controllers/ExpenseController.cs) return payer, recorder, void state, and persisted shares to the UI. |
| Equal splitting | Implemented; package 1.3 smoke-tested | [CreateExpense](../../src/Application/UseCases/CreateExpense.cs) persists selected members' allocations, assigning remainder cents in stable participant-key order. The former paid-split settle-all operation was removed. |
| Balances | Implemented; package 1.4 locally verified | [GroupLedger](../../src/Core/Entites/GroupLedger.cs) computes per-member net balances from nonvoid expenses and repayments, plus total spent, outstanding, and debtor-to-creditor suggestions. [RetrieveGroupLedger](../../src/Application/UseCases/RetrieveGroupLedger.cs) serves the [balances page](../../src/Presentation/UI/Views/Repayment/Ledger.cshtml). |
| Group detail totals/members | Implemented; package 1.5 verified | [RetrieveGroup](../../src/Application/UseCases/RetrieveGroup.cs) populates members and the same expensed/outstanding totals as the group list. |
| Explicit repayment recording | Implemented; package 1.4 locally verified | [RecordRepayment](../../src/Application/UseCases/RecordRepayment.cs) is organizer-only, validates against one locked ledger state, and stores each repayment with an idempotency key. The [API controller](../../src/Presentation/Api/Controllers/RepaymentController.cs) and [UI controller](../../src/Presentation/UI/Controllers/RepaymentController.cs) use it. The legacy Pay endpoint and PayExpense handler were removed. Corrections and reversals are not supported. |
| Expense editing | Not offered | The unimplemented update stub and its route were removed in package 1.5. Corrections use void and re-entry; voiding is rejected once the group has repayments. |
| Flexible splits/integrated payments | Not delivered | Deferred beyond Phase 1. The [home page](../../src/Presentation/UI/Views/Home/Index.cshtml) and [privacy page](../../src/Presentation/UI/Views/Home/Privacy.cshtml) now describe only equal splitting, registered members, and manual repayment records. |
| Password recovery | Not offered | The placeholder forgot-password flow was removed in package 1.5; the sign-in page states that recovery is unavailable. Deferred beyond Phase 1. |
| Compose and persistent MySQL | Implemented; locally verified | [Compose](../../docker-compose.yml), [API Dockerfile](../../src/Presentation/Api/Dockerfile), [UI Dockerfile](../../src/Presentation/UI/Dockerfile). Package 1.1 verification in the [roadmap](roadmap.md) confirms fresh-volume migration, service readiness, API/UI reachability, persistence across restart, and repeatable migration. |
| Automated checks | Useful but incomplete | [Regression suite](../../src/Tests/Regression), [infrastructure suite](../../src/Tests/Infrastructure), [CI workflow](../../.github/workflows/build.yml). Unit tests use EF InMemory. Package 1.5 added real-MySQL constraint, decimal, and concurrency tests (run when `EXPENSEWISE_TEST_MYSQL_CONNECTION` is set) and the [HTTP acceptance script](../../scripts/acceptance-api.sh), which CI runs against a freshly built Compose stack. Browser acceptance was performed manually with a scripted walkthrough and is not committed or run in CI. |

## Remaining gaps

### 1. Release acceptance by the owner

Package 1.5 recorded engineering evidence against the [Phase 1 release gate](roadmap.md#phase-1-release-gate). The project owner still needs to complete scenario A unaided, accept the release, and publish versioned release notes.

### 2. Coverage outside the automated suites

- Browser behavior is verified manually; there is no committed browser test or CI browser job.
- The real-MySQL integration tests are skipped unless a disposable test database is configured. CI does not configure one, so CI exercises real MySQL only through the Compose acceptance job.
- The stack is a trusted local/private deployment: plain HTTP, Development environment, nonpersisted data-protection keys, and manual backups. See [operations](../operations/README.md#limitations).

This assessment is not a formal security audit or production readiness review.

## Preserve rather than rewrite

- Existing Clean Architecture/CQRS boundaries, repository ports, validation, and response patterns.
- Existing registration/login forms, group management UI, and generated-client integration as starting points.
- Named MySQL persistence and Docker build scaffolding.
- Regression, repository, and mapping tests.

Do not make framework replacement, a new frontend, microservices, or broad dependency modernization prerequisites unless a verified compatibility blocker requires a targeted change.
