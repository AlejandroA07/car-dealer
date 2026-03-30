# Westcoast Cars (Microservices)

Westcoast Cars is a .NET 9 microservices-based sample application for a car dealership platform. It ships as a Docker Compose stack with a Web UI, a main API, and a dedicated authentication service.

## Architecture

This repository runs as a Docker Compose stack:

- **web** (`westcoast-cars.web`): ASP.NET Core MVC app (user-facing UI)
- **api** (`westcoast-cars.api`): main REST API (inventory, manufacturers, etc.)
- **auth-api** (`westcoast-cars.auth/Api`): authentication/authorization microservice (ASP.NET Core Identity + JWT)
- **db**: MySQL for the main API
- **auth-db**: MySQL for the auth service

## Tech stack

- .NET 9 / ASP.NET Core
- MySQL 8
- Docker + Docker Compose

## Quick start (Docker Compose)

### Prerequisites

- Docker Desktop (or Docker Engine + Docker Compose plugin)

### 1) Create a `.env` file (NOT committed)

Create `.env` in the repository root:

- `MYSQL_PASSWORD` (MySQL root password used by the compose DB containers)
- `JWT_SECRET` (must be the same for `api` and `auth-api`)
- `ADMIN_PASSWORD` (used by `auth-api` to seed an admin user)

Example:
```bash
MYSQL_PASSWORD=change-me
JWT_SECRET=change-me-to-a-long-random-value
ADMIN_PASSWORD=change-me
```

### 2) Start the stack

```bash
docker compose up --build
```

### 3) Open the app

- Web UI: `http://localhost:5002`

Optional local endpoints (if exposed by your compose config):
- API: `http://localhost:5001`
- Auth API: `http://localhost:5003`

## Local development (without Docker)

### Prerequisites

- .NET 9 SDK
- MySQL 8

### Create databases

```sql
CREATE DATABASE westcoast_cars_db;
CREATE DATABASE westcoast_auth;
```

### Configure User Secrets

You’ll need connection strings plus a shared JWT secret.

API:
```bash
dotnet user-secrets init --project westcoast-cars.api/westcoast-cars.api.csproj
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost;Port=3306;Database=westcoast_cars_db;Uid=root;Pwd=YourLocalPassword;" --project westcoast-cars.api/westcoast-cars.api.csproj
dotnet user-secrets set "JwtSettings:Secret" "<YOUR_GENERATED_SECRET>" --project westcoast-cars.api/westcoast-cars.api.csproj
```

Auth API (JWT secret must match the API secret):
```bash
dotnet user-secrets init --project westcoast-cars.auth/Api/WestcoastCars.Auth.Api.csproj
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost;Port=3306;Database=westcoast_auth;Uid=root;Pwd=YourLocalPassword;" --project westcoast-cars.auth/Api/WestcoastCars.Auth.Api.csproj
dotnet user-secrets set "JwtSettings:Secret" "<YOUR_GENERATED_SECRET>" --project westcoast-cars.auth/Api/WestcoastCars.Auth.Api.csproj
dotnet user-secrets set "AdminSettings:Password" "ChangeThisAdminPassword!" --project westcoast-cars.auth/Api/WestcoastCars.Auth.Api.csproj
```

Web:
```bash
dotnet user-secrets init --project westcoast-cars.web/westcoast-cars.web.csproj
dotnet user-secrets set "Services:ApiUrl" "http://localhost:5001" --project westcoast-cars.web/westcoast-cars.web.csproj
dotnet user-secrets set "Services:AuthUrl" "http://localhost:5003" --project westcoast-cars.web/westcoast-cars.web.csproj
```

### Run services (3 terminals)

```bash
dotnet run --project westcoast-cars.api/westcoast-cars.api.csproj
dotnet run --project westcoast-cars.auth/Api/WestcoastCars.Auth.Api.csproj
dotnet run --project westcoast-cars.web/westcoast-cars.web.csproj
```

## Tests

```bash
dotnet test westcoast-cars.sln
```

## Contributing (short)

- Use feature branches.
- Keep secrets out of git (use `.env` for Docker or .NET User Secrets for local runs).
- Prefer small, focused PRs with a clear description and test notes.
