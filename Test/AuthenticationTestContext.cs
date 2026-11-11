using Domain.Interface;
using Domain.Service;
using Infrastructure;
using Infrastructure.Interface;
using Infrastructure.Interface.Repository;
using Infrastructure.Models;
using Infrastructure.Repository;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace Test;

internal sealed class AuthenticationTestContext : IDisposable
{
    private readonly ServiceProvider _serviceProvider;
    private readonly ApplicationDbContext _dbContext;

    private AuthenticationTestContext(
        ServiceProvider serviceProvider,
        ApplicationDbContext dbContext,
        Mock<ISmtpEmailSender> emailSender)
    {
        _serviceProvider = serviceProvider;
        _dbContext = dbContext;
        EmailSender = emailSender;
    }

    public Mock<ISmtpEmailSender> EmailSender { get; }

    public IServiceScope CreateScope() => _serviceProvider.CreateScope();

    public void Dispose()
    {
        _serviceProvider.Dispose();
        _dbContext.Dispose();
    }

    public static AuthenticationTestContext Create()
    {
        ServiceCollection services = new();
        services.AddLogging();
        services.AddAuthentication();

        DbContextOptions<ApplicationDbContext> options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite($"Data Source=file:auth-tests-{Guid.NewGuid():N}?mode=memory&cache=shared")
            .Options;
        ApplicationDbContext dbContext = new(options);
        dbContext.Database.OpenConnection();
        dbContext.Database.EnsureCreated();

        Dictionary<string, string?> configurationValues = new()
        {
            ["JWT:KEY"] = Convert.ToBase64String(new byte[32]),
            ["Jwt:Issuer"] = "fileodon-tests",
            ["Jwt:Audience"] = "fileodon-test-client",
            ["Jwt:RefreshTokenValidityInDays"] = "30",
            ["API:HOST"] = "http://localhost:5000",
        };

        IConfiguration configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false)
            .AddInMemoryCollection(configurationValues)
            .Build();

        services.AddSingleton(configuration);
        services.AddSingleton(dbContext);
        services.AddScoped<RepositoryTransactionSettings>();
        services.AddScoped<IRepositoryTransactionSettings>(services =>
            services.GetRequiredService<RepositoryTransactionSettings>());
        services.AddScoped<IOrganizationRepository, OrganizationRepository>();
        services.AddScoped<IProfileRepository, ProfileRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<ISFTPConnectionRepository, SFTPConnectionRepository>();
        services.AddScoped<ISFTPConnectionAuthCodeRepository, SFTPConnectionAuthCodeRepository>();
        services.AddScoped<IUserOrganizationRepository, UserOrganizationRepository>();
        services.AddScoped<IAuditLogRepository, AuditLogRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        Mock<ISmtpEmailSender> emailSender = new();
        emailSender
            .Setup(sender => sender.SendConfirmationLinkAsync(
                It.IsAny<User>()))
            .ReturnsAsync(true);
        emailSender
            .Setup(sender => sender.SendPasswordResetLinkAsync(
                It.IsAny<User>()))
            .ReturnsAsync(true);
        services.AddSingleton(emailSender);
        services.AddSingleton<ISmtpEmailSender>(emailSender.Object);
        services.AddIdentityCore<User>(identityOptions =>
        {
            IConfigurationSection passwordPolicy = configuration.GetSection("PasswordPolicy");
            identityOptions.Password.RequireDigit = passwordPolicy.GetValue<bool>(nameof(identityOptions.Password.RequireDigit));
            identityOptions.Password.RequireUppercase = passwordPolicy.GetValue<bool>(nameof(identityOptions.Password.RequireUppercase));
            identityOptions.Password.RequireLowercase = passwordPolicy.GetValue<bool>(nameof(identityOptions.Password.RequireLowercase));
            identityOptions.Password.RequireNonAlphanumeric = passwordPolicy.GetValue<bool>(nameof(identityOptions.Password.RequireNonAlphanumeric));
            identityOptions.Password.RequiredLength = passwordPolicy.GetValue<int>(nameof(identityOptions.Password.RequiredLength));
            identityOptions.User.RequireUniqueEmail = passwordPolicy.GetValue<bool>(nameof(identityOptions.User.RequireUniqueEmail));
        })
            .AddRoles<AspNetRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddSignInManager()
            .AddDefaultTokenProviders();
        services.AddScoped<ITokenService, TokenService>();

        return new AuthenticationTestContext(services.BuildServiceProvider(), dbContext, emailSender);
    }
}
