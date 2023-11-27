using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace CleanArchitecture.IntegrationTests
{
    // https://learn.microsoft.com/en-us/aspnet/core/test/integration-tests?view=aspnetcore-5.0#mock-authentication-1
    public class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        private readonly ITestClaimsProvider _claimsProvider;

        public TestAuthHandler(IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock, ITestClaimsProvider claimsProvider)
        : base(options, logger, encoder, clock)
        {
            _claimsProvider = claimsProvider;
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var ticket = new AuthenticationTicket(_claimsProvider.GetClaimsPrincipal(), "Scheme");

            var result = AuthenticateResult.Success(ticket);

            return Task.FromResult(result);
        }
    }

    public interface ITestClaimsProvider
    {
        ClaimsPrincipal GetClaimsPrincipal();
    }

    public class TestClaimsProvider : ITestClaimsProvider
    {
        private readonly string _role;

        public TestClaimsProvider(string role)
        {
            _role = role;
        }

        public ClaimsPrincipal GetClaimsPrincipal()
        {
            var claims = new[] { new Claim(ClaimTypes.Name, "Foo Bar")
                , new Claim("email", "foo@bar.com")
                , new Claim(ClaimTypes.Role, _role) };
            var identity = new ClaimsIdentity(claims, "Type");
            var principal = new ClaimsPrincipal(identity);

            return principal;
        }
    }
}