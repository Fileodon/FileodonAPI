using Infrastructure;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace Controller.Utility;

/// <summary>
/// This class is responsible for initializing the database connection string from either the appsettings.json configuration or environment variables.
/// </summary>
public static class Database
{
    /// <summary>
    /// Initializes the database connection string. It first checks the appsettings.json configuration for a connection string named "DefaultConnection". If not found, it attempts to load the connection string from environment variables. If any required environment variables are missing, it throws an InvalidOperationException.
    /// </summary>
    /// <param name="builder">The WebApplicationBuilder instance used to access configuration and environment variables.</param>
    /// <returns>The database connection string.</returns>
    /// <exception cref="InvalidOperationException">Thrown when required environment variables are missing or not set.</exception>
    public static string InitializeConnectionString(WebApplicationBuilder builder)
    {
        string? connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrEmpty(connectionString))
        {
            Log.Information("Connection string not found in configuration. Attempting to load from environment variables.");
            string filepath = Path.Combine(Directory.GetCurrentDirectory(), ".env");
            if (File.Exists(filepath))
            {
                Log.Information($"Loading environment variables from {filepath}");
                DotEnv.Load(filepath);
            }

            builder.Configuration.AddEnvironmentVariables();

            List<string> missingEnvVars = [];
            foreach (string envVar in (List<string>)["DB_HOST", "DB_PORT", "DB_USER", "DB_PASSWORD", "DB_NAME"])
            {
                if (string.IsNullOrEmpty(builder.Configuration[envVar]))
                {
                    missingEnvVars.Add(envVar);
                }
            }

            if (missingEnvVars.Count > 0)
            {
                throw new InvalidOperationException($"Missing required environment variables: {string.Join(", ", missingEnvVars)}");
            }

            string dbHost = builder.Configuration["DB_HOST"] ?? throw new InvalidOperationException("DB_HOST environment variable is not set.");
            string dbPort = builder.Configuration["DB_PORT"] ?? throw new InvalidOperationException("DB_PORT environment variable is not set.");
            string dbUser = builder.Configuration["DB_USER"] ?? throw new InvalidOperationException("DB_USER environment variable is not set.");
            string dbPassword = builder.Configuration["DB_PASSWORD"] ?? throw new InvalidOperationException("DB_PASSWORD environment variable is not set.");
            string dbName = builder.Configuration["DB_NAME"] ?? throw new InvalidOperationException("DB_NAME environment variable is not set.");

            connectionString = $"Host={dbHost};Port={dbPort};Username={dbUser};Password={dbPassword};Database={dbName}";
        }

        return connectionString;
    }

    /// <summary>
    /// Checks if the application can connect to the database. This method creates a new scope, retrieves the ApplicationDbContext, and calls the CanConnect method to verify database connectivity. If an error occurs while checking connectivity, it logs the error and returns false.
    /// </summary>
    /// <param name="webapp">The WebApplication instance used to create a scope for retrieving the ApplicationDbContext.</param>
    /// <returns>True if the application can connect to the database; otherwise, false.</returns>
    public static bool CanConnectToDb(WebApplication webapp)
    {
        try
        {
            using var scope = webapp.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            return dbContext.Database.CanConnect();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error while checking database connectivity.");
            return false;
        }
    }

    /// <summary>
    /// Applies any pending database migrations. This method creates a new scope, retrieves the ApplicationDbContext, and calls the Migrate method to apply any pending migrations. If an error occurs during migration, it logs the error and rethrows the exception.
    /// </summary>
    /// <param name="webapp">The WebApplication instance used to create a scope for retrieving the ApplicationDbContext.</param>
    public static void ApplyMigrations(WebApplication webapp)
    {
        try
        {
            using var scope = webapp.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            dbContext.Database.Migrate();
            Log.Information("Database migrations applied successfully.");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error while applying database migrations.");
            throw;
        }
    }
}
