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

Compose defines the API, MVC UI, and MySQL services. The default host ports are API `8081`, UI `8080`, and MySQL `3306`; the API and UI ports and forwarded database port can be overridden with `API_PORT`, `UI_PORT`, and `FORWARD_DB_PORT`. Database initialization requires `DB_DATABASE`, `DB_USERNAME`, and `DB_PASSWORD`.

Set the database password through a local secret manager or untracked local environment configuration. Never put a real password in a command copied into documentation, commit, issue, or terminal transcript. From the repository root, run:

```bash
docker compose up --build -d
docker compose logs -f
```

The UI reaches the API at `http://webapi:8081/` from within the Compose network; container-to-container traffic should use the service hostname, not `localhost`. Stop the stack with `docker compose down`. `docker compose down -v` also deletes the named database volume and its local data; use it only when that data is disposable.

Before relying on a local Compose deployment, verify the application's database connection configuration matches the Compose database service and the credentials supplied locally. Compose database initialization variables do not by themselves guarantee that the application is configured to use the same values.

## Useful checks

- Inspect service output with `docker compose logs <service>`.
- Inspect schema history under `Infrastructure/SqlDatabase/Migrations`.
- Check the API's Development OpenAPI document at `/openapi/v1.json` when the API is running in Development.

These instructions describe local development only; they are not a production deployment or backup procedure.
