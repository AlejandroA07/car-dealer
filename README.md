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

## Deployment (Oracle Cloud Always Free VM)

This project is easiest to deploy as-is on a single Linux VM using Docker Compose. The recommended setup exposes only the `web` service publicly over HTTP on port 80.

### 1) Create the VM

- Create an Oracle Cloud “Always Free” Linux VM (Ampere A1 is a good choice).
- Open inbound ports:
  - `22/tcp` (SSH) from your IP
  - `80/tcp` (HTTP) from anywhere

### 2) SSH in and install Docker

Example (Ubuntu):
```bash
sudo apt-get update
sudo apt-get install -y git docker.io docker-compose-plugin
sudo usermod -aG docker $USER
exit
```

SSH back in so group permissions apply.

### 3) Clone the repo on the VM

```bash
git clone git@github.com:AlejandroA07/car-dealer.git
cd car-dealer
```

### 4) Create production secrets (never commit)

Create `.env` in the repo root:
```bash
MYSQL_PASSWORD=change-me
JWT_SECRET=change-me-to-a-long-random-value
ADMIN_PASSWORD=change-me
```

Create `prod_db_password.txt` (used by `docker-compose.prod.yml`) and set it to the same value as `MYSQL_PASSWORD`:
```bash
echo "change-me" > prod_db_password.txt
```

Create the shared data-protection key ring directory on the VM (host-side):
```bash
mkdir -p dpkeys
```

### 5) Start the stack (production-ish)

`docker-compose.deploy.yml` exposes only the Web UI on port 80 and keeps the APIs private to the Docker network.

```bash
docker compose --env-file .env \
  -f docker-compose.yml \
  -f docker-compose.prod.yml \
  -f docker-compose.deploy.yml \
  up -d --build
```

### 6) Verify

```bash
docker compose ps
docker compose logs -f web
```

Open:
- `http://<YOUR_VM_PUBLIC_IP>/`

## Deployment (Railway)

Railway can host the **entire stack** (microservices + MySQL) in one project:

- `web` (public)
- `api` (private)
- `auth-api` (private)
- `mysql-api` (MySQL for `api`)
- `mysql-auth` (MySQL for `auth-api`)

Railway provides private networking between services. Every service gets an internal DNS name like `api.railway.internal` for service-to-service HTTP calls.

### 1) Create a Railway project + databases

1. Create a new Railway project from this GitHub repo.
2. Add two MySQL services in the project:
   - Name one `mysql-api`
   - Name one `mysql-auth`

### 2) Create the microservices as Docker services

Create three services from the same repo and set each to build from a different Dockerfile:

- `web` (Dockerfile: `westcoast-cars.web/Dockerfile`)
- `api` (Dockerfile: `westcoast-cars.api/Dockerfile`)
- `auth-api` (Dockerfile: `westcoast-cars.auth/Api/Dockerfile`)

Recommended: make only `web` publicly reachable and keep `api` + `auth-api` private (they’re still reachable from `web` via Railway private DNS).

### 3) Configure environment variables

Set these variables on each service:

**`web`**
- `PORT=8080`
- `ASPNETCORE_URLS=http://0.0.0.0:8080`
- `Services__ApiUrl=http://api.railway.internal:8080`
- `Services__AuthUrl=http://auth-api.railway.internal:8080`

**`api`**
- `PORT=8080`
- `ASPNETCORE_URLS=http://0.0.0.0:8080`
- `JwtSettings__Secret=<generate a strong random value>`
- `ConnectionStrings__DefaultConnection=Server=<mysql-api host>;Port=<mysql-api port>;Database=<mysql-api database>;Uid=<mysql-api user>;Pwd=<mysql-api password>;`

**`auth-api`**
- `PORT=8080`
- `ASPNETCORE_URLS=http://0.0.0.0:8080`
- `JwtSettings__Secret=<same value as api>`
- `AdminSettings__Password=<choose a strong password>`
- `ConnectionStrings__DefaultConnection=Server=<mysql-auth host>;Port=<mysql-auth port>;Database=<mysql-auth database>;Uid=<mysql-auth user>;Pwd=<mysql-auth password>;`

Tip: Railway MySQL services expose `MYSQLHOST`, `MYSQLPORT`, `MYSQLUSER`, `MYSQLPASSWORD`, `MYSQLDATABASE`, and `MYSQL_URL` variables that you can copy into the connection string.

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
