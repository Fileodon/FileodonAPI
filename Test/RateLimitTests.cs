using System.Security.Claims;
using Controller.Middleware;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Configuration;
using System.Threading.RateLimiting;

namespace Test;

public sealed class RateLimitTests
{
    [Fact]
    public void Authenticated_users_get_isolated_partitions()
    {
        RateLimit policy = CreatePolicy(permitLimit: 2);
        DefaultHttpContext firstUser = CreateContext("user-1");
        DefaultHttpContext secondUser = CreateContext("user-2");

        RateLimitPartition<string> firstPartition = policy.GetPartition(firstUser);
        RateLimitPartition<string> secondPartition = policy.GetPartition(secondUser);

        Assert.NotEqual(firstPartition.PartitionKey, secondPartition.PartitionKey);
    }

    [Fact]
    public void Anonymous_requests_are_partitioned_by_ip_address()
    {
        RateLimit policy = CreatePolicy();
        DefaultHttpContext firstRequest = CreateContext(ipAddress: "192.0.2.1");
        DefaultHttpContext sameIpRequest = CreateContext(ipAddress: "192.0.2.1");
        DefaultHttpContext differentIpRequest = CreateContext(ipAddress: "192.0.2.2");

        string firstKey = policy.GetPartition(firstRequest).PartitionKey;
        string sameIpKey = policy.GetPartition(sameIpRequest).PartitionKey;
        string differentIpKey = policy.GetPartition(differentIpRequest).PartitionKey;

        Assert.Equal(firstKey, sameIpKey);
        Assert.NotEqual(firstKey, differentIpKey);
    }

    [Fact]
    public void Partition_enforces_configured_permit_limit_without_queueing()
    {
        RateLimit policy = CreatePolicy(permitLimit: 2, queueLimit: 0);
        RateLimitPartition<string> partition = policy.GetPartition(CreateContext(ipAddress: "192.0.2.1"));
        using RateLimiter limiter = partition.Factory(partition.PartitionKey);

        Assert.True(limiter.AttemptAcquire().IsAcquired);
        Assert.True(limiter.AttemptAcquire().IsAcquired);
        RateLimitLease rejectedLease = limiter.AttemptAcquire();

        Assert.False(rejectedLease.IsAcquired);
        Assert.True(rejectedLease.TryGetMetadata(
            MetadataName.RetryAfter,
            out TimeSpan retryAfter));
        Assert.InRange(retryAfter, TimeSpan.Zero, TimeSpan.FromMinutes(1));
    }

    [Fact]
    public async Task Rejection_callback_returns429_and_retry_after_header()
    {
        RateLimit policy = CreatePolicy(permitLimit: 1);
        RateLimitPartition<string> partition = policy.GetPartition(CreateContext(ipAddress: "192.0.2.1"));
        using RateLimiter limiter = partition.Factory(partition.PartitionKey);
        limiter.AttemptAcquire();
        RateLimitLease rejectedLease = limiter.AttemptAcquire();

        DefaultHttpContext context = new();
        context.Response.Body = new MemoryStream();
        OnRejectedContext rejectionContext = new()
        {
            HttpContext = context,
            Lease = rejectedLease
        };

        await policy.OnRejected!(rejectionContext, CancellationToken.None);

        Assert.Equal(StatusCodes.Status429TooManyRequests, context.Response.StatusCode);
        Assert.NotEmpty(context.Response.Headers.RetryAfter.ToString());
        context.Response.Body.Position = 0;
        using StreamReader reader = new(context.Response.Body);
        Assert.Contains("Rate limit exceeded", await reader.ReadToEndAsync());
    }

    [Fact]
    public void Invalid_positive_settings_fail_explicitly()
    {
        RateLimit policy = CreatePolicy(permitLimit: 0);

        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(
            () => policy.GetPartition(CreateContext()));

        Assert.Contains("RateLimiting:PermitLimit", exception.Message);
    }

    private static RateLimit CreatePolicy(
        int permitLimit = 120,
        int queueLimit = 0,
        int windowMinutes = 1)
    {
        Dictionary<string, string?> settings = new()
        {
            ["RateLimiting:PermitLimit"] = permitLimit.ToString(),
            ["RateLimiting:QueueLimit"] = queueLimit.ToString(),
            ["RateLimiting:WindowMinutes"] = windowMinutes.ToString()
        };

        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(settings)
            .Build();

        return new RateLimit(configuration);
    }

    private static DefaultHttpContext CreateContext(
        string? userId = null,
        string? ipAddress = null)
    {
        DefaultHttpContext context = new();
        context.Connection.RemoteIpAddress =
            System.Net.IPAddress.Parse(ipAddress ?? "192.0.2.1");

        if (userId is not null)
        {
            context.User = new ClaimsPrincipal(new ClaimsIdentity(
                [new Claim(ClaimTypes.NameIdentifier, userId)],
                authenticationType: "test"));
        }

        return context;
    }
}
