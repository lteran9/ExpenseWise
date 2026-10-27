---
description: "Use when creating, editing, or refactoring repositories, queries, Entity Framework Core models, migrations, or AutoMapper profiles in src/Infrastructure/SqlDatabase."
applyTo: "src/Infrastructure/SqlDatabase/**/*.cs"
---

# Data Access & SQL Database Guidelines (src/Infrastructure/SqlDatabase)

This project handles persistence using Entity Framework Core with Oracle's MySQL provider (`MySql.EntityFrameworkCore`). Connection strings are built with `MySql.Data.MySqlClient.MySqlConnectionStringBuilder` in the composition layer.

## 1. Repositories and Ports

- Repository implementations in `src/Infrastructure/SqlDatabase/Repository/` must implement the port interfaces defined in `Application.UseCases.Ports`.
- Use AutoMapper to map between domain entities in `Core.Entities` and EF database entities in `Infrastructure.SqlDatabase.EntityFramework.Models`.
- Keep AutoMapper profiles in `src/Infrastructure/SqlDatabase/Configuration/AutoMapper/`.

## 2. Queries and Raw SQL

- Keep custom query implementations under `src/Infrastructure/SqlDatabase/Query/`.
- Prefer EF Core LINQ queries. Use raw SQL only when LINQ cannot express the operation, such as a row lock.
- Always parameterize raw SQL to prevent SQL injection. Use `FromSql`, `ExecuteSql`, or `ExecuteSqlInterpolatedAsync`, which convert interpolated values into parameters. Never build SQL by string concatenation, and never pass interpolated strings to `FromSqlRaw` or `ExecuteSqlRaw`:
  ```csharp
  // Correct: EF Core sends groupId as a parameter.
  await context.Database.ExecuteSqlInterpolatedAsync(
      $"SELECT id FROM `groups` WHERE id = {groupId} FOR UPDATE",
      cancellationToken);
  ```

## 3. Entity Framework Core (`CoreContext`)

- Configure entities inside `CoreContext.OnModelCreating`.
- When adding migrations, ensure column mappings, keys, foreign keys, and indexes are explicitly defined.
- Always handle cancellation tokens and dispose database contexts properly through ASP.NET Core dependency injection.
