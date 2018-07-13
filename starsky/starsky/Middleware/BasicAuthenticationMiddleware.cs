using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace starsky.Middleware
{
    /// <summary>
    /// Accepts either username or email as user identifier for sign in with Http Basic authentication
    /// </summary>
    public class BasicAuthenticationMiddleware
    {
        
        public BasicAuthenticationMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        private readonly RequestDelegate _next;

        public async Task Invoke(HttpContext context)
        {
            if (!context.User.Identity.IsAuthenticated)
            {
                var basicAuthenticationHeader = GetBasicAuthenticationHeaderValue(context);
                if (basicAuthenticationHeader.IsValidBasicAuthenticationHeaderValue)
                {
                    var authenticationManager = new BasicAuthenticationSignInManager(context, basicAuthenticationHeader);
                    await authenticationManager.TrySignInUser();
                }
            }
            await _next.Invoke(context);
        }

        private BasicAuthenticationHeaderValue GetBasicAuthenticationHeaderValue(HttpContext context)
        {
            var basicAuthenticationHeader = context.Request.Headers["Authorization"]
                .FirstOrDefault(header => header.StartsWith("Basic", StringComparison.OrdinalIgnoreCase));
            var decodedHeader = new BasicAuthenticationHeaderValue(basicAuthenticationHeader);
            return decodedHeader;
        }
    }
}