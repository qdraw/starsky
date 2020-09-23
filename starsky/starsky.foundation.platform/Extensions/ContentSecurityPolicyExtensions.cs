using Microsoft.AspNetCore.Builder;
using starskycore.Middleware;

namespace starsky.foundation.platform.Extensions
{
    public static class ContentSecurityPolicyExtensions
    {
        public static IApplicationBuilder UseContentSecurityPolicy(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ContentSecurityPolicyMiddleware>();
        }
    }
}
