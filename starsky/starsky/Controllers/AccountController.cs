using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using starsky.foundation.accountmanagement.Interfaces;
using starsky.foundation.accountmanagement.Models;
using starsky.foundation.accountmanagement.Models.Account;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Storage;
using starsky.Helpers;

namespace starsky.Controllers
{
    public sealed class AccountController : Controller
    {
        private readonly IUserManager _userManager;
        private readonly AppSettings _appSettings;
        private readonly IAntiforgery _antiForgery;
        private readonly IStorage _storageHostFullPathFilesystem;

        public AccountController(IUserManager userManager, AppSettings appSettings, IAntiforgery antiForgery, ISelectorStorage selectorStorage)
        {
            _userManager = userManager;
            _appSettings = appSettings;
            _antiForgery = antiForgery;
            _storageHostFullPathFilesystem = selectorStorage.Get(SelectorStorage.StorageServices.HostFilesystem);
        }
        
		/// <summary>
		/// Check the account status of the current logged in user
		/// </summary>
		/// <response code="200">Logged in</response>
		/// <response code="401">When not logged in</response>
		/// <response code="406">There are no accounts, you must create an account first</response>
		/// <response code="409">The Current User does not exist in database</response>
		/// <response code="503">Database Connection Error</response>
		/// <returns>account name, id, and create date as json</returns>
		[HttpGet("/api/account/status")]
		[ProducesResponseType(typeof(UserIdentifierStatusModel), 200)]
		[ProducesResponseType(typeof(string), 401)]
		[ProducesResponseType(typeof(string), 406)]
		[ProducesResponseType(typeof(string), 503)]
		[Produces("application/json")]
		[AllowAnonymous]
		public async Task<IActionResult> Status()
		{
			var userOverview = await _userManager.AllUsersAsync();
			if ( !userOverview.IsSuccess)
			{
				Response.StatusCode = 503;
				return Json("Database error");
			}
			
			if ( userOverview.Users.Count == 0 && _appSettings.NoAccountLocalhost != true )
			{
				Response.StatusCode = 406;
				return Json("There are no accounts, you must create an account first");
			}

			if ( User.Identity?.IsAuthenticated == false )
			{
				return Unauthorized("false");
			}

			// use model to avoid circular references
			var currentUser = _userManager.GetCurrentUser(HttpContext);
			if ( currentUser == null )
			{
				return Conflict("Current User does not exist in database");
			}
			
			var model = new UserIdentifierStatusModel
			{
				Name = currentUser.Name,
				Id = currentUser.Id,
				Created = currentUser.Created,
			};
			
			var credentials = _userManager.GetCredentialsByUserId(currentUser.Id);
			if ( credentials == null )
			{
				model.CredentialsIdentifiers = null;
				model.CredentialTypeIds = null;
				return Json(model);
			}
			
			model.CredentialsIdentifiers?.Add(credentials.Identifier!);
			model.CredentialTypeIds?.Add(credentials.CredentialTypeId);
			return Json(model);
		}

		
		/// <summary>
		/// Login form page (HTML)
		/// </summary>
		/// <returns></returns>
		/// <response code="200">Login form page</response>
		[HttpGet("/account/login")]
		[HttpHead("/account/login")]
		[ProducesResponseType(200)]
		[Produces("text/html")]
		[SuppressMessage("ReSharper", "UnusedParameter.Global")]
		[AllowAnonymous]
		public IActionResult LoginGet(string? returnUrl = null, bool? fromLogout = null)
		{
			new AntiForgeryCookie(_antiForgery).SetAntiForgeryCookie(HttpContext);
			var clientApp = Path.Combine(_appSettings.BaseDirectoryProject,
				"clientapp", "build", "index.html");

			if ( !_storageHostFullPathFilesystem.ExistFile(clientApp) ) return Content("Please check if the client code exist");
			return PhysicalFile(clientApp, "text/html");
		}
		
        /// <summary>
        /// Login the current HttpContext in
        /// </summary>
        /// <param name="model">Email, password and remember me bool</param>
        /// <returns>Login status</returns>
        /// <response code="200">Successful login</response>
        /// <response code="401">Login failed</response>
        /// <response code="405">ValidateAntiForgeryToken error</response>
        /// <response code="423">Login failed due lock</response>
        /// <response code="500">Login failed due signIn errors</response>
        [HttpPost("/api/account/login")]
        [ProducesResponseType(typeof(string),200)]
        [ProducesResponseType(typeof(string),401)]
        [ProducesResponseType(typeof(string),405)]
#if ! DEBUG
        [ValidateAntiForgeryToken]
#endif
        [Produces("application/json")]
        [AllowAnonymous]
        public async Task<IActionResult> LoginPost(LoginViewModel model)
        {
            ValidateResult validateResult = await _userManager.ValidateAsync("Email", model.Email, model.Password);

            if (!validateResult.Success)
            {
	            Response.StatusCode = 401;
	            if ( validateResult.Error == ValidateResultError.Lockout )
	            {
		            Response.StatusCode = 423;
	            }
                return Json("Login failed");
            } 
            
            await _userManager.SignIn(HttpContext, validateResult.User, model.RememberMe);
            if ( User.Identity?.IsAuthenticated == true)
            {
	            return Json("Login Success");
            }

            Response.StatusCode = 500;
            return Json("Login failed");
        }

        /// <summary>
        /// Logout the current HttpContext out
        /// </summary>
        /// <returns>Login Status</returns>
        /// <response code="200">Successful logout</response>
        [HttpPost("/api/account/logout")]
        [ProducesResponseType(200)]
        [AllowAnonymous]
        public IActionResult LogoutJson()
        {
	        _userManager.SignOut(HttpContext);
	        return Json("your logged out");
        }
        
