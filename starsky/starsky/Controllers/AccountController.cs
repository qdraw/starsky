// Copyright © 2017 Dmitry Sikorsky. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using starsky.foundation.database.Models.Account;
using starsky.foundation.platform.Models;
using starsky.Helpers;
using starskycore.Interfaces;
using starskycore.ViewModels.Account;

namespace starsky.Controllers
{
    public class AccountController : Controller
    {
        private readonly IUserManager _userManager;
        private readonly AppSettings _appSettings;
        private readonly IAntiforgery _antiForgery;

        public AccountController(IUserManager userManager, AppSettings appSettings, IAntiforgery antiForgery)
        {
            _userManager = userManager;
            _appSettings = appSettings;
            _antiForgery = antiForgery;
        }
        
		/// <summary>
		/// Check the account status of the login
		/// </summary>
		/// <response code="200">logged in</response>
		/// <response code="401">when not logged in</response>
		/// <response code="406">There are no accounts, you must create an account first</response>
		/// <returns>account name, id, and create date</returns>
		[HttpGet("/account/status")]
		[ProducesResponseType(typeof(User), 200)]
		[ProducesResponseType(typeof(string), 401)]
		[ProducesResponseType(typeof(string), 406)]
		[Produces("application/json")]
		public IActionResult Status()
		{
			if ( !_userManager.AllUsers().Any() )
			{
				Response.StatusCode = 406;
				return Json("There are no accounts, you must create an account first");
			}
			
			if ( !User.Identity.IsAuthenticated ) return Unauthorized("false");

			// use model to avoid circular references
			var model = new User
			{
				Name = _userManager.GetCurrentUser(HttpContext)?.Name,
				Id = _userManager.GetCurrentUser(HttpContext).Id,
				Created = _userManager.GetCurrentUser(HttpContext).Created,
			};

			return Json(model);
		}

		
		/// <summary>
		/// Login form page
		/// </summary>
		/// <returns></returns>
		/// <response code="200">Login form page</response>
		[HttpGet("/account/login")]
		[ProducesResponseType(200)]
		[Produces("text/html")]
		public IActionResult Login(string returnUrl = null)
		{
			new AntiForgeryCookie(_antiForgery).SetAntiForgeryCookie(HttpContext);
			return PhysicalFile(Path.Combine(Directory.GetCurrentDirectory(), "clientapp", "build", "index.html"), "text/html");
		}
		
        /// <summary>
        /// Login the current HttpContext in
        /// </summary>
        /// <param name="model">Email, password and remember me bool</param>
        /// <returns>Login status</returns>
        /// <response code="200">successful login</response>
        /// <response code="401">login failed</response>
        [HttpPost("/account/login")]
        [ProducesResponseType(typeof(string),200)]
        [ProducesResponseType(typeof(string),401)]
        [Produces("application/json")]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            ValidateResult validateResult = _userManager.Validate("Email", model.Email, model.Password);

            if (!validateResult.Success)
            {
                Response.StatusCode = 401;
                return Json("Login failed");
            } 
            
            await _userManager.SignIn(HttpContext, validateResult.User,model.RememberMe);
            if ( User.Identity.IsAuthenticated)
            {
	            return Json("Login Success");
            }

            Response.StatusCode = 500;
            return Json("Login failed");
        }

        /// <summary>
        /// Logout the current HttpContext
        /// </summary>
        /// <returns></returns>
        /// <response code="200">successful logout</response>
        [HttpGet("/account/logout")]
        [ProducesResponseType(200)]
        public IActionResult Logout(string returnUrl = null)
        {
            _userManager.SignOut(HttpContext);
            return RedirectToAction(nameof(Login), new {ReturnUrl = returnUrl});
        }
        
        /// <summary>
        /// Update password for current user
        /// </summary>
        /// <param name="model">Password, ChangedPassword and ChangedConfirmPassword</param>
        /// <returns>Login status</returns>
        /// <response code="200">successful login</response>
        /// <response code="400">Model is not correct</response>
        /// <response code="401"> please login first or your current password is not correct</response>
        [HttpPost("/api/account/change-secret")]
        [ProducesResponseType(typeof(string),200)]
        [ProducesResponseType(typeof(string),400)]
        [ProducesResponseType(typeof(string),401)]
        [Produces("application/json")]
        [Authorize]
        public IActionResult ChangeSecret(ChangePasswordViewModel model)
        {
	        if ( !User.Identity.IsAuthenticated ) return Unauthorized("please login first");

	        if ( !ModelState.IsValid || model.ChangedPassword != model.ChangedConfirmPassword )
		        return BadRequest("Model is not correct");
	        
	        var currentUserId =
		        _userManager.GetCurrentUser(HttpContext).Id;

	        var credential = _userManager.GetCredentialsByUserId(currentUserId);

	        // Re-check password
	        var validateResult =
		        _userManager.Validate("Email", credential.Identifier, model.Password);
	        if ( !validateResult.Success )
	        {
		        return Unauthorized("Password is not correct");
	        }

	        var changeSecretResult =
		        _userManager.ChangeSecret("Email", credential.Identifier,
			        model.ChangedPassword);

	        return Json(changeSecretResult);
        }

        /// <summary>
        /// Create a new user
        /// </summary>
        /// <param name="model">with the userdata</param>
        /// <returns>redirect or json</returns>
        /// <response code="200">successful register</response>
        /// <response code="400">Wrong model or Wrong AntiForgeryToken</response>
        /// <response code="403">Account Register page is closed</response>
        [HttpPost("/account/register")]
        [ProducesResponseType(typeof(string),200)]
        [ProducesResponseType(typeof(string),400)]
        [ProducesResponseType(typeof(string),403)]
        [Produces("application/json")]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public IActionResult Register(RegisterViewModel model)
        {
	        if ( IsAccountRegisterClosed(User.Identity.IsAuthenticated) )
	        {
		        Response.StatusCode = 403;
		        return Json("Account Register page is closed");
	        }
	        
            if (ModelState.IsValid && model.ConfirmPassword == model.Password)
            {
                _userManager.SignUp(model.Name, "email", model.Email, model.Password );
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
        private bool IsAccountRegisterClosed(bool userIdentityIsAuthenticated)
        {
	        if ( userIdentityIsAuthenticated ) return false;
	        return !_appSettings.IsAccountRegisterOpen && _userManager.AllUsers().Any();
        }
        
        /// <summary>
        /// Is the register form open
        /// </summary>
        /// <returns></returns>
        /// <response code="200">Account Register page is open</response>
        /// <response code="403">Account Register page is closed</response>
        [HttpGet("/account/register/status")]
        [ProducesResponseType(typeof(string),200)]
        [ProducesResponseType(typeof(string),403)]
        [Produces("application/json")]
        public IActionResult RegisterStatus()
        {
	        if ( !IsAccountRegisterClosed(User.Identity.IsAuthenticated) ) return Json("RegisterStatus open");
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
