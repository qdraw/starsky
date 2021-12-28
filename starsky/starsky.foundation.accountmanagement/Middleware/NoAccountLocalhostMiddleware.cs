using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using starsky.foundation.accountmanagement.Helpers;
using starsky.foundation.accountmanagement.Interfaces;

// ReSharper disable once IdentifierTypo
[assembly: InternalsVisibleTo("starskytest")]
namespace starsky.foundation.accountmanagement.Middleware
{
    /// <summary>
    /// Auto login when use is on localhost
    /// </summary>
    public class NoAccountLocalhostMiddleware
    {
       
        public NoAccountLocalhostMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        private readonly RequestDelegate _next;

        internal const string Identifier = "mail@localhost";

        /// <summary>
        /// Enable: app__NoAccountLocalhost
        /// </summary>
        /// <param name="context"></param>
        public async Task Invoke(HttpContext context)
        {
	        var isHostLocal = IsLocalhost.IsHostLocalHost(context.Connection.LocalIpAddress, context.Connection.RemoteIpAddress);
	        var isApiCall = context.Request.Path.HasValue && (context.Request.Path.Value.StartsWith("/api") || 
		        context.Request.Path.Value.StartsWith("/realtime"));
	        
	        var isFromLogoutCall = context.Request.QueryString.HasValue && 
	                               context.Request.QueryString.Value.Contains("fromLogout");
	        
	        if ( isHostLocal && !context.User.Identity.IsAuthenticated && !isApiCall && !isFromLogoutCall)
	        {
		        var userManager = (IUserManager) context.RequestServices.GetService(typeof(IUserManager));

		        var user = userManager.GetUser("email", Identifier);
		        if ( user == null )
		        {
			        var newPassword = Convert.ToBase64String(
					        Pbkdf2Hasher.GenerateRandomSalt());
			        await userManager.SignUpAsync(string.Empty, 
				        "email", Identifier, newPassword+newPassword);
			        user = userManager.GetUser("email", Identifier);
		        }
		        
		        await userManager.SignIn(context, user, true);
	        }
            await _next.Invoke(context);
        }
    }
}
