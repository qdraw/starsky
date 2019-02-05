// Copyright © 2017 Dmitry Sikorsky. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using starsky.ViewModels.Account;
using starskycore.Interfaces;
using starskycore.Models;
using starskycore.Models.Account;

namespace starsky.Controllers
{
    public class AccountController : Controller
    {
        private readonly IUserManager _userManager;

        public AccountController(IUserManager userManager)
        {
            _userManager = userManager;
        }
        
	    /// <summary>
	    /// View Account settings
	    /// </summary>
	    /// <param name="json">true => 200 == success, 401 is not logged</param>
	    /// <returns>account page or status</returns>
	    /// <response code="200">User exist</response>
	    /// <response code="401">when using json=true, not logged in</response>
	    [HttpGet("/account")]
	    [ProducesResponseType(typeof(User), 200)]
	    [ProducesResponseType(401)]
	    
        public IActionResult Index(bool json = false)
	    {
		    if ( json && !User.Identity.IsAuthenticated ) return Unauthorized();
            if (!User.Identity.IsAuthenticated) return RedirectToLocal(null);
	        if ( json ) return Json(_userManager.GetCurrentUser(HttpContext));
			return View(_userManager.GetCurrentUser(HttpContext));
        }

        /// <summary>
        /// Login form page
        /// </summary>
        /// <returns></returns>
        /// <response code="200">Login form page</response>
        [HttpGet("/account/login")]
        [ProducesResponseType(200)]
        public IActionResult Login(string returnUrl = null)
        {

            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        /// <summary>
        /// Login the current HttpContext in
        /// </summary>
        /// <param name="model">Email, password and remember me bool</param>
        /// <param name="returnUrl">null or localurl</param>
        /// <returns></returns>
 	    /// <response code="200">successful login</response>
        /// <response code="401">login failed</response>
        [HttpPost("/account/login")]
        [ProducesResponseType(200)]
        public async Task<IActionResult> LoginPost(LoginViewModel model, string returnUrl = null)
        {
            ValidateResult validateResult = _userManager.Validate("Email", model.Email, model.Password);
            ViewData["ReturnUrl"] = returnUrl;

            if (!validateResult.Success)
            {
                Response.StatusCode = 401;
                ModelState.AddModelError("All", "Login Failed");
                return View(model);
            } 
            
            await _userManager.SignIn(HttpContext, validateResult.User,model.RememberMe);
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToLocal(returnUrl);
            }
            return View(model);
        }

        /// <summary>
        /// Logout the current HttpContext
        /// </summary>
        /// <returns></returns>
        /// <response code="200">successful logout</response>
        [HttpGet("/account/logout")]
        [ProducesResponseType(200)]
        public IActionResult Logout()
        {
            _userManager.SignOut(HttpContext);
            return RedirectToAction("Login");
        }
        
        /// <summary>
        /// View the Register form
        /// </summary>
        /// <param name="returnUrl">when successful continue</param>
        /// <returns></returns>
        /// <response code="200">successful Register-page</response>
        [HttpGet("/account/register")]
        [AllowAnonymous]
        [ProducesResponseType(200)]
        public IActionResult Register(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        /// <summary>
        /// Create a new user
        /// </summary>
        /// <param name="model">with the userdata</param>
        /// <param name="json">get a json response</param>
        /// <param name="returnUrl">to redirect if json=false</param>
        /// <returns>redirect or json</returns>
        /// <response code="200">successful register</response>
        [HttpPost("/account/register")]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model, bool json = false, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (ModelState.IsValid && model.ConfirmPassword == model.Password)
            {
                var result = _userManager.SignUp("", "email", model.Email, model.Password);
                if (json && result.Success) return Json("Account Created");
                if(result.Success) return RedirectToLocal(returnUrl);
            }

            // If we got this far, something failed, redisplay form
            Response.StatusCode = 400;
            if (json) return Json("Model is not correct");
            return View(model);
        }
        
        /// <summary>
        /// When this url is local 302 redirect,
        /// </summary>
        /// <param name="returnUrl">the url to redirect, when null redirect to home</param>
        /// <returns>302 redirect</returns>
        private IActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction(nameof(HomeController.Index), "Home");
        }
        
    }
}