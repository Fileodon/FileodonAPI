using System.IdentityModel.Tokens.Jwt;
using Domain.Service;
using Infrastructure;
using Infrastructure.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Test;

public sealed class TokenServiceTests
{
    [Fact]
    public async Task CreateToken_includes_user_and_role_claims()
    {
        using AuthenticationTestContext context = AuthenticationTestContext.Create();
        using var scope = context.CreateScope();
        UserManager<User> userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
        TokenService tokenService = new(
            scope.ServiceProvider.GetRequiredService<IConfiguration>(),
            userManager);
        User user = new()
        {
            Id = "user-id",
            UserName = "ada",
            Email = "ada@example.com"
        };
        await userManager.CreateAsync(user);
        ApplicationDbContext dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        Organization organization = new()
        {
            UserOwnerId = user.Id,
            OrgName = "Test organization",
            WorkspaceURL = "test.example.com",
            ContactEmail = "owner@test.example.com"
        };
        dbContext.Organizations.Add(organization);
        await dbContext.SaveChangesAsync();
        RoleManager<AspNetRole> roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<AspNetRole>>();
        await roleManager.CreateAsync(new AspNetRole { Name = "Administrator", OrganizationId = organization.Id });
        await userManager.AddToRoleAsync(user, "Administrator");

        string token = await tokenService.CreateTokenAsync(user);
        JwtSecurityToken parsedToken = new JwtSecurityTokenHandler().ReadJwtToken(token);

        Assert.Equal("user-id", parsedToken.Claims.Single(claim => claim.Type == "id").Value);
        Assert.Equal("ada", parsedToken.Claims.Single(claim => claim.Type == "name").Value);
        Assert.Equal("ada@example.com", parsedToken.Claims.Single(claim => claim.Type == "email").Value);
        Assert.Equal("Administrator", parsedToken.Claims.Single(claim => claim.Type == "role").Value);
    }

    [Fact]
    public void Refresh_tokens_are_random_and_hashes_are_deterministic()
    {
        using AuthenticationTestContext context = AuthenticationTestContext.Create();
        using var scope = context.CreateScope();
        TokenService tokenService = new(
            scope.ServiceProvider.GetRequiredService<IConfiguration>(),
            scope.ServiceProvider.GetRequiredService<UserManager<User>>());

        string firstToken = tokenService.CreateRefreshToken();
        string secondToken = tokenService.CreateRefreshToken();

        Assert.NotEqual(firstToken, secondToken);
        Assert.Equal(tokenService.HashRefreshToken(firstToken), tokenService.HashRefreshToken(firstToken));
        Assert.NotEqual(tokenService.HashRefreshToken(firstToken), tokenService.HashRefreshToken(secondToken));
        Assert.Equal(64, tokenService.HashRefreshToken(firstToken).Length);
    }

    [Fact]
    public async Task CreateToken_requires_a_configured_jwt_key()
    {
        using AuthenticationTestContext context = AuthenticationTestContext.Create();
        using var scope = context.CreateScope();
        UserManager<User> userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Issuer"] = "issuer",
                ["Jwt:Audience"] = "audience"
            })
            .Build();
        TokenService tokenService = new(configuration, userManager);

        InvalidOperationException exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => tokenService.CreateTokenAsync(new User { Id = "user-id" }));

        Assert.Contains("JWT key is not set", exception.Message);
    }
}
