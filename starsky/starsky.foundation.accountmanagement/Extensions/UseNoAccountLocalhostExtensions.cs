using Microsoft.AspNetCore.Builder;
using starsky.foundation.accountmanagement.Middleware;

// ReSharper disable once IdentifierTypo
namespace starsky.foundation.accountmanagement.Extensions
{
    public static class UseNoAccountLocalhostExtensions
    {
        public static IApplicationBuilder UseNoAccountLocalhost(this IApplicationBuilder builder, bool enable)
        {
	        return !enable ? builder : builder.UseMiddleware<NoAccountLocalhostMiddleware>();
        }
    }
}
