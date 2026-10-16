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
| Layered solution and DI | Implemented | [Architecture overview](../architecture/README.md), [DI registrations](../../Configuration/DependencyConfiguration/Extensions/DependencyInjection.cs). Retain inward dependencies and repository ports. |
| Registration and password checking | Implemented, integration unverified | [CreateUser](../../Application/UseCases/CreateUser.cs), [AuthenticateUser](../../Application/UseCases/AuthenticateUser.cs), [auth UI](../../Presentation/UI/Controllers/AuthController.cs). Password hashing and form flows exist; server-enforced identity and authorization still need completion. |
| Session identification | Partial | [UserInfoActionFilter](../../Presentation/UI/Filters/UserInfoActionFilter.cs) reads a session user key; [API startup](../../Presentation/Api/Program.cs) does not establish an authenticated principal. A session GUID is not an authorization policy. |
| Group creation/listing/editing | Implemented, integration unverified | [Group UI controller](../../Presentation/UI/Controllers/GroupController.cs) uses the generated API client; [CreateGroup](../../Application/UseCases/CreateGroup.cs) also adds the owner as a member. Need authorized, atomic writes and normal empty states. |
| Add registered members | Implemented, integration unverified | [AddMemberToGroup](../../Application/UseCases/AddMemberToGroup.cs) looks up an existing user by phone and checks country code. This does not send an invitation or create guests. |
| Member removal/group deletion | Partial | [DeleteMember](../../Application/UseCases/DeleteMember.cs) exists but has no member-removal API action; [GroupController](../../Presentation/Api/Controllers/GroupController.cs) exposes group deletion. Complete only the agreed safe lifecycle rules, not unrestricted deletion. |
| Expense creation | Partial | [CreateExpense](../../Application/UseCases/CreateExpense.cs) persists description/currency/amount/group/creator. [Expense UI](../../Presentation/UI/Controllers/ExpenseController.cs) uses MediatR directly; [form](../../Presentation/UI/Views/Expense/Create.cshtml) omits currency input. No explicit independent payer or participant selection. |
| Expense listing | Partial | [ListExpenses](../../Application/UseCases/ListExpenses.cs) exists and serves the UI via MediatR. It fetches splits but does not use them in its response; no list action exists in [ExpenseController](../../Presentation/Api/Controllers/ExpenseController.cs). |
| Equal splitting | Prototype; not the target behavior | [SplitExpense](../../Application/UseCases/SplitExpense.cs) visits every group expense for one requested user, divides by the current group member count, and immediately marks each inserted split paid. It does not create all selected participants' obligations or allocate remainder cents. |
| Balances | Partial and unsuitable for target ledger | [ListGroups](../../Application/UseCases/ListGroups.cs) subtracts paid split amounts from expense totals and clamps outstanding at zero. It does not calculate per-member payer credits/debts or who owes whom. |
| Group detail totals/members | Partial | [RetrieveGroup](../../Application/UseCases/RetrieveGroup.cs) declares totals and members in its response but does not populate them. The list path populates more fields; read surfaces need consistent results. |
| Explicit repayment recording | Stub | [PayExpense](../../Application/UseCases/PayExpense.cs) throws `NotImplementedException`; request/response types are empty despite an exposed API route. Existing settle-up UI calls the split experiment, not this handler. |
| Expense editing | Stub | [UpdateExpense](../../Application/UseCases/UpdateExpense.cs) throws `NotImplementedException`, with empty contracts and an exposed route. General editing is not required for Phase 1; implement the agreed void/re-entry correction path instead. |
| Flexible splits/integrated payments | Not delivered | [Home page](../../Presentation/UI/Views/Home/Index.cshtml) advertises capabilities beyond the connected UI and implemented handlers. Align claims to the actual release. |
| Password recovery | Placeholder | [AuthController](../../Presentation/UI/Controllers/AuthController.cs) has a nonfunctional forgot-password flow. Deferred from Phase 1, not a working capability. |
| Compose and persistent MySQL | Scaffolding exists; execution unverified | [Compose](../../docker-compose.yml), [API Dockerfile](../../Presentation/Api/Dockerfile), [UI Dockerfile](../../Presentation/UI/Dockerfile). Named database volume and internal UI-to-API URL exist; startup/configuration/schema gaps remain. |
| Automated checks | Useful but incomplete | [Regression suite](../../Tests/Regression), [infrastructure suite](../../Tests/Infrastructure), [CI workflow](../../.github/workflows/build.yml). Repository tests use EF InMemory; no real-MySQL, HTTP authorization, Docker smoke, or browser acceptance coverage was found. |

