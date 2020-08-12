using Microsoft.AspNetCore.Builder;

namespace starsky.foundation.accountmanagement.Middleware
{
    public static class UseBasicAuthenticationExtensions
    {

        public static IApplicationBuilder UseBasicAuthentication(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<BasicAuthenticationMiddleware>();
        }
    }
}
