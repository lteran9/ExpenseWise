---
description: "Use when creating, editing, or refactoring repositories, queries, Entity Framework Core models, migrations, or AutoMapper profiles in src/Infrastructure/SqlDatabase."
applyTo: "src/Infrastructure/SqlDatabase/**/*.cs"
---

# Data Access & SQL Database Guidelines (src/Infrastructure/SqlDatabase)

This project handles persistence using Entity Framework Core (MySQL) alongside Dapper queries where high-performance raw SQL is required.

## 1. Repositories and Ports

- Repository implementations in `src/Infrastructure/SqlDatabase/Repository/` must implement the port interfaces defined in `Application.UseCases.Ports`.
- Use AutoMapper to map between domain entities in `Core.Entities` and EF database entities in `Infrastructure.SqlDatabase.EntityFramework.Models`.
- Keep AutoMapper profiles in `src/Infrastructure/SqlDatabase/Configuration/AutoMapper/`.

## 2. Dapper & SQL Queries

- Keep custom query implementations under `src/Infrastructure/SqlDatabase/Query/`.
- Always parameterize SQL queries to prevent SQL injection vulnerabilities. Never concatenate raw strings into SQL statements:
  ```csharp
  // Correct:
  var result = await connection.QueryAsync<GroupEntity>(
      "SELECT * FROM Groups WHERE UniqueKey = @Key",
      new { Key = key });
  ```

## 3. Entity Framework Core (`CoreContext`)

- Configure entities inside `CoreContext.OnModelCreating`.
- When adding migrations, ensure column mappings, keys, foreign keys, and indexes are explicitly defined.
- Always handle cancellation tokens and dispose database contexts properly through ASP.NET Core dependency injection.
