# Current-state assessment

Assessed: 2026-10-16, against baseline commit `fd3cdc7`.

This is an implementation inventory and gap assessment, not a claim that the application runs end to end. Product requirements are in the [scope specification](README.md); implementation work is in the [roadmap](roadmap.md).

## Summary

ExpenseWise has a reusable layered foundation, account/group workflows, expense persistence, and an early split/settlement experiment. It is not yet a complete, verified expense-sharing product. Preserve the foundation and complete one vertical user journey rather than starting over or adding more disconnected endpoints.

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
| Group creation/listing/editing | Implemented; package 1.2 locally verified | The [Group UI controller](../../src/Presentation/UI/Controllers/GroupController.cs) uses the generated API client. Package 1.2 verification covers member-scoped lists and organizer-only writes; [group detail totals](../../src/Application/UseCases/RetrieveGroup.cs) remain incomplete. |
| Add registered members | Implemented; package 1.2 locally verified | [AddMemberToGroup](../../src/Application/UseCases/AddMemberToGroup.cs) looks up an existing user by phone and checks country code. This does not send an invitation or create guests. |
| Member removal/group deletion | Partial | [DeleteMember](../../src/Application/UseCases/DeleteMember.cs) exists but has no member-removal API action; [GroupController](../../src/Presentation/Api/Controllers/GroupController.cs) exposes group deletion. Complete only the agreed safe lifecycle rules, not unrestricted deletion. |
| Expense creation | Implemented; package 1.3 smoke-tested | [CreateExpense](../../src/Application/UseCases/CreateExpense.cs) validates the organizer, payer, selected active members, USD precision/range, and request key; it calculates stable cent shares. The [Expense UI](../../src/Presentation/UI/Controllers/ExpenseController.cs) submits through the generated API client. |
| Expense listing | Implemented; package 1.3 smoke-tested | [ListExpenses](../../src/Application/UseCases/ListExpenses.cs) and the [expense API](../../src/Presentation/Api/Controllers/ExpenseController.cs) return payer, recorder, void state, and persisted shares to the UI. |
| Equal splitting | Implemented; package 1.3 smoke-tested | [CreateExpense](../../src/Application/UseCases/CreateExpense.cs) persists selected members' allocations, assigning remainder cents in stable participant-key order. The former paid-split settle-all operation was removed. |
| Balances | Partial | [ListGroups](../../src/Application/UseCases/ListGroups.cs) derives outstanding from nonvoid expense payers and shares. Repayments and debtor-to-creditor suggestions remain package 1.4 work. |
| Group detail totals/members | Partial | [RetrieveGroup](../../src/Application/UseCases/RetrieveGroup.cs) declares totals and members in its response but does not populate them. The list path populates more fields; read surfaces need consistent results. |
| Explicit repayment recording | Stub | [PayExpense](../../src/Application/UseCases/PayExpense.cs) throws `NotImplementedException`; request/response types are empty despite an exposed API route. Existing settle-up UI calls the split experiment, not this handler. |
| Expense editing | Stub | [UpdateExpense](../../src/Application/UseCases/UpdateExpense.cs) throws `NotImplementedException`, with empty contracts and an exposed route. General editing is not required for Phase 1; implement the agreed void/re-entry correction path instead. |
| Flexible splits/integrated payments | Not delivered | [Home page](../../src/Presentation/UI/Views/Home/Index.cshtml) advertises capabilities beyond the connected UI and implemented handlers. Align claims to the actual release. |
| Password recovery | Placeholder | [AuthController](../../src/Presentation/UI/Controllers/AuthController.cs) has a nonfunctional forgot-password flow. Deferred from Phase 1, not a working capability. |
| Compose and persistent MySQL | Implemented; locally verified | [Compose](../../docker-compose.yml), [API Dockerfile](../../src/Presentation/Api/Dockerfile), [UI Dockerfile](../../src/Presentation/UI/Dockerfile). Package 1.1 verification in the [roadmap](roadmap.md) confirms fresh-volume migration, service readiness, API/UI reachability, persistence across restart, and repeatable migration. |
| Automated checks | Useful but incomplete | [Regression suite](../../src/Tests/Regression), [infrastructure suite](../../src/Tests/Infrastructure), [CI workflow](../../.github/workflows/build.yml). Unit tests use EF InMemory; package 1.3's isolated MySQL/API/UI smoke is recorded in the [roadmap](roadmap.md), but committed real-MySQL, HTTP authorization, and browser-acceptance tests remain absent. |

## Highest-priority gaps

### 1. Balances and manual repayments are not complete

- Expenses now distinguish the recording user from the payer and persist selected participants' immutable shares. Repayments are not yet recorded as separate sender-to-recipient facts.
- Group outstanding values derive from expense payers and shares but do not account for repayments or show who owes whom.

Package 1.4 must add repayment history, deterministic repayment suggestions, and consistent validation against one ledger state.

### 2. Automated checks do not yet cover full release acceptance

The regression and infrastructure suites now cover exact allocations, selected participants, payer distinction, retries, and voiding. Remaining gaps include automated concurrency and real-MySQL migration/constraint tests, plus full Docker-hosted browser acceptance for the product scenarios.

This assessment is not a formal security audit or production readiness review.

## Preserve rather than rewrite

- Existing Clean Architecture/CQRS boundaries, repository ports, validation, and response patterns.
- Existing registration/login forms, group management UI, and generated-client integration as starting points.
- Named MySQL persistence and Docker build scaffolding.
- Regression, repository, and mapping tests.

Do not make framework replacement, a new frontend, microservices, or broad dependency modernization prerequisites unless a verified compatibility blocker requires a targeted change.
