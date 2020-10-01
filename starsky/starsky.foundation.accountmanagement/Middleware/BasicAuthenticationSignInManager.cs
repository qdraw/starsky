using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using starsky.foundation.accountmanagement.Interfaces;

namespace starsky.foundation.accountmanagement.Middleware
{
    public class BasicAuthenticationSignInManager
    {
        private readonly IUserManager _userManager;

        public BasicAuthenticationSignInManager(
            HttpContext context, 
            BasicAuthenticationHeaderValue authenticationHeaderValue,
            IUserManager userManager)
        {
            _context = context;
            _authenticationHeaderValue = authenticationHeaderValue;
            _userManager = userManager;
        }
        
        private readonly HttpContext _context;
        private readonly BasicAuthenticationHeaderValue _authenticationHeaderValue;

        public async Task TrySignInUser()
        {
            if (_authenticationHeaderValue.IsValidBasicAuthenticationHeaderValue)
            {
                var validateResult = _userManager.Validate("email",
                    _authenticationHeaderValue.UserIdentifier,
                    _authenticationHeaderValue.UserPassword);
                
                if (!validateResult.Success)
                {
                    _context.Response.StatusCode = 401;
					if(!_context.Response.Headers.ContainsKey("WWW-Authenticate") )
					{
						_context.Response.Headers.Add("WWW-Authenticate", "Basic realm=\"Starsky " + validateResult.Error + " \"");
					}
					return;
                }

                await _userManager.SignIn(_context, validateResult.User);
            }
        }
    }
}
