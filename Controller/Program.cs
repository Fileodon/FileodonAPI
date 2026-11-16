#region Using Directives

using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Channels;
using Controller.Controllers.Authorization;
using Controller.Extensions;
using Controller.Middleware;
using Controller.Utility;
using Controller.Validators.SFTPConnectionModels;
using Domain.Interface;
using Domain.Service;
using Domain.Services;
using Domain.Worker;
using FluentValidation;
using Infrastructure;
using Infrastructure.Interceptors;
using Infrastructure.Interface;
using Infrastructure.Interface.Repository;
using Infrastructure.Interface.Repository.SFTP;
using Infrastructure.Interface.Service;
using Infrastructure.Models;
using Infrastructure.Repository;
using Infrastructure.Repository.SFTP;
using Infrastructure.Service;
using Infrastructure.Services;
using Infrastructure.Worker;
using MicroElements.AspNetCore.OpenApi.FluentValidation;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Serilog;
using SharpGrip.FluentValidation.AutoValidation.Mvc.Extensions;

#endregion
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

if (string.IsNullOrEmpty(builder.Configuration["JWT:KEY"]))
{
    Log.Warning("JWT key is not set in configuration. Generating a new secret.");
    string key = await TokenService.GenerateSecret();
    if (string.IsNullOrEmpty(key))
    {
        Log.Error("Failed to generate JWT secret.");
        throw new InvalidOperationException("Failed to generate JWT secret.");
    }
    DotEnv.WriteEntry("JWT:KEY", key);
    builder.Configuration["JWT:KEY"] = key;
}
#region CORS
string[] allowedOrigins = [.. (builder.Configuration.GetValue<string>("CORS:ALLOWED_HOSTS") ?? string.Empty)
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
#endregion
builder.Services.AddHttpContextAccessor();

#region Standard Services
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
builder.Services.AddScoped<ISFTPDistributionJobRepository, SFTPDistributionJobRepository>();
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
builder.Services.AddScoped<ISFTPActionService, SFTPActionService>();
builder.Services.AddScoped<IAuthorizationHandler, EmailVerifiedHandler>();
builder.Services.AddSingleton<IAuthorizationMiddlewareResultHandler, VerifiedEmailResultHandler>();
#endregion
#region FluentValidation
builder.Services.AddValidatorsFromAssemblyContaining<SFTPConnectionCreateRequestValidator>();
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddFluentValidationRulesToOpenApi();
#endregion
#region OpenAPI
builder.Services.AddOpenApi(options =>
{
    options.AddFluentValidationRules();
    options.AddDocumentTransformer<BearerSecuritySchemeTransformer>();
    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        if (builder.Environment.IsDevelopment())
        {
            document.Servers = [new OpenApiServer { Url = "/api" }];
        }

        return Task.CompletedTask;
    });
    options.AddOperationTransformer<PollStatusExamplesOperationTransformer>();
    options.AddSchemaTransformer((schema, context, ct) =>
    {
        if (context.JsonTypeInfo.Type == typeof(int))
        {
            schema.Type = JsonSchemaType.Integer;
            schema.Pattern = null;
        }
        return Task.CompletedTask;
    });
});
#endregion
#region Email Services and Worker
Channel<EmailWorkItem> channel =
    Channel.CreateBounded<EmailWorkItem>(new BoundedChannelOptions(1000)
    {
        FullMode = BoundedChannelFullMode.DropWrite,
        SingleReader = true,
        SingleWriter = true,
    });

builder.Services.AddSingleton(channel);
builder.Services.AddSingleton<IEmailQueue, EmailQueue>();
builder.Services.AddScoped<ISmtpEmailSender, SmtpEmailSender>();
builder.Services.AddSingleton<IEmailTransport, EmailTransport>();
builder.Services.AddHostedService<EmailWorker>();
#endregion
#region SFTP Upload Services and Worker
Channel<SFTPUploadWorkItem> sftpUploadChannel =
    Channel.CreateBounded<SFTPUploadWorkItem>(new BoundedChannelOptions(1000)
    {
        FullMode = BoundedChannelFullMode.Wait,
        SingleReader = true,
        SingleWriter = false,
    });

builder.Services.AddSingleton(sftpUploadChannel);
builder.Services.AddSingleton<ISFTPUploadJobStore, SFTPUploadJobStore>();
builder.Services.AddSingleton<ISFTPUploadQueue, SFTPUploadQueue>();
builder.Services.AddScoped<ISFTPUploadSender, SFTPUploadSender>();
builder.Services.AddSingleton<ISFTPRepository, SFTPRepository>();
builder.Services.AddHostedService<SFTPUploadWorker>();
#endregion

