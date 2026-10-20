# Operations and local development

## Prerequisites

- .NET SDK 10.0.
- Docker Engine and Docker Compose for the containerized stack.

## Build and test

From the repository root:

```bash
dotnet build ExpenseWise.sln
dotnet test --no-build
```

The GitHub Actions workflow also restores, builds, and tests with the .NET 10 SDK on pushes and pull requests targeting `master`.

## Docker Compose

Compose defines the API, MVC UI, MySQL database, and a one-shot migration service. The default host ports are API `8081`, UI `8080`, and MySQL `3306`; the API and UI ports and forwarded database port can be overridden with `API_PORT`, `UI_PORT`, and `FORWARD_DB_PORT`. Database initialization requires `DB_DATABASE`, `DB_USERNAME`, and `DB_PASSWORD`. The API also requires `JWT_SIGNING_KEY`, a private random value of at least 32 UTF-8 bytes. Copy `.env.example` to `.env`, replace both placeholders with private local values, and never commit `.env`. Generate a signing key with `openssl rand -base64 48`.

The API receives database host, port, database, username, and password through its composition-layer settings. Compose uses the internal `mysql` service DNS name; do not set the API's database host to `localhost`. The migration service waits for MySQL's healthcheck, applies all pending EF migrations, and must complete successfully before the API starts. Migration errors cause startup to fail rather than serving against an unknown schema.

For running the API or UI directly with `dotnet run`, provide the same database settings through `Database__Host`, `Database__Port`, `Database__Name`, `Database__Username`, and `Database__Password`. The API additionally requires `Authentication__SigningKey` with the same minimum length. Do not put passwords or signing keys in tracked settings files.

The API issues a signed bearer token from `POST /User/Authenticate`; tokens expire after 20 minutes. The MVC UI keeps the API token in its server-side session and protects browser routes with an HTTP-only authentication cookie. Sign in again after expiry. The signing key must be identical across API instances that validate the same tokens and must remain private. This local configuration is not a recommendation for public deployment.

**Package 1.3 migration warning:** the migration intentionally deletes all existing ExpenseWise application data (accounts, groups, memberships, passwords, expenses, and legacy splits) before creating the new payer/share schema. This reset was approved for the beta; do not run it against a database whose data must be retained. Register accounts and recreate groups after migration. Later migrations must preserve the supported ledger unless a separate reset is explicitly approved.

From the repository root, run:

```bash
docker compose up --build -d
docker compose logs -f
```

The UI reaches the API at `http://webapi:8081/` from within the Compose network; container-to-container traffic should use the service hostname, not `localhost`. Stop the stack with `docker compose down`. `docker compose down -v` also deletes the named database volume and its local data; use it only when that data is disposable.

To apply newly added migrations, run `docker compose run --build --rm migrate` to rebuild the image before running it. The step is repeatable: EF applies only migrations not already recorded in the database history table. Editing an already-applied migration does not cause it to run again.

The named `expensewise-mysql` volume keeps the database across container recreation and `docker compose down`. Do not use `docker compose down -v` if the data should be retained.

## Acceptance checks

With the Compose stack running, run the HTTP acceptance script from the repository root:

```bash
scripts/acceptance-api.sh
```

The script requires `curl`, `jq`, and `uuidgen`. It registers disposable accounts on every run and exercises product scenarios A (steps 1-8), B, and the API-observable parts of C: invalid input, authentication and membership rejection, idempotent retries, voiding, self-repayment and overpayment rejection, concurrent repayments, void-after-repayment rejection, rejected account deletion, and the removed expense-update route. Step A8 restarts the `mysql` and `webapi` services and checks that the ledger survives; set `SKIP_RESTART=1` to skip it. Set `API_BASE_URL` when the API is not at `http://localhost:${API_PORT:-8081}`. The script stops at the first failed check and exits nonzero. The `acceptance` job in the GitHub Actions workflow runs the same script against a freshly built Compose stack.

The real-MySQL integration tests in `src/Tests/Infrastructure` are skipped unless `EXPENSEWISE_TEST_MYSQL_CONNECTION` is set. Point them at a disposable, already-migrated database (never one whose data matters) and pass the password through `Database__Password`:

