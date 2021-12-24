using Microsoft.AspNetCore.Builder;
using starsky.foundation.accountmanagement.Middleware;

// ReSharper disable once IdentifierTypo
namespace starsky.foundation.accountmanagement.Extensions
{
    public static class UseBasicAuthenticationExtensions
    {
        public static IApplicationBuilder UseBasicAuthentication(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<BasicAuthenticationMiddleware>();
        }
    }
}
