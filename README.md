# Fileodon API

Fileodon API is an ASP.NET Core Web API built on .NET 10, using PostgreSQL with Entity Framework Core and ASP.NET Core Identity for authentication and identity endpoints.

This repository is organized into a small multi-project solution:

- `Controller/` — ASP.NET Core host, configuration, middleware, Swagger, and API endpoints
- `Infrastructure/` — EF Core DbContext and persistence layer
- `Domain/` — domain model definitions

## Requirements

Before working on the project, make sure you have the following installed:

- .NET SDK 10.0+
- PostgreSQL 14+ or a compatible local instance
- A tool for managing PostgreSQL (optional but recommended: pgAdmin or psql)
- Git

## Solution layout

```text
FileodonAPI.sln
.env.example
Controller/
  Program.cs
  appsettings.json
  appsettings.Development.json
  Controller.csproj
  Utility/
    DotEnv.cs
Infrastructure/
  ApplicationDbContext.cs
  Infrastructure.csproj
Domain/
  Domain.csproj
Scripts/
  apply_migrations.bat
  create_migration.bat
```

## Quick start

1. Restore packages:

```bash
dotnet restore
```

2. Create a local environment file from the sample template:

```bash
copy .env.example .env
```

On Linux/macOS:

```bash
cp .env.example .env
```

Update the values in `.env` to match your local PostgreSQL instance.

The project includes a sample template at `.env.example`, so the user can create a local copy named `.env` without committing secrets to source control.

The API first attempts to read `ConnectionStrings:DefaultConnection` from configuration. If it is missing, it looks for a `.env` file in the current working directory and then reads environment variables from the process environment.

Example `.env` file:

```env
DB_HOST=localhost
DB_PORT=5432
DB_USER=postgres
DB_PASSWORD=postgres
DB_NAME=fileodon
```

If you prefer to use application configuration instead, add a `ConnectionStrings` section to `Controller/appsettings.Development.json` or your secrets configuration:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Username=postgres;Password=postgres;Database=fileodon"
  }
}
```

3. Run the API:

```bash
dotnet run --project Controller
```

The app will start using the ASP.NET Core development environment and expose Swagger UI in development.

## Database setup

The project uses PostgreSQL through Npgsql and the `ApplicationDbContext` in `Infrastructure/ApplicationDbContext.cs`.

### Create the database

Ensure PostgreSQL is running and the database exists:

```sql
CREATE DATABASE fileodon;
```

If your credentials differ, update the connection values accordingly.

### Apply EF Core migrations

Install the EF Core CLI if you do not already have it:

```bash
dotnet tool install --global dotnet-ef
```

Then apply migrations:

```bash
dotnet ef database update --project Infrastructure --startup-project Controller --context ApplicationDbContext
```

If the project has no migrations yet, generate one:

```bash
dotnet ef migrations add InitialCreate --project Infrastructure --startup-project Controller --context ApplicationDbContext
```

## Configuration model

The application uses ASP.NET Core configuration providers in this order:

1. `appsettings.json`
2. `appsettings.Development.json`
3. user secrets / environment-specific config
4. `.env` loaded via `DotEnv.Load(...)`
5. OS environment variables via `builder.Configuration.AddEnvironmentVariables()`

The custom `.env` loader is implemented in `Controller/Utility/DotEnv.cs` and reads simple `KEY=value` pairs.

Important notes:

- The project expects either a `DefaultConnection` connection string or individual `DB_*` environment variables.
- If those values are missing, it builds a PostgreSQL connection string using default values:
  - DB_HOST = `localhost`
  - DB_PORT = `5432`
  - DB_USER = `postgres`
  - DB_PASSWORD = `postgres`
  - DB_NAME = `postgres`

## Authentication and identity

The API is configured with ASP.NET Core Identity and generated Identity endpoints.

Core setup in `Controller/Program.cs` includes:

```csharp
builder.Services.AddAuthorization();
builder.Services.AddIdentityApiEndpoints<IdentityUser>()
    .AddEntityFrameworkStores<ApplicationDbContext>();
