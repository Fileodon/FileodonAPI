using Controller.Controllers.Authorization;
using Controller.Extensions;
using Controller.Middleware;
using Controller.Utility;
using Controller.Validators.SFTPConnectionModels;
using Domain.Interface;
using Domain.Service;
using Domain.Services;
using FluentValidation;
using Infrastructure;
using Infrastructure.Interface;
using Infrastructure.Interface.Repository;
using Infrastructure.Models;
using Infrastructure.Repository;
using Infrastructure.Service;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Serilog;
using Infrastructure.Interceptors;
using Infrastructure.Services;
using Infrastructure.Interface.Service;
using SharpGrip.FluentValidation.AutoValidation.Mvc.Extensions;

const string PolicyName = "default";
const string CorsPolicyName = "AllowListed";

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(options =>
{
    options.AddServerHeader = false;
});

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration).CreateLogger();
builder.Services.AddSerilog();

string filepath = Path.Combine(Directory.GetCurrentDirectory(), ".env");
if (File.Exists(filepath))
    DotEnv.Load(filepath);
else
    DotEnv.CreateEnvFile(filepath);

builder.Configuration.AddEnvironmentVariables();

string? keyPath = builder.Configuration["Encryption:KeyPath"];
if (string.IsNullOrEmpty(keyPath))
{
    Log.Error("Encryption key path is not set in configuration.");
    throw new InvalidOperationException("Encryption key path is not set in configuration.");
}

if (!File.Exists(keyPath))
{
    Log.Warning("Encryption key file not found at {KeyPath}. Generating a new key.", keyPath);
    CryptoService.CreateKey(keyPath);
}

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

string[] allowedOrigins = [.. (builder.Configuration.GetValue<string>("CORS_ALLOWED_HOSTS") ?? string.Empty)
    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
    .Select(origin => origin.Trim('"', '\''))
    .Where(origin => origin.Length > 0)];
Log.Information("Allowed origins for CORS: {AllowedOrigins}", string.Join(", ", allowedOrigins));
builder.Services.AddCors(options =>
{
    options.AddPolicy(CorsPolicyName,
                     policy =>
                     {
                         policy.WithOrigins(allowedOrigins)
                               .AllowAnyHeader()
                               .AllowAnyMethod()
                               .AllowCredentials();
                     }
    );
});

builder.Services.AddHttpContextAccessor();

builder.Services.AddScoped<RepositoryTransactionSettings>();
builder.Services.AddScoped<IRepositoryTransactionSettings>(services =>
    services.GetRequiredService<RepositoryTransactionSettings>());
builder.Services.AddScoped<IOrganizationRepository, OrganizationRepository>();
builder.Services.AddScoped<IProfileRepository, ProfileRepository>();
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
builder.Services.AddScoped<IRoleRepository, RoleRepository>();
builder.Services.AddScoped<ISFTPConnectionRepository, SFTPConnectionRepository>();
builder.Services.AddScoped<ISFTPConnectionAuthCodeRepository, SFTPConnectionAuthCodeRepository>();
builder.Services.AddScoped<IUserOrganizationRepository, UserOrganizationRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IAuditLogRepository, AuditLogRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<ISFTPConnectionService, SFTPConnectionService>();
builder.Services.AddScoped<IRoleService, RoleService>();
builder.Services.AddScoped<IAuthorizationPermissionService, AuthorizationPermissionService>();
builder.Services.AddScoped<IOrganizationService, OrganizationService>();
builder.Services.AddScoped<IPermissionService, PermissionService>();
builder.Services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();
builder.Services.AddScoped<IUserOrganizationService, UserOrganizationService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IOrganizationService, OrganizationService>();
builder.Services.AddScoped<IPermissionService, PermissionService>();
builder.Services.AddScoped<IPermissionRepository, PermissionRepository>();
builder.Services.AddScoped<IUserContextService, UserContextService>();
builder.Services.AddScoped<IAuditLogService, AuditLogService>();


builder.Services.AddValidatorsFromAssemblyContaining<SFTPConnectionCreateRequestValidator>();
builder.Services.AddFluentValidationAutoValidation();

// Add CryptoService as a singleton, ensuring that the key is loaded from configuration or a default path
builder.Services.AddSingleton<ICryptoService>(services =>
{
    return new CryptoService(keyPath);
});

builder.Services.AddIdentityCore<User>(options =>
{
    IConfigurationSection passwordPolicy = builder.Configuration.GetSection("PasswordPolicy");
    options.Password.RequireDigit = passwordPolicy.GetValue<bool>(nameof(options.Password.RequireDigit));
    options.Password.RequiredLength = passwordPolicy.GetValue<int>(nameof(options.Password.RequiredLength));
    options.Password.RequireUppercase = passwordPolicy.GetValue<bool>(nameof(options.Password.RequireUppercase));
    options.Password.RequireLowercase = passwordPolicy.GetValue<bool>(nameof(options.Password.RequireLowercase));
    options.Password.RequireNonAlphanumeric = passwordPolicy.GetValue<bool>(nameof(options.Password.RequireNonAlphanumeric));
    options.User.RequireUniqueEmail = passwordPolicy.GetValue<bool>(nameof(options.User.RequireUniqueEmail));
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
    options.OperationFilter<PollStatusExamplesOperationFilter>();
});

builder.Services.Configure<RouteOptions>(options =>
{
    options.LowercaseUrls = true;
    options.LowercaseQueryStrings = true;
});

builder.Services.AddScoped<AuditLogInterceptor>();

builder.Services.AddDbContext<ApplicationDbContext>(
    (sp,options) => {
        options.UseNpgsql(connectionString);
        options.AddInterceptors(sp.GetRequiredService<AuditLogInterceptor>());
    }
);

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

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseCors(CorsPolicyName);

app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();

app.MapControllers()
    .RequireRateLimiting(PolicyName);

app.Run();