// Add CryptoService as a singleton, ensuring that the key is loaded from configuration or a default path
builder.Services.AddSingleton<ICryptoService>(services =>
{
    return new CryptoService(keyPath);
});

#region Identity and Authentication
builder.Services.AddIdentityCore<User>(options =>
{
    // Password policy
    IConfigurationSection passwordPolicy = builder.Configuration.GetSection("PasswordPolicy");
    options.Password.RequireDigit = passwordPolicy.GetValue<bool>(nameof(options.Password.RequireDigit));
    options.Password.RequiredLength = passwordPolicy.GetValue<int>(nameof(options.Password.RequiredLength));
    options.Password.RequireUppercase = passwordPolicy.GetValue<bool>(nameof(options.Password.RequireUppercase));
    options.Password.RequireLowercase = passwordPolicy.GetValue<bool>(nameof(options.Password.RequireLowercase));
    options.Password.RequireNonAlphanumeric = passwordPolicy.GetValue<bool>(nameof(options.Password.RequireNonAlphanumeric));

    // User and SignIn policy
    options.User.RequireUniqueEmail = passwordPolicy.GetValue<bool>(nameof(options.User.RequireUniqueEmail));
    options.SignIn.RequireConfirmedEmail = passwordPolicy.GetValue<bool>(nameof(options.SignIn.RequireConfirmedEmail));
})
    .AddRoles<AspNetRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();

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
            IssuerSigningKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(builder.Configuration["JWT:KEY"] ?? throw new InvalidOperationException("JWT key is not set in configuration."))),
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    });
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("EmailVerified", policy =>
    {
        policy.RequireAuthenticatedUser()
            .AddRequirements(new EmailVerifiedRequirement());
    });
});
#endregion

builder.Services.AddSingleton<
    IAuthorizationPolicyProvider,
    PermissionPolicyProvider>();

builder.Services.AddScoped<
    IAuthorizationHandler,
    Handler>();

builder.Services.AddRateLimiter(options =>
{
    options.AddPolicy<string, RateLimit>(PolicyName);
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
});

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    });
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddEndpointsApiExplorer();

builder.Services.Configure<RouteOptions>(options =>
{
    options.LowercaseUrls = true;
    options.LowercaseQueryStrings = true;
});

builder.Services.AddScoped<AuditLogInterceptor>();

builder.Services.AddDbContext<ApplicationDbContext>(
    (sp, options) =>
    {
        options.UseNpgsql(connectionString);
        options.UseSnakeCaseNamingConvention();
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
if (app.Environment.IsDevelopment())
{
    // UsePathBase only adds an alternative url, not replacing
    app.UsePathBase("/api");
    // Enforce that /api is the base path from the request
    app.Use(async (context, next) =>
    {
        if (context.Request.PathBase != "/api" && context.Request.PathBase != "")
        {
            context.Response.StatusCode = StatusCodes.Status404NotFound;
            return;
        }

        await next();
    });
}

app.MapOpenApi();
app.UseSwaggerUI(options =>
{
    if (app.Environment.IsDevelopment())
    {
        options.SwaggerEndpoint("/api/openapi/v1.json", "v1");
    }
    else
    {
        options.SwaggerEndpoint("/openapi/v1.json", "v1");
    }
    options.RoutePrefix = string.Empty;
});

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseCors(CorsPolicyName);
app.UseRouting();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.UseRouting();
app.UseRateLimiter();
app.MapControllers().RequireRateLimiting(PolicyName);

app.Run();

internal sealed class BearerSecuritySchemeTransformer(IAuthenticationSchemeProvider authenticationSchemeProvider) : IOpenApiDocumentTransformer
{
    public async Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
    {
        var authenticationSchemes = await authenticationSchemeProvider.GetAllSchemesAsync();
        if (authenticationSchemes.Any(authScheme => authScheme.Name == "Bearer"))
        {
            // Add the security scheme at the document level
            var securitySchemes = new Dictionary<string, IOpenApiSecurityScheme>
            {
                ["Bearer"] = new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer", // "bearer" refers to the header name here
                    In = ParameterLocation.Header,
                    BearerFormat = "Json Web Token"
                }
            };
            document.Components ??= new OpenApiComponents();
            document.Components.SecuritySchemes = securitySchemes;

            // Apply it as a requirement for all operations
            foreach (var operation in document.Paths.Values.SelectMany(path => path.Operations ?? []))
            {
                operation.Value.Security ??= [];
                operation.Value.Security.Add(new OpenApiSecurityRequirement
                {
                    [new OpenApiSecuritySchemeReference("Bearer", document)] = []
                });
            }
        }
    }
}