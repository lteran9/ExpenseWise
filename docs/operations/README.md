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

### Restarting the stack

Restart with `docker compose down` followed by `docker compose up -d`. Compose then waits for MySQL to be healthy, runs the migration service, and starts the API only after migration succeeds. Avoid `docker compose restart` for the whole stack: it does not honor `depends_on`, so the migration service can start before MySQL is ready, fail to connect, and stay running while the API starts against an unmigrated database. If that happens, run `docker compose down` and `docker compose up -d`.

The UI does not persist its ASP.NET Core data-protection keys. After the UI container is recreated, existing browser sign-ins are invalid and users must sign in again. Sign-ins also expire after 20 minutes.

## Backup and restore

Back up the running database to a file outside the repository. Load the same `.env` values into the shell first (`set -a; . ./.env; set +a`):

```bash
docker compose exec -T -e MYSQL_PWD="$DB_PASSWORD" mysql \
  mysqldump -uroot --single-transaction --routines --triggers "$DB_DATABASE" > expensewise-backup.sql
```

Rehearse a restore in a separate, disposable MySQL container before relying on a backup, and compare its record counts and totals with the source:

```bash
docker run -d --name expensewise-restore -e MYSQL_ROOT_PASSWORD=restore-only \
  -e MYSQL_DATABASE=expensewise mysql:8.0 --character-set-server=utf8mb4 --collation-server=utf8mb4_unicode_ci
# Wait until `docker exec -e MYSQL_PWD=restore-only expensewise-restore mysql -uroot -e "SELECT 1"` succeeds.
docker exec -i -e MYSQL_PWD=restore-only expensewise-restore mysql -uroot expensewise < expensewise-backup.sql
docker exec -e MYSQL_PWD=restore-only expensewise-restore mysql -uroot expensewise -e \
  "SELECT (SELECT COUNT(*) FROM users) users, (SELECT COUNT(*) FROM expenses) expenses, (SELECT SUM(amount) FROM expense_shares) shares, (SELECT SUM(amount) FROM repayments) repayments;"
docker rm -f expensewise-restore
```

To restore into the Compose stack itself, which replaces all current data:

```bash
docker compose stop webapp webapi
docker compose exec -T -e MYSQL_PWD="$DB_PASSWORD" mysql mysql -uroot -e \
  "DROP DATABASE \`$DB_DATABASE\`; CREATE DATABASE \`$DB_DATABASE\` CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;"
docker compose exec -T -e MYSQL_PWD="$DB_PASSWORD" mysql mysql -uroot "$DB_DATABASE" < expensewise-backup.sql
docker compose up -d
```

The migration service should then report that no migrations were applied, and signed-in members should see the same balances as before the backup. The backup contains account data and password hashes; store it privately.

## Failure behavior

- **Missing configuration:** Compose refuses to start and names the missing variable, for example `required variable JWT_SIGNING_KEY is missing a value`.
- **API unavailable:** UI pages show "Unable to load … right now" instead of an empty ledger or group list, and form submissions show an error rather than a confirmation. Check `docker compose logs webapp`.
- **Database unavailable:** the API returns HTTP 500 problem details and logs the connection error; the UI shows the error on the page. Check `docker compose logs webapi`. Service resumes once MySQL is healthy again; no restart of the API is needed.
- **Migration failure:** the migration service exits with an error and the API does not start. Check `docker compose logs migrate`.

## Useful checks

- Inspect service output with `docker compose logs <service>`.
- Check migration completion with `docker compose ps -a migrate` and its output with `docker compose logs migrate`.
- Inspect schema history under `src/Infrastructure/SqlDatabase/Migrations`.
- Check the API's Development OpenAPI document at `/openapi/v1.json` when the API is running in Development.

These instructions describe local, trusted use only; they are not a production deployment guide.
