using Controller.Utility;
using Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration).CreateLogger();
builder.Services.AddSerilog();
string connectionString = InitializeConnectionString(builder);

builder.Services.AddDbContext<ApplicationDbContext>(
    options => options.UseNpgsql(connectionString)
);

builder.Services.AddAuthorization();
builder.Services.AddIdentityApiEndpoints<IdentityUser>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 8;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = true;
})
    .AddEntityFrameworkStores<ApplicationDbContext>();

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddEndpointsApiExplorer();
// Enables authentication via SwaggerUI
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "JWT Authorization header using the Bearer scheme."
    });
    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("bearer", document)] = []
    });
});

var app = builder.Build();

if (!CanConnectToDb(app))
{
    Log.Error("Cannot connect to the database. Please check your connection string and database server.");
    throw new InvalidOperationException("Cannot connect to the database. Please check your connection string and database server.");
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
        options.RoutePrefix = string.Empty;
    });
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapGroup("/auth").MapIdentityApi<IdentityUser>();

app.Run();

static string InitializeConnectionString(WebApplicationBuilder builder)
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

static bool CanConnectToDb(WebApplication webapp)
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