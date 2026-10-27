using Controller.Middleware;
using Controller.Utility;
using Domain.Interface;
using Domain.Service;
using Infrastructure;
using Infrastructure.Interface;
using Infrastructure.Interface.Repository;
using Infrastructure.Models;
using Infrastructure.Repository;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration).CreateLogger();
builder.Services.AddSerilog();

string filepath = Path.Combine(Directory.GetCurrentDirectory(), ".env");
if (File.Exists(filepath))
    DotEnv.Load(filepath);
else
    DotEnv.CreateEnvFile(filepath);

builder.Configuration.AddEnvironmentVariables();

string connectionString = Database.InitializeConnectionString(builder);

if (string.IsNullOrEmpty(builder.Configuration["JWT_KEY"]))
{
    Log.Warning("JWT key is not set in configuration. Generating a new secret.");
    string key = await TokenService.GenerateSecret();
    DotEnv.WriteEntryAndUpdateEnvironmentVariable("JWT_KEY", key);
    if (string.IsNullOrEmpty(builder.Configuration["JWT_KEY"]))
    {
        Log.Error("Failed to set JWT key in configuration after generating a new secret.");
        throw new InvalidOperationException("Failed to set JWT key in configuration after generating a new secret.");
    }
}

builder.Services.AddDbContext<ApplicationDbContext>(
    options => options.UseNpgsql(connectionString)
);
builder.Services.AddScoped<RepositoryTransactionSettings>();
builder.Services.AddScoped<IRepositoryTransactionSettings>(services =>
    services.GetRequiredService<RepositoryTransactionSettings>());
builder.Services.AddScoped<IAuditLogRepository, AuditLogRepository>();
builder.Services.AddScoped<IOrganizationRepository, OrganizationRepository>();
builder.Services.AddScoped<IProfileRepository, ProfileRepository>();
builder.Services.AddScoped<IRoleRepository, RoleRepository>();
builder.Services.AddScoped<ISFTPConnectionRepository, SFTPConnectionRepository>();
builder.Services.AddScoped<IUserOrganizationRepository, UserOrganizationRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IRoleService, RoleService>();

builder.Services.AddAuthorization();
builder.Services.AddIdentityApiEndpoints<User>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 8;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = true;
})
    .AddRoles<AspNetRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddSignInManager();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(builder.Configuration["JWT_KEY"] ?? throw new InvalidOperationException("JWT key is not set in configuration."))),
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    });
builder.Services.AddAuthorization();

const string PolicyName = "default";

builder.Services.AddRateLimiter(options =>
{
    options.AddPolicy<string, RateLimit>(PolicyName);
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
});

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

builder.Services.Configure<RouteOptions>(options =>
{
    options.LowercaseUrls = true;
});

var app = builder.Build();

if (!Database.CanConnectToDb(app))
{
    Log.Error("Cannot connect to the database. Please check your connection string and database server.");
    throw new InvalidOperationException("Cannot connect to the database. Please check your connection string and database server.");
}

Database.ApplyMigrations(app);

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
    options.RoutePrefix = string.Empty;
});

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();

app.MapControllers()
    .RequireRateLimiting(PolicyName);

app.Run();
