using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using starsky.foundation.accountmanagement.Interfaces;

// ReSharper disable once IdentifierTypo
namespace starsky.foundation.accountmanagement.Middleware
{
    /// <summary>
    /// Accepts either username or email as user identifier for sign in with Http Basic authentication
    /// </summary>
    public sealed class BasicAuthenticationMiddleware
    {
       
        public BasicAuthenticationMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        private readonly RequestDelegate _next;

        public async Task Invoke(HttpContext context)
        {
	        await Authenticate(context);
            await _next.Invoke(context);
        }

        public static async Task<bool> Authenticate(HttpContext context)
        {
	        if ( context.User.Identity?.IsAuthenticated != false )
	        {
		        return false;
	        }
	        var basicAuthenticationHeader = GetBasicAuthenticationHeaderValue(context);
	        
	        if ( !basicAuthenticationHeader
		            .IsValidBasicAuthenticationHeaderValue )
	        {
		        return false;
	        }
	        
	        var userManager = (IUserManager) context.RequestServices.GetService(typeof(IUserManager));
		                
	        var authenticationManager = new BasicAuthenticationSignInManager(
		        context, basicAuthenticationHeader, userManager);
	        await authenticationManager.TrySignInUser();

	        return context.User.Identity?.IsAuthenticated == true;
        }

        private static BasicAuthenticationHeaderValue GetBasicAuthenticationHeaderValue(HttpContext context)
        {
            var basicAuthenticationHeader = context.Request.Headers["Authorization"]
                .FirstOrDefault(header => header.StartsWith("Basic", StringComparison.OrdinalIgnoreCase));
            var decodedHeader = new BasicAuthenticationHeaderValue(basicAuthenticationHeader);
            return decodedHeader;
        }
    }
}
