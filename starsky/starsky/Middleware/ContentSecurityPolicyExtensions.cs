using Microsoft.AspNetCore.Builder;
using starsky.Interfaces;

namespace starsky.Middleware
{
    public static class ContentSecurityPolicyExtensions
    {

        public static IApplicationBuilder UseContentSecurityPolicy(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ContentSecurityPolicyMiddleware>();
        }
    }
}