```

The generated identity routes are mapped in the app with:

```csharp
app.MapIdentityApi<IdentityUser>();
```

This creates standard endpoints for identity operations, such as registration and login, depending on the current ASP.NET Core Identity configuration.

## Repository transactions

Repositories automatically wrap each write operation (`PostAsync`, `PutAsync`, `UpdateAsync`, and
`DeleteAsync`) in a database transaction. If the operation fails, the transaction is rolled back
and the exception is rethrown. Read operations are not wrapped because they do not change state.

For a transaction spanning multiple repositories, use the registered `IUnitOfWork` and explicitly
commit or roll back the shared transaction:

```csharp
await unitOfWork.BeginTransactionAsync();
try
{
    await unitOfWork.OrganizationRepository.PostAsync(organization);
    await unitOfWork.ProfileRepository.PostAsync(profile);
    await unitOfWork.CommitAsync();
}
catch
{
    await unitOfWork.RollbackAsync();
    throw;
}
```

Automatic per-operation transactions can be temporarily disabled for a scope. This is useful when
the caller intentionally controls the transaction boundary itself:

```csharp
using (unitOfWork.DisableAutomaticTransactions())
{
    await unitOfWork.OrganizationRepository.PostAsync(organization);
    await unitOfWork.SaveChangesAsync();
}
```

An explicitly started unit-of-work transaction takes precedence over automatic transactions, so
repository calls inside that scope participate in the same transaction.

## Swagger / API documentation

Swagger is enabled in development mode:

```csharp
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
        options.RoutePrefix = string.Empty;
    });
}
```

Because `RoutePrefix` is set to an empty string, the Swagger UI is served at the root path during development, typically:

```text
https://localhost:<port>/
```

This makes it easier to use the UI directly without navigating to `/swagger`.

### Testing secured endpoints in Swagger

To authorize requests in Swagger UI:

1. Register or log in through the Identity endpoints.
2. Copy the JWT token.
3. In Swagger UI, click the Authorize button.
4. Use the value format:

```text
Bearer <your-token>
```

Then any endpoint marked with `[Authorize]` can be tested from Swagger.

## Development workflow

### Restore and build

```bash
dotnet restore
dotnet build
```

### Run in development mode

```bash
dotnet run --project Controller
```

### Watch for changes

```bash
dotnet watch --project Controller run
```

## Common tasks

### Add a migration

```bash
dotnet ef migrations add <MigrationName> --project Infrastructure --startup-project Controller --context ApplicationDbContext
```

If you prefer using the bundled Windows scripts, run:

```bat
Scripts\create_migration.bat
```

This script prompts for a migration name and runs the equivalent `dotnet ef` command.

### Update the database

```bash
dotnet ef database update --project Infrastructure --startup-project Controller --context ApplicationDbContext
```

Or use the helper script:

```bat
Scripts\apply_migrations.bat
```

This applies any pending migrations using the project and startup project paths already configured in the script.

### Inspect the app in Swagger

Open the application root in a browser while running in development:

```text
https://localhost:<port>/
```

### Run tests

This project does not currently include an explicit test project in the solution snapshot, so use the .NET solution-level test command when you add tests:

```bash
dotnet test
```

## Troubleshooting

### Connection string not found

If startup logs report that `DefaultConnection` is missing, check:

- `Controller/appsettings.json`
- `Controller/appsettings.Development.json`
- environment variables
- the `.env` file in the current working directory

### PostgreSQL login failures

Verify:

- PostgreSQL is running
- username/password are correct
- the `DB_NAME` database exists
- the database server is listening on the configured port

### `dotnet ef` not recognized

Install the .NET EF Core tool:

```bash
dotnet tool install --global dotnet-ef
```

Then reopen the terminal if necessary.

## Notes for contributors

- Prefer environment variables or secrets for local development credentials rather than committing production values.
- Keep sensitive configuration out of source control.
- Validate connectivity and migrations before pushing changes affecting the database schema or authentication flow.

## License

TBD
