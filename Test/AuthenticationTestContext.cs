using Domain.Interface;
using Domain.Service;
using Infrastructure;
using Infrastructure.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Test;

internal sealed class AuthenticationTestContext : IDisposable
{
    private readonly ServiceProvider _serviceProvider;
    private readonly ApplicationDbContext _dbContext;

    private AuthenticationTestContext(
        ServiceProvider serviceProvider,
        ApplicationDbContext dbContext)
    {
        _serviceProvider = serviceProvider;
        _dbContext = dbContext;
    }

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
            ["JWT_KEY"] = Convert.ToBase64String(new byte[32]),
            ["Jwt:Issuer"] = "fileodon-tests",
            ["Jwt:Audience"] = "fileodon-test-client",
            ["Jwt:RefreshTokenValidityInDays"] = "30"
        };

        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configurationValues)
            .Build();

        services.AddSingleton(configuration);
        services.AddSingleton(dbContext);
        services.AddIdentityCore<User>(identityOptions =>
        {
            identityOptions.Password.RequireDigit = configuration.GetValue<bool>("Identity:Password:RequireDigit");
            identityOptions.Password.RequireUppercase = configuration.GetValue<bool>("Identity:Password:RequireUppercase");
            identityOptions.Password.RequireLowercase = configuration.GetValue<bool>("Identity:Password:RequireLowercase");
            identityOptions.Password.RequireNonAlphanumeric = configuration.GetValue<bool>("Identity:Password:RequireNonAlphanumeric");
            identityOptions.Password.RequiredLength = configuration.GetValue<int>("Identity:Password:RequiredLength");
        })
            .AddRoles<AspNetRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddSignInManager();
        services.AddScoped<ITokenService, TokenService>();

        return new AuthenticationTestContext(services.BuildServiceProvider(), dbContext);
    }
}
