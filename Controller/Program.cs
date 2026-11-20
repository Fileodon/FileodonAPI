#region Using Directives

using System.Security.Cryptography;
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
string dataDirectory = Path.Combine(builder.Environment.ContentRootPath, "data");
Directory.CreateDirectory(dataDirectory);
Directory.CreateDirectory(Path.Combine(dataDirectory, "logs"));
Environment.SetEnvironmentVariable("FILEODON_DATA_DIRECTORY", dataDirectory);

builder.WebHost.ConfigureKestrel(options =>
{
    options.AddServerHeader = false;
});

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration).CreateLogger();
builder.Services.AddSerilog();

string filepath = Path.Combine(dataDirectory, ".env");
if (File.Exists(filepath))
    DotEnv.Load(filepath);
else
    DotEnv.CreateEnvFile(filepath);

builder.Configuration.AddEnvironmentVariables();

string? keyPath = builder.Configuration["Encryption:KeyPath"];
if (string.IsNullOrEmpty(keyPath))
{
    keyPath = Path.Combine(dataDirectory, "encryption.key");
}
else if (!Path.IsPathRooted(keyPath))
{
    keyPath = Path.Combine(builder.Environment.ContentRootPath, keyPath);
}

builder.Configuration["Encryption:KeyPath"] = keyPath;

if (!File.Exists(keyPath))
{
    Log.Warning("Encryption key file not found at {KeyPath}. Generating a new key.", keyPath);
    CryptoService.CreateKey(keyPath);
}

string jwtKeyPath = Path.Combine(dataDirectory, "jwt.key");
if (!File.Exists(jwtKeyPath))
{
    Log.Warning("JWT key file not found at {KeyPath}. Generating a new secret.", jwtKeyPath);
    File.WriteAllText(jwtKeyPath, await TokenService.GenerateSecret());
}

builder.Configuration["JWT:KEY"] = File.ReadAllText(jwtKeyPath).Trim();

string pamPrivateKeyPath = Path.Combine(dataDirectory, "sftp-pam-jwt-private.key");
string pamPublicKeyPath = Path.Combine(dataDirectory, "sftp-pam-jwt-public.key");
if (!File.Exists(pamPrivateKeyPath) || !File.Exists(pamPublicKeyPath))
{
    Log.Warning("SFTP PAM JWT keypair not found. Generating a new ES256 keypair.");
    using ECDsa pamKey = ECDsa.Create(ECCurve.NamedCurves.nistP256);
    File.WriteAllBytes(pamPrivateKeyPath, pamKey.ExportPkcs8PrivateKey());
    File.WriteAllText(pamPublicKeyPath, pamKey.ExportSubjectPublicKeyInfoPem());
}

builder.Configuration["SFTPPamJwt:PrivateKeyPath"] = pamPrivateKeyPath;
builder.Configuration["SFTPPamJwt:PublicKeyPath"] = pamPublicKeyPath;

string connectionString = Database.InitializeConnectionString(builder);

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
                         policy.WithExposedHeaders("Content-Disposition");
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
builder.Services.AddScoped<ISFTPActionAuditLogRepository, SFTPActionAuditLogRepository>();
#endregion
#region GlobalExceptionHandler
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();
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
builder.Services.AddSingleton<ISFTPPamTokenService, SFTPPamTokenService>();
builder.Services.AddHostedService<SFTPUploadWorker>();
builder.Services.AddHostedService<SFTPDistributionWorker>();
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

app.MapOpenApi();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/openapi/v1.json", "v1");
    options.RoutePrefix = string.Empty;
});

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseExceptionHandler();
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