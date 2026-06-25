# Random & Time API

A minimal web application exposing four REST endpoints and an Angular UI with four buttons. Two endpoints generate values (random number, current server time) and persist attempts to separate databases; two return the most recent 50 attempts of each type.

## What Was Built

- **Backend**: .NET 10 ASP.NET Core Minimal API (`Api/`) with two services:
  - `RandomService`: Generates random numbers, persists to SQLite (`random.db`)
  - `TimeService`: Captures server time (UTC), persists to PostgreSQL
- **Frontend**: Angular 20 app (`web/`) with Angular Material components — four buttons invoking the endpoints and displaying results
- **Databases**:
  - SQLite: Embedded file (random.db), no external service needed
  - PostgreSQL: External Docker service (postgres:16, timedb database)

## Stack

- **Language**: C# 10, TypeScript
- **Backend Framework**: ASP.NET Core Minimal APIs
- **ORM**: Entity Framework Core (Sqlite provider + Npgsql provider)
- **Frontend**: Angular 20 with Angular Material 21
- **HTTP Port**: 5000 (fixed; frontend proxy targets this)

## Installation

### Prerequisites

- Docker and Docker Compose (or .NET 10 SDK + Node.js 20+ for local development)
- Environment variables (for the app, not the build):
  - `ASPNETCORE_ENVIRONMENT`: Set to `Production` in Docker (default for the compose setup)
  - `ConnectionStrings__Default`: PostgreSQL connection string; injected by docker-compose for the db service; for local dev, configure in `Api/appsettings.json`

### Via Docker Compose (Recommended)

```bash
cd workspace
wsl docker compose up --build --detach
```

The app will be available at `http://localhost:5000`. The docker-compose setup:
- Builds the .NET app (publishes, copies to a runtime-only image)
- Starts a PostgreSQL service (postgres:16, database=timedb, user=postgres, password=password)
- Injects the PostgreSQL connection string via environment variable

The API is automatically served from the app container's wwwroot (built Angular dist).

### Local Development

**Backend**:
```bash
cd workspace/Api
dotnet restore
dotnet build
dotnet run
```

The API will listen on `http://localhost:5000`.

**Frontend**:
```bash
cd workspace/web
npm install
npx ng serve --port 4200
```

The Angular dev server will run on `http://localhost:4200` and proxy `/api` calls to `http://localhost:5000` via `proxy.conf.json`.

**Database Setup** (local dev):
- SQLite: Automatically created at `workspace/Api/random.db` when the app first runs
- PostgreSQL: Install Postgres locally or use Docker:
  ```bash
  wsl docker run -d -e POSTGRES_DB=timedb -e POSTGRES_USER=postgres -e POSTGRES_PASSWORD=password -p 5432:5432 postgres:16
  ```
  Then update `Api/appsettings.json` with the PostgreSQL connection string if not using the default localhost:5432.

## Running Tests

```bash
cd workspace/Api
dotnet test --nologo --logger "console;verbosity=minimal"
```

Tests run against in-memory SQLite and EF InMemory (no database service required). 14 tests cover the four endpoints and acceptance criteria.

## API Endpoints

All endpoints are GET requests returning JSON:

- `GET /api/random` — Generates a random number, persists to SQLite, returns `{ id, value, createdAt }` (createdAt in ISO-8601 UTC)
- `GET /api/random/history` — Returns at most 50 prior random attempts from SQLite, ordered most-recent first; empty array if none
- `GET /api/now` — Captures current server time (UTC), persists to PostgreSQL, returns `{ id, serverTimeUtc }` (ISO-8601 UTC)
- `GET /api/now/history` — Returns at most 50 prior time attempts from PostgreSQL, ordered most-recent first; empty array if none

## UI

Four buttons on a single Angular Material page:
- **get-random**: Calls `/api/random` and displays the returned value
- **get-random-history**: Calls `/api/random/history` and displays the list in a table; shows an empty-state message if no records
- **get-now**: Calls `/api/now` and displays the server time
- **get-now-history**: Calls `/api/now/history` and displays the list in a table; shows an empty-state message if no records

Error messages (from failed API calls) are displayed below each button's result area.

## Environment Variables (Docker)

When running via docker-compose, the following are set automatically:

- `ASPNETCORE_ENVIRONMENT=Production`
- `ConnectionStrings__Default=Host=db;Port=5432;Database=timedb;Username=postgres;Password=password`

For custom deployments, override `ConnectionStrings__Default` to point to your PostgreSQL instance.

## Notes

- No authentication required — the app is a demo with no sensitive data
- SQLite file (random.db) is created in the app's working directory (in Docker, `/app/random.db`)
- PostgreSQL tables are auto-created on first run via `Database.EnsureCreated()`
- CORS is permissive in development (allows requests from any origin) and restricted to `http://localhost:4200` in production
