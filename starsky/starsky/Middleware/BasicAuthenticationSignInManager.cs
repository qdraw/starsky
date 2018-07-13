using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using starsky.Models;

namespace starsky.Middleware
{
    public class BasicAuthenticationSignInManager
    {
        public BasicAuthenticationSignInManager(HttpContext context, BasicAuthenticationHeaderValue authenticationHeaderValue)
        {
            _context = context;
            _authenticationHeaderValue = authenticationHeaderValue;
            _userManager = _context.RequestServices.GetRequiredService<UserManager<ApplicationUser>>();
            _signInManager = _context.RequestServices.GetRequiredService<SignInManager<ApplicationUser>>();
        }
        
        private readonly HttpContext _context;
        private readonly BasicAuthenticationHeaderValue _authenticationHeaderValue;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private ApplicationUser _user;

        public async Task TrySignInUser()
        {
                
            if (_authenticationHeaderValue.IsValidBasicAuthenticationHeaderValue)
            {
                await GetUserByUsernameOrEmail();
                if (_user != null)
                {
                    await SignInUserIfPasswordIsCorrect();
                }
            }
        }

        private async Task GetUserByUsernameOrEmail()
        {
            _user = await  _userManager.FindByEmailAsync(_authenticationHeaderValue.UserIdentifier)
                    ?? await _userManager.FindByNameAsync(_authenticationHeaderValue.UserIdentifier);
        }

        private async Task SignInUserIfPasswordIsCorrect()
        {
            if (await _userManager.CheckPasswordAsync(_user, _authenticationHeaderValue.UserPassword))
            {
                _context.User = await _signInManager.CreateUserPrincipalAsync(_user);
            }
        }
    }
}