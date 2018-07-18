// Copyright © 2017 Dmitry Sikorsky. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using starsky.Interfaces;
using starsky.ViewModels.Account;

namespace starsky.Controllers
{
    public class AccountController : Controller
    {
        private IUserManager _userManager;

        public AccountController(IUserManager userManager)
        {
            _userManager = userManager;
        }
        
        public IActionResult Index()
        {
            return Json(User.Identity.IsAuthenticated.ToString());
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ActionName("Login")]
        public async Task<IActionResult> LoginPost(LoginViewModel model, string returnUrl = null)
        {
            ValidateResult validateResult = _userManager.Validate("Email", model.Email, model.Password);

            if (!validateResult.Success) return View(model);
            await _userManager.SignIn(HttpContext, validateResult.User,model.RememberMe);
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToLocal(returnUrl);
            }

            return View(model);
        }

        [HttpPost]
        public IActionResult Logout()
        {
            this._userManager.SignOut(this.HttpContext);
            return this.RedirectToAction("Login");
        }
        
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model, bool json = false, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (ModelState.IsValid)
            {
                var result = _userManager.SignUp("", "email", model.Email, model.Password);
                if (json && result.Success) return Json("Account Created");
                if(result.Success) return RedirectToLocal(returnUrl);
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }
        
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