## Highest-priority gaps

### 1. Docker installation is not reproducible yet

- [CoreContext](../../Infrastructure/SqlDatabase/EntityFramework/CoreContext.cs) defaults to a fixed connection string and a generated-container-style hostname rather than consuming the Compose database variables.
- [Compose](../../docker-compose.yml) supplies MySQL initialization variables, but not application database connection configuration. Initialization variables do not configure EF.
- Startup dependencies are service-start ordering, not a healthy-database/migration-complete gate. MySQL has a healthcheck, but application dependencies do not wait on it.
- A migration exists, but no migration application step was found in startup or Compose. A fresh volume must not require hand-written schema setup.
- There is visible model/migration drift: the [initial migration](../../Infrastructure/SqlDatabase/Migrations/20260615043632_CreateTablesAndSeed.cs) creates a required `expenses.settled` column absent from the current [expense model](../../Infrastructure/SqlDatabase/EntityFramework/Models/ExpenseEntity.cs). Verify and reconcile the complete model on real MySQL; do not assume the migration matches today's code.
- The actual SQL provider is `MySql.EntityFrameworkCore` 8.0.11 in [SqlDatabase.csproj](../../Infrastructure/SqlDatabase/SqlDatabase.csproj), not Pomelo. Verify the actual dependency set before changing tooling or migrations.

### 2. The money model conflates obligations and repayment

- [Expense](../../Core/Entites/Expense.cs) has `CreatedBy`, but no distinct payer/recording-user concepts.
- [Split](../../Core/Entites/Split.cs) combines allocated amount, a paid flag, and paid date; no independent sender-to-recipient repayment record exists.
- The split operation applies to every expense using today's membership, records only the requesting user's share, and marks it paid immediately.
- Repeating that operation attempts duplicate rows against the [unique split index](../../Infrastructure/SqlDatabase/EntityFramework/Models/SplitEntity.cs); it is not an idempotent settlement operation.
- Writes occur one split at a time; an all-or-nothing expense/allocation operation is needed.
- There are no explicit selected participants, deterministic cent allocation, stable historical allocations, or reconciling member balances.

This is a small but necessary domain correction before wiring more forms to the existing split action.

### 3. The UI does not consistently go through the API

Groups use the generated HTTP client. Authentication, expenses, and splitting still call application handlers directly from the UI. Consequently the UI requires database/application wiring as well as API access, and API-only tests cannot verify the whole product.

Complete UI -> API -> use case -> persistence for all Phase 1 journeys, update contracts, and regenerate the client through the [existing workflow](../../Infrastructure/OpenAPI/README.md). Preserve form validation, antiforgery, redirects, and useful API error messages.

### 4. Identity and group access need explicit enforcement

The inspected startup and handlers do not establish and enforce the organizer/member permissions in the proposed specification. Hiding edit links is insufficient. Define the authenticated actor and enforce the same rules on reads and writes through both UI and API.

This assessment is not a formal security audit or production readiness review.

### 5. Tests miss the defining product behavior

The passing suites provide a useful starting point. Add exact-money, selected-participant, duplicate/retry, concurrency, payer, access-boundary, real-MySQL migration/constraint, and Docker/browser acceptance tests before declaring Phase 1 complete.

## Preserve rather than rewrite

- Existing Clean Architecture/CQRS boundaries, repository ports, validation, and response patterns.
- Existing registration/login forms, group management UI, and generated-client integration as starting points.
- Named MySQL persistence and Docker build scaffolding.
- Regression, repository, and mapping tests.

Do not make framework replacement, a new frontend, microservices, or broad dependency modernization prerequisites unless a verified compatibility blocker requires a targeted change.