        /// <summary>
        /// Logout the current HttpContext and redirect to login 
        /// </summary>
        /// <param name="returnUrl">insert url to redirect</param>
        /// <response code="302">redirect to return url</response>
        /// <returns>Redirect to login page</returns>
        [HttpGet("/account/logout")]
        [ProducesResponseType(200)]
        [AllowAnonymous]
        public IActionResult Logout(string? returnUrl = null)
        {
            _userManager.SignOut(HttpContext);
            // fromLogout is used in middleware
            return RedirectToAction(nameof(LoginGet), new {ReturnUrl = returnUrl, fromLogout = true});
        }
        
        /// <summary>
        /// Update password for current user
        /// </summary>
        /// <param name="model">Password, ChangedPassword and ChangedConfirmPassword</param>
        /// <returns>Change secret status</returns>
        /// <response code="200">successful login</response>
        /// <response code="400">Model is not correct</response>
        /// <response code="401"> please login first or your current password is not correct</response>
        [HttpPost("/api/account/change-secret")]
        [ProducesResponseType(typeof(string),200)]
        [ProducesResponseType(typeof(string),400)]
        [ProducesResponseType(typeof(string),401)]
        [Produces("application/json")]
        [Authorize]
        public async Task<IActionResult> ChangeSecret(ChangePasswordViewModel model)
        {
	        if ( User.Identity?.IsAuthenticated != true )
	        {
		        return Unauthorized("please login first");
	        }

	        if ( !ModelState.IsValid ||
	             model.ChangedPassword != model.ChangedConfirmPassword )
	        {
		        return BadRequest("Model is not correct");
	        }

	        var currentUserId = _userManager.GetCurrentUser(HttpContext)?.Id;
	        currentUserId ??= -1;
	        var credential = _userManager.GetCredentialsByUserId((int) currentUserId);

	        // Re-check password
	        var validateResult = await _userManager.ValidateAsync(
		        "Email", 
		        credential?.Identifier, 
		        model.Password);
	        
	        if ( !validateResult.Success )
	        {
		        return Unauthorized("Password is not correct");
	        }

	        var changeSecretResult =
		        _userManager.ChangeSecret("Email", credential?.Identifier,
			        model.ChangedPassword);

	        return Json(changeSecretResult);
        }

        /// <summary>
        /// Create a new user (you need a AF-token first)
        /// </summary>
        /// <param name="model">with the userdata</param>
        /// <returns>redirect or json</returns>
        /// <response code="200">successful register</response>
        /// <response code="400">Wrong model or Wrong AntiForgeryToken</response>
        /// <response code="403">Account Register page is closed</response>
        /// <response code="405">AF token is missing</response>
        [HttpPost("/api/account/register")]
        [ProducesResponseType(typeof(string),200)]
        [ProducesResponseType(typeof(string),400)]
        [ProducesResponseType(typeof(string),403)]
        [ProducesResponseType(typeof(string),405)]
        [Produces("application/json")]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
	        if ( await IsAccountRegisterClosed(User.Identity?.IsAuthenticated == true) )
	        {
		        Response.StatusCode = 403;
		        return Json("Account Register page is closed");
	        }
	        
            if (ModelState.IsValid && model.ConfirmPassword == model.Password)
            {
                await _userManager.SignUpAsync(model.Name, "email", model.Email, model.Password );
                return Json("Account Created");
            }

            // If we got this far, something failed, redisplay form
            Response.StatusCode = 400;
            return Json("Model is not correct");
        }

        /// <summary>
        /// True == not allowed
        /// </summary>
        /// <param name="userIdentityIsAuthenticated"></param>
        /// <returns></returns>
        private async Task<bool> IsAccountRegisterClosed(bool userIdentityIsAuthenticated)
        {
	        if ( userIdentityIsAuthenticated ) return false;
	        return _appSettings.IsAccountRegisterOpen != true && (await _userManager.AllUsersAsync()).Users.Count != 0;
        }
        
        /// <summary>
        /// Is the register form open
        /// </summary>
        /// <returns></returns>
        /// <response code="200">Account Register page is open</response>
        /// <response code="202">open, but you are the first user</response>
        /// <response code="403">Account Register page is closed</response>
        [HttpGet("/api/account/register/status")]
        [ProducesResponseType(typeof(string),200)]
        [ProducesResponseType(typeof(string),403)]
        [Produces("application/json")]
        [AllowAnonymous]
        public async Task<IActionResult> RegisterStatus()
        {
	        if ( ( await _userManager.AllUsersAsync() ).Users.Count == 0 )
	        {
		        Response.StatusCode = 202;
	        }

	        if ( !await IsAccountRegisterClosed(
		            User.Identity?.IsAuthenticated == true) )
	        {
		        return Json("RegisterStatus open");
	        }
	        
	        Response.StatusCode = 403;
	        return Json("Account Register page is closed");
        }
        
        /// <summary>
        /// List of current permissions
        /// </summary>
        /// <returns>list of current permissions</returns>
        /// <response code="200">list of permissions</response>
        /// <response code="401"> please login first</response>
        [HttpGet("/api/account/permissions")]
        [Authorize]
        [ProducesResponseType(typeof(List<string>),200)]
        [ProducesResponseType(401)]
        public IActionResult Permissions()
        {
	        var claims = User.Claims.Where(p=> p.Type == "Permission").Select( p=>  p.Value);
	        return Json(claims);
        }

    }
}
