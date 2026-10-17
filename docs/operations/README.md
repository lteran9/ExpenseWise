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

Compose defines the API, MVC UI, MySQL database, and a one-shot migration service. The default host ports are API `8081`, UI `8080`, and MySQL `3306`; the API and UI ports and forwarded database port can be overridden with `API_PORT`, `UI_PORT`, and `FORWARD_DB_PORT`. Database initialization requires `DB_DATABASE`, `DB_USERNAME`, and `DB_PASSWORD`. Copy `.env.example` to `.env` and set a private local password before starting the stack. The ignored `.env` file is not committed.

The API receives database host, port, database, username, and password through its composition-layer settings. Compose uses the internal `mysql` service DNS name; do not set the API's database host to `localhost`. The migration service waits for MySQL's healthcheck, applies all pending EF migrations, and must complete successfully before the API starts. Migration errors cause startup to fail rather than serving against an unknown schema.

For running the API or UI directly with `dotnet run`, provide the same settings through `Database__Host`, `Database__Port`, `Database__Name`, `Database__Username`, and `Database__Password`. Do not put passwords in tracked settings files.

From the repository root, run:

```bash
docker compose up --build -d
docker compose logs -f
```

The UI reaches the API at `http://webapi:8081/` from within the Compose network; container-to-container traffic should use the service hostname, not `localhost`. Stop the stack with `docker compose down`. `docker compose down -v` also deletes the named database volume and its local data; use it only when that data is disposable.

To apply migrations again after changing migration files, run `docker compose run --rm migrate`. The step is repeatable: EF applies only migrations not already recorded in the database history table.

The named `expensewise-mysql` volume keeps the database across container recreation and `docker compose down`. Do not use `docker compose down -v` if the data should be retained.

## Useful checks

- Inspect service output with `docker compose logs <service>`.
- Check migration completion with `docker compose ps -a migrate` and its output with `docker compose logs migrate`.
- Inspect schema history under `Infrastructure/SqlDatabase/Migrations`.
- Check the API's Development OpenAPI document at `/openapi/v1.json` when the API is running in Development.

These instructions describe local development only; they are not a production deployment or backup procedure.
