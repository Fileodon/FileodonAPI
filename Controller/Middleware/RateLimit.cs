using System.Security.Claims;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;

namespace Controller.Middleware;

public sealed class RateLimit : IRateLimiterPolicy<string>
{
    private readonly IConfiguration _configuration;

    public RateLimit(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public RateLimitPartition<string> GetPartition(HttpContext httpContext)
    {
        string key = GetPartitionKey(httpContext);

        int permitLimit = GetPositiveSetting("PermitLimit", 120);
        int queueLimit = GetNonNegativeSetting("QueueLimit", 0);
        int windowMinutes = GetPositiveSetting("WindowMinutes", 1);

        return RateLimitPartition.GetFixedWindowLimiter(
            key,
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = permitLimit,
                QueueLimit = queueLimit,
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                Window = TimeSpan.FromMinutes(windowMinutes)
            });
    }

    public Func<OnRejectedContext, CancellationToken, ValueTask>? OnRejected =>
        (context, token) =>
        {
            context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
            context.HttpContext.Response.ContentType = "text/plain; charset=utf-8";

            if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out TimeSpan retryAfter))
            {
                int retryAfterSeconds = Math.Max(1, (int)Math.Ceiling(retryAfter.TotalSeconds));
                context.HttpContext.Response.Headers.RetryAfter = retryAfterSeconds.ToString();
                return new ValueTask(context.HttpContext.Response.WriteAsync(
                    $"Rate limit exceeded. Please try again in {retryAfterSeconds} seconds.",
                    token));
            }

            return new ValueTask(context.HttpContext.Response.WriteAsync(
                "Rate limit exceeded. Please try again later.",
                token));
        };

    private int GetPositiveSetting(string name, int defaultValue)
    {
        int value = _configuration.GetValue($"RateLimiting:{name}", defaultValue);
        return value > 0
            ? value
            : throw new InvalidOperationException(
                $"RateLimiting:{name} must be greater than zero.");
    }

    private int GetNonNegativeSetting(string name, int defaultValue)
    {
        int value = _configuration.GetValue($"RateLimiting:{name}", defaultValue);
        return value >= 0
            ? value
            : throw new InvalidOperationException(
                $"RateLimiting:{name} must be zero or greater.");
    }

    private static string GetPartitionKey(HttpContext httpContext)
    {
        if (httpContext.User.Identity?.IsAuthenticated == true)
        {
            string? userId = httpContext.User.FindFirstValue("id")
                ?? httpContext.User.FindFirstValue("sub")
                ?? httpContext.User.FindFirstValue("unique_name");

            if (!string.IsNullOrWhiteSpace(userId))
            {
                return $"user:{userId}";
            }
        }

        string? ipAddress = httpContext.Connection.RemoteIpAddress?.ToString();
        return string.IsNullOrWhiteSpace(ipAddress) ? "ip:unknown" : $"ip:{ipAddress}";
    }
}