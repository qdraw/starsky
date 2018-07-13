using Microsoft.AspNetCore.Builder;

namespace starsky.Middleware
{
    public static class MiddlewareExtensions
    {
        public static IApplicationBuilder UseBasicAuthentication(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<BasicAuthenticationMiddleware>();
        }
    }
}