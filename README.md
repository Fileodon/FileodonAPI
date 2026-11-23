# Fileodon API

The Fileodon API is an ASP.NET Core Web API for managing users, organizations, roles,
permissions, SFTP connections, file operations, and audit logs. It targets **.NET 10**,
uses **PostgreSQL** with Entity Framework Core, and exposes an OpenAPI/Swagger UI for
development.

This README is the quickest path from a fresh checkout to a running local API.

## What you need

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [PostgreSQL 14 or newer](https://www.postgresql.org/download/)
- [Git](https://git-scm.com/downloads)
- Optional: [Docker Desktop](https://www.docker.com/products/docker-desktop/) for Mailpit

The solution also contains a `Test` project with xUnit tests. You do not need a separate
frontend to run or explore the API.

## Quick start

### 1. Clone the repository

```bash
git clone https://github.com/Fileodon/FileodonAPI.git
cd FileodonAPI
```

### 2. Create a PostgreSQL database

Create a database for local development. For example, from `psql`:

```sql
CREATE DATABASE fileodon;
```

The PostgreSQL server must be running and the user in your connection settings must have
access to this database.

### 3. Configure the API

Copy the example environment file into the API's data directory. The application reads
this file from `Controller/data/.env`:

Windows PowerShell:

```powershell
Copy-Item Controller\.env.example Controller\data\.env
```

macOS/Linux:

```bash
cp Controller/.env.example Controller/data/.env
```

Open `Controller/data/.env` and set the database values:

```env
DB__HOST=localhost
DB__PORT=5432
DB__USER=postgres
DB__PASSWORD=your-postgres-password
DB__NAME=fileodon
```

The double underscore (`__`) maps to the nested ASP.NET Core configuration key. For
example, `DB__HOST` becomes `DB:HOST`.

The application creates `Controller/data` and its encryption, JWT, SFTP PAM, and log
files automatically on first startup. These files are local runtime data and should not
be committed.

> **Alternative:** You can provide a complete connection string as
> `ConnectionStrings__DefaultConnection` instead of the `DB__*` values.

### 4. Restore, build, and run

From the repository root:

```bash
dotnet restore
dotnet build
dotnet run --project Controller
```

The API checks the database connection and applies pending EF Core migrations
automatically during startup.

When using the launch profile, the development URLs are:

- Swagger UI: `https://localhost:7248/api/`
- HTTP Swagger UI: `http://localhost:5283/api/`
- Health check: `https://localhost:7248/api/system/health`

The HTTPS development certificate may need to be trusted once:

```bash
dotnet dev-certs https --trust
```

## Optional email setup with Mailpit

Email confirmation and password-reset flows require an SMTP server. Mailpit provides a
local inbox without sending real email:

```bash
docker compose up -d mailpit
```

Add these values to `Controller/data/.env`:

```env
SMTP__HOST=localhost
SMTP__PORT=1025
SMTP__STARTTLS=false
SMTP__USER=
SMTP__PASSWORD=
SMTP__FROM_ADDRESS=no-reply@localhost
```

Open the Mailpit inbox at <http://localhost:8025>.

`docker-compose.yml` only starts Mailpit; PostgreSQL is expected to be installed and
running separately.

## Configuration

The most useful local settings are:

| Variable | Purpose |
| --- | --- |
| `DB__HOST` | PostgreSQL host |
| `DB__PORT` | PostgreSQL port |
| `DB__USER` | PostgreSQL user |
| `DB__PASSWORD` | PostgreSQL password |
| `DB__NAME` | PostgreSQL database |
| `API__HOST` | Public API URL used in generated links |
| `CORS__ALLOWED_HOSTS` | Comma-separated frontend origins |
| `SMTP__*` | SMTP settings for confirmation and reset emails |
| `SFTPPamJwt__KeyId` | Identifier for the SFTP PAM signing key |
| `SFTPIntermediary__Port` | Legacy fallback distribution listener port (new connections store this port per SFTP connection) |

Use environment variables or a local secrets provider for credentials. Do not put
passwords, tokens, or private keys in source control.

## Database migrations

Normal application startup applies all existing migrations automatically. To manage
migrations while developing the schema, install the EF Core CLI once:

```bash
dotnet tool install --global dotnet-ef
```

Create a migration:

```bash
dotnet ef migrations add <MigrationName> `
  --project Infrastructure `
  --startup-project Controller
```

On macOS/Linux, use `\` instead of PowerShell's backtick for line continuation, or put
the command on one line:

```bash
dotnet ef migrations add <MigrationName> --project Infrastructure --startup-project Controller
```

Apply migrations manually when needed:

```bash
dotnet ef database update --project Infrastructure --startup-project Controller
```

Windows contributors can also use:

```bat
Scripts\create_migration.bat
Scripts\apply_migrations.bat
```

## Running tests

Run the complete test suite from the repository root:

```bash
dotnet test
```

Tests use SQLite in-memory databases where appropriate and do not require your local
PostgreSQL database.

## Docker

Build the API image from the repository root:

```bash
docker build -f Controller/Dockerfile -t fileodon-api .
docker run --rm -p 8080:8080 fileodon-api
```

The container listens on port `8080`. Provide the database configuration as environment
variables when starting it, for example:

```bash
docker run --rm -p 8080:8080 `
  -e DB__HOST=host.docker.internal `
  -e DB__PORT=5432 `
  -e DB__USER=postgres `
  -e DB__PASSWORD=your-postgres-password `
  -e DB__NAME=fileodon `
  fileodon-api
```

Use `\` instead of backticks on macOS/Linux. Mount `/app/data` in a persistent
deployment so generated keys and logs survive container replacement.

## Project layout

```text
FileodonAPI.sln
├── Controller/       API host, controllers, middleware, configuration
├── Domain/           Domain services, DTOs, interfaces, and workers
├── Infrastructure/   EF Core DbContext, repositories, and migrations
├── Test/             xUnit tests
├── Scripts/          Windows migration helpers
└── docker-compose.yml
```

## Common troubleshooting

### The API says database settings are missing

Verify that the file is exactly `Controller/data/.env`, that all five `DB__*` values
are set, and that the API is started from the repository root. You can also use
`ConnectionStrings__DefaultConnection`.

### The API cannot connect to PostgreSQL

Check that PostgreSQL is running, the port is correct, the database exists, and the
configured user can log in. A rejected connection prevents the API from starting.

### Registration emails do not arrive

Start Mailpit, configure the `SMTP__*` values above, and check <http://localhost:8025>.
For a real SMTP provider, replace the Mailpit settings with that provider's host, port,
TLS, credentials, and sender address.

### Swagger does not load

Use the `/api/` URL in Development and confirm that the process is still running. The
OpenAPI document is available at `/api/openapi/v1.json` in Development.

## Contributing

Before opening a change, run:

```bash
dotnet build
dotnet test
```

Keep generated data, logs, credentials, and key files out of commits. Add database
migrations when changing the EF Core model, and verify that a clean checkout can restore,
build, and start with the documented configuration.

## License

TBD
