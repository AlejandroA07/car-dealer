# Westcoast Cars

[![CI](https://github.com/AlejandroA07/car-dealer/actions/workflows/ci.yml/badge.svg)](https://github.com/AlejandroA07/car-dealer/actions/workflows/ci.yml)

A car dealership platform built with .NET 10 and Clean Architecture. Manages vehicle inventory, service bookings, and user roles through a REST API and a server-rendered web UI — all running as a Docker Compose stack.

## Tech Stack

- .NET 10 / ASP.NET Core
- PostgreSQL 16
- Entity Framework Core + CQRS (MediatR)
- JWT authentication / ASP.NET Core Identity
- Docker + Docker Compose

## Architecture

```mermaid
flowchart LR
    browser["Browser"] --> web["WestcoastCars.Web\nASP.NET Core MVC"]
    web --> api["WestcoastCars.Api\nREST API"]
    api --> db["PostgreSQL"]
```

---

## Quick Start

Just want to run the app? You only need Docker — no .NET SDK required.

**Prerequisites:** Docker Desktop (or Docker Engine + Compose plugin)

**1. Set up credentials**

```bash
cp .env.example .env
```

Open `.env` and fill in your values — Docker Compose injects these into all containers:

| Variable | Description |
|----------|-------------|
| `POSTGRES_PASSWORD` | PostgreSQL password |
| `JWT_SECRET` | JWT signing secret — minimum 32 characters |
| `ADMIN_PASSWORD` | Admin seed account password |

**2. Start the stack**

```bash
docker compose up --build
```

**3. Open**

| Service | URL |
|---------|-----|
| Web UI | http://localhost:5002 |
| API | http://localhost:5001 |
| Swagger | http://localhost:5001/swagger |
| PostgreSQL | localhost:5432 |

> **Data protection keys** — both containers persist ASP.NET Core Data Protection keys to `./dpkeys/` on the host. Created automatically on first run. **Back it up** — losing these keys invalidates all active sessions. Do not delete between deployments.

---

## Development

Run the API and Web locally for hot reload and easier debugging.

**Prerequisites:**
- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- Docker Desktop — for the database container (or a local PostgreSQL 16 install)

### 1. Set up credentials

You need credentials in two places: `.env` (used by the DB container) and user secrets (used by the local .NET processes). Both must have the same values.

**a) Create your `.env`**

```bash
cp .env.example .env
```

Open `.env` and fill in your values:

| Variable | Description |
|----------|-------------|
| `POSTGRES_PASSWORD` | PostgreSQL password |
| `JWT_SECRET` | JWT signing secret — minimum 32 characters |
| `ADMIN_PASSWORD` | Admin seed account password |

**b) Set user secrets**

User secrets are stored outside the repo and never committed. Replace each `<...>` with the matching value from your `.env`.

API — from `WestcoastCars.Api/`:

```bash
cd WestcoastCars.Api

dotnet user-secrets set "ConnectionStrings:DefaultConnection" \
  "Host=localhost;Port=5432;Database=westcoast_cars;Username=postgres;Password=<POSTGRES_PASSWORD>"

dotnet user-secrets set "JwtSettings:Secret" "<JWT_SECRET>"

dotnet user-secrets set "AdminSettings:Password" "<ADMIN_PASSWORD>"
```

Web — from `WestcoastCars.Web/`:

```bash
cd WestcoastCars.Web

dotnet user-secrets set "JwtSettings:Secret" "<JWT_SECRET>"
```

### 2. Start the database

**Option A — Docker (recommended):**

```bash
docker compose up db
```

Starts PostgreSQL at `localhost:5432` using `POSTGRES_PASSWORD` from your `.env`.

**Option B — native PostgreSQL 16:**

Create a database named `westcoast_cars` with the username and password you used in your user secrets.

### 3. Run the projects

Open two terminals:

```bash
# Terminal 1 — API
cd WestcoastCars.Api
dotnet run --launch-profile http      # http://localhost:5001
dotnet watch --launch-profile http    # same, with hot reload
```

```bash
# Terminal 2 — Web
cd WestcoastCars.Web
dotnet run --launch-profile http      # http://localhost:5002
dotnet watch --launch-profile http    # same, with hot reload
```

| Service | URL |
|---------|-----|
| Web UI | http://localhost:5002 |
| API | http://localhost:5001 |
| Swagger | http://localhost:5001/swagger |
| PostgreSQL | localhost:5432 |

> Migrations run automatically on API startup — no `dotnet ef database update` needed.

---

## Tests

```bash
dotnet test westcoast-cars.sln
```

## Further Reading

- [Architecture overview](public-docs/architecture-overview.md)
- [Architecture decision records](public-docs/adr/)
