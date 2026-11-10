using System.Text.Encodings.Web;
using Domain.Interface;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace Controller.Middleware;

public class Authentication : AuthenticationHandler<AuthenticationSchemeOptions>
{
    private readonly ILogger<Authentication> _logger;
    private readonly IOptionsMonitor<AuthenticationSchemeOptions> _options;
    private readonly UrlEncoder _encoder;
    private readonly ISystemClock _clock;
    private readonly ITokenService _tokenService;
    public Authentication(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock)
        : base(options, logger, encoder, clock)
    {
        _options = options;
        _logger = logger.CreateLogger<Authentication>();
        _encoder = encoder;
        _clock = clock;
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        // Implement your authentication logic here
        // For example, you can check for a valid token in the request headers
        // If authentication is successful, create an AuthenticationTicket
        var claims = new[] { new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Name, "User") };
        var identity = new System.Security.Claims.ClaimsIdentity(claims, Scheme.Name);
        var principal = new System.Security.Claims.ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);
        return AuthenticateResult.Success(ticket);
    }
}