```bash
docker compose exec -T mysql sh -c 'MYSQL_PWD="$MYSQL_ROOT_PASSWORD" exec mysql -uroot -e "CREATE DATABASE IF NOT EXISTS expensewise_test; GRANT ALL ON expensewise_test.* TO \`$MYSQL_USER\`@\`%\`"'
Database__Host=127.0.0.1 Database__Port=3306 Database__Name=expensewise_test \
  Database__Username="$DB_USERNAME" Database__Password="$DB_PASSWORD" \
  dotnet run --project src/Presentation/Api -- --migrate
Database__Password="$DB_PASSWORD" \
  EXPENSEWISE_TEST_MYSQL_CONNECTION="Server=127.0.0.1;Port=3306;Database=expensewise_test;User ID=$DB_USERNAME" \
  dotnet test
```

Load `DB_USERNAME` and `DB_PASSWORD` from `.env` first (for example, `set -a; . ./.env; set +a`). Tests use uniquely keyed data, so they can be rerun against the same test database.

## Backup and restore

The database is the only state that must be backed up; the API and UI containers are stateless apart from sign-in sessions. Backups contain account emails, phone numbers, password hashes, and ledger data, so store them privately, never commit them (the repository ignores `backups/`), and delete copies you no longer need.

Create a consistent backup while the stack is running:

```bash
mkdir -p backups
docker compose exec -T mysql sh -c 'MYSQL_PWD="$MYSQL_PASSWORD" exec mysqldump --single-transaction --no-tablespaces -u"$MYSQL_USER" "$MYSQL_DATABASE"' \
  > "backups/expensewise-$(date +%Y%m%d-%H%M%S).sql"
```

The credentials are read from the container's environment, so they do not appear on the host command line. Check that the file is not empty and ends with `-- Dump completed`.

### Rehearse a restore

Rehearse restores in a disposable Compose project so the working database is not touched. Use different host ports and the same `.env`:

```bash
export COMPOSE_PROJECT_NAME=expensewise-restore-test API_PORT=18081 UI_PORT=18080 FORWARD_DB_PORT=13306
docker compose up -d --wait mysql
docker compose exec -T mysql sh -c 'MYSQL_PWD="$MYSQL_PASSWORD" exec mysql -u"$MYSQL_USER" "$MYSQL_DATABASE"' < backups/<backup-file>.sql
docker compose up -d --wait
```

The migration service should report that no migrations were applied. Reconcile the restore against the source before trusting the backup:

1. Compare `CHECKSUM TABLE` output (and row counts) for `users`, `passwords`, `groups`, `member_of`, `expenses`, `expense_shares`, `repayments`, and `__EFMigrationsHistory` in both databases. Both must run the same MySQL version for checksums to be comparable.
2. Sign in to the restored stack and compare each group's balances, outstanding total, expenses, and repayment history with the source.

Remove the rehearsal with `docker compose down -v` while `COMPOSE_PROJECT_NAME` is still set to the disposable project, then `unset COMPOSE_PROJECT_NAME API_PORT UI_PORT FORWARD_DB_PORT`. Double-check the project name before running `down -v`.

### Restore the working database

Restore only a backup taken from the same ExpenseWise version (the same applied migrations). The dump drops and recreates each table, replacing all current data:

```bash
docker compose stop webapp webapi
docker compose exec -T mysql sh -c 'MYSQL_PWD="$MYSQL_PASSWORD" exec mysql -u"$MYSQL_USER" "$MYSQL_DATABASE"' < backups/<backup-file>.sql
docker compose up -d
```

Sign in again afterwards and verify balances as in the rehearsal.

## Troubleshooting

- Inspect service output with `docker compose logs <service>`. Logs must not contain database passwords or signing keys; report it as a defect if they do.
- Check migration completion with `docker compose ps -a migrate` and its output with `docker compose logs migrate`. The API does not start when migration fails.
- If the API is down, UI pages show an error such as "Unable to load your groups" instead of empty data, and actions are not recorded. Restart it with `docker compose up -d webapi`.
- If MySQL is down, the API logs `Unable to connect to any of the specified MySQL hosts`, and the UI shows a failed request instead of balances. Restart it with `docker compose up -d mysql` and retry; a submission that failed was not recorded, and a retried submission from the same form is not recorded twice.
- Recreating the UI container ends browser sessions; sign in again. API tokens expire after 20 minutes.
- Inspect schema history under `src/Infrastructure/SqlDatabase/Migrations`.
- Check the API's Development OpenAPI document at `/openapi/v1.json` when the API is running in Development.

## Limitations

These instructions cover a trusted local or private-network Compose deployment. They are not a hardened public deployment: the stack serves plain HTTP, runs the API in the Development environment, does not persist ASP.NET Core data-protection keys, and has no automated backup schedule, monitoring, or password recovery.
