using Controller.Utility;
using Microsoft.AspNetCore.Builder;

namespace Test;

[Collection("Environment variables")]
public sealed class DatabaseTests
{
    private static readonly string[] RequiredEnvironmentVariables =
    [
        "DB_HOST",
        "DB_PORT",
        "DB_USER",
        "DB_PASSWORD",
        "DB_NAME"
    ];

    [Fact]
    public void Configured_connection_string_takes_precedence()
    {
        WebApplicationBuilder builder = CreateBuilder(
            connectionString: "Host=configured;Database=app");

        string result = Database.InitializeConnectionString(builder);

        Assert.Equal("Host=configured;Database=app", result);
    }

    [Fact]
    public void Builds_connection_string_from_environment_configuration()
    {
        WebApplicationBuilder builder = CreateBuilder();
        SetDatabaseConfiguration(builder, "database.example", "5432", "fileodon", "test-password", "fileodon");

        string result = Database.InitializeConnectionString(builder);

        Assert.Equal(
            "Host=database.example;Port=5432;Username=fileodon;Password=test-password;Database=fileodon",
            result);
    }

    [Fact]
    public void Reports_all_missing_environment_variables()
    {
        WebApplicationBuilder builder = CreateBuilder();
        builder.Configuration["DB_HOST"] = "database.example";
        builder.Configuration["DB_PORT"] = "5432";

        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(
            () => Database.InitializeConnectionString(builder));

        Assert.Contains("DB_USER", exception.Message);
        Assert.Contains("DB_PASSWORD", exception.Message);
        Assert.Contains("DB_NAME", exception.Message);
    }

    [Fact]
    public void Treats_empty_environment_values_as_missing()
    {
        WebApplicationBuilder builder = CreateBuilder();
        SetDatabaseConfiguration(builder, "", "5432", "fileodon", "test-password", "fileodon");

        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(
            () => Database.InitializeConnectionString(builder));

        Assert.Contains("DB_HOST", exception.Message);
    }

    private static WebApplicationBuilder CreateBuilder(string? connectionString = null)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder(
            new WebApplicationOptions
            {
                EnvironmentName = "Testing",
                ApplicationName = typeof(DatabaseTests).Assembly.GetName().Name
            });

        if (connectionString is not null)
        {
            builder.Configuration["ConnectionStrings:DefaultConnection"] = connectionString;
        }

        return builder;
    }

    private static void SetDatabaseConfiguration(
        WebApplicationBuilder builder,
        string host,
        string port,
        string user,
        string password,
        string database)
    {
        builder.Configuration["DB_HOST"] = host;
        builder.Configuration["DB_PORT"] = port;
        builder.Configuration["DB_USER"] = user;
        builder.Configuration["DB_PASSWORD"] = password;
        builder.Configuration["DB_NAME"] = database;
    }
}

[CollectionDefinition("Environment variables", DisableParallelization = true)]
public sealed class EnvironmentVariablesCollection;
