using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;

namespace starskytests.Mocks
{
    // Mock class for IAuthenticationService
    public class NoOpAuth : IAuthenticationService
    {
        public Task<AuthenticateResult> AuthenticateAsync(HttpContext context, string scheme)
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        public Task ChallengeAsync(HttpContext context, string scheme, AuthenticationProperties properties)
        {
            return Task.FromResult(0);
        }

        public Task ForbidAsync(HttpContext context, string scheme, AuthenticationProperties properties)
        {
            return Task.FromResult(0);
        }

        public Task SignInAsync(HttpContext context, string scheme, ClaimsPrincipal principal, AuthenticationProperties properties)
        {
            return Task.FromResult(0);
        }

        public Task SignOutAsync(HttpContext context, string scheme, AuthenticationProperties properties)
        {
            return Task.FromResult(0);
        }
    }
}