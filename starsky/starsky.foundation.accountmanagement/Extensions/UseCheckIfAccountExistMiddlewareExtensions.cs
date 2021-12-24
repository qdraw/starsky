using Microsoft.AspNetCore.Builder;
using starsky.foundation.accountmanagement.Middleware;

// ReSharper disable once IdentifierTypo
namespace starsky.foundation.accountmanagement.Extensions
{
    public static class UseCheckIfAccountExistMiddlewareExtensions
    {
        public static IApplicationBuilder UseCheckIfAccountExist(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<CheckIfAccountExistMiddleware>();
        }
    }
}
