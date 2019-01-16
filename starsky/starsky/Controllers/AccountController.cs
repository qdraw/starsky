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

namespace starsky.Controllers
{
    public class AccountController : Controller
    {
        private readonly IUserManager _userManager;

        public AccountController(IUserManager userManager)
        {
            _userManager = userManager;
        }
        
	    [HttpGet("/account")]
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
        [HttpGet]
        public IActionResult Login(string returnUrl = null)
        {

	        var test = new List<FileIndexItem>().ToHashSet();
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        /// <summary>
        /// Login the current HttpContext in
        /// </summary>
        /// <param name="model">Email, password and remember me bool</param>
        /// <param name="returnUrl">null or localurl</param>
        /// <returns></returns>
        [HttpPost]
        [ActionName("Login")]
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
        [HttpGet]
        public IActionResult Logout()
        {
            _userManager.SignOut(HttpContext);
            return RedirectToAction("Login");
        }
        
        /// <summary>
        /// View the Register form
        /// </summary>
        /// <param name="returnUrl">when succesfull continue</param>
        /// <returns></returns>
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        /// <summary>
        /// Create a new user
        /// </summary>
        /// <param name="model"></param>
        /// <param name="json"></param>
        /// <param name="returnUrl"></param>
        /// <returns></returns>
        [HttpPost]
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