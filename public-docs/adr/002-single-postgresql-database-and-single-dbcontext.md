# ADR 002: Use one PostgreSQL database and one EF Core context

## Context

The codebase needs inventory data and ASP.NET Core Identity data to coexist cleanly without adding unnecessary operational complexity.

## Decision

Use one PostgreSQL database configured through `ConnectionStrings:DefaultConnection` and one EF Core context: `WestcoastCarsContext`.

## Why

- Matches the current modular-monolith scope.
- Simplifies migrations, startup, and local setup.
- Keeps identity and business persistence in one consistent unit.

## Consequence

- Shared repository documentation should not describe multiple DbContexts.
- Persistence changes should be evaluated against a single-context architecture first.
