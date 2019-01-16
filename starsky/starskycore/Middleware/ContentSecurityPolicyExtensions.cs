using Microsoft.AspNetCore.Builder;

namespace starskycore.Middleware
{
    public static class ContentSecurityPolicyExtensions
    {

        public static IApplicationBuilder UseContentSecurityPolicy(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ContentSecurityPolicyMiddleware>();
        }
    }
}