// Copyright © 2017 Dmitry Sikorsky. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.WindowsAzure.Storage.Shared.Protocol;
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
        public async Task<IActionResult> Register(RegisterViewModel model, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (ModelState.IsValid)
            {
                var result = _userManager.SignUp("", "email", model.Email, model.Password);
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

//using System;
//using System.Threading.Tasks;
//using Microsoft.AspNetCore.Authentication;
//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Identity;
//using Microsoft.AspNetCore.Mvc;
//using starsky.Models;
//using starsky.Models.AccountViewModels;
//using starsky.Models.ManageViewModels;
//
//namespace starsky.Controllers
//{
//    [Authorize]
//    public class AccountController : Controller
//    {
//        private readonly UserManager<ApplicationUser> _userManager;
//        private readonly SignInManager<ApplicationUser> _signInManager;
//
//        public AccountController(
//            UserManager<ApplicationUser> userManager,
//            SignInManager<ApplicationUser> signInManager)
//        {
//            _userManager = userManager;
//            _signInManager = signInManager;
//        }
//
//        [TempData]
//        public string ErrorMessage { get; set; }
//        public string StatusMessage { get; set; }
//
//        [HttpGet]
//        public async Task<IActionResult> Index()
//        {
//            var user = await _userManager.GetUserAsync(User);
//            if (user == null)
//            {
//                throw new  MissingMemberException($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
//            }
//
//            var model = new IndexViewModel
//            {
//                Username = user.UserName,
//                Email = user.Email,
//                PhoneNumber = user.PhoneNumber,
//                IsEmailConfirmed = user.EmailConfirmed,
//                StatusMessage = StatusMessage
//            };
//
//            return View(model);
//        }
//
//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> Index(IndexViewModel model)
//        {
//            if (!ModelState.IsValid)
//            {
//                return View(model);
//            }
//
//            var user = await _userManager.GetUserAsync(User);
//            if (user == null)
//            {
//                throw new MissingMemberException($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
//            }
//
//            var email = user.Email;
//            if (model.Email != email)
//            {
//                var setEmailResult = await _userManager.SetEmailAsync(user, model.Email);
//                if (!setEmailResult.Succeeded)
//                {
//                    throw new MissingFieldException($"Unexpected error occurred setting email for user with ID '{user.Id}'.");
//                }
//            }
//
//            var phoneNumber = user.PhoneNumber;
//            if (model.PhoneNumber != phoneNumber)
//            {
//                var setPhoneResult = await _userManager.SetPhoneNumberAsync(user, model.PhoneNumber);
//                if (!setPhoneResult.Succeeded)
//                {
//                    throw new MissingFieldException($"Unexpected error occurred setting phone number for user with ID '{user.Id}'.");
//                }
//            }
//
//            StatusMessage = "Your profile has been updated";
//            return RedirectToAction(nameof(Index));
//        }
//        
//        [HttpGet]
//        public async Task<IActionResult> ChangePassword()
//        {
//            var user = await _userManager.GetUserAsync(User);
//            if (user == null)
//            {
//                throw new MissingMemberException($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
//            }
//
//            var hasPassword = await _userManager.HasPasswordAsync(user);
//            if (!hasPassword)
//            {
//                return RedirectToAction(nameof(Index));
//            }
//
//            var model = new ChangePasswordViewModel { StatusMessage = StatusMessage };
//            return View(model);
//        }
//        
//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
//        {
//            if (!ModelState.IsValid)
//            {
//                return View(model);
//            }
//
//            var user = await _userManager.GetUserAsync(User);
//            if (user == null)
//            {
//                throw new MissingMemberException($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
//            }
//
//            var changePasswordResult = await _userManager.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);
//            if (!changePasswordResult.Succeeded)
//            {
//                AddErrors(changePasswordResult);
//                return View(model);
//            }
//
//            await _signInManager.SignInAsync(user, isPersistent: false);
//            StatusMessage = "Your password has been changed.";
//
//            return RedirectToAction(nameof(ChangePassword));
//        }
//        
//        [HttpGet]
//        [AllowAnonymous]
//        public async Task<IActionResult> Login(string returnUrl = null)
//        {
//            // Clear the existing external cookie to ensure a clean login process
//            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);
//
//            ViewData["ReturnUrl"] = returnUrl;
//            return View();
//        }
//
//        [HttpPost]
//        [AllowAnonymous]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
//        {
//            ViewData["ReturnUrl"] = returnUrl;
//            if (ModelState.IsValid)
//            {
//                // This doesn't count login failures towards account lockout
//                // To enable password failures to trigger account lockout, set lockoutOnFailure: true
//                var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);
//                if (result.Succeeded)
//                {
//                    return RedirectToLocal(returnUrl);
//                }
//                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
//                return View(model);
//            }
//
//            // If we got this far, something failed, redisplay form
//            return View(model);
//        }
//
//        [HttpGet]
//        [AllowAnonymous]
//        public IActionResult Register(string returnUrl = null)
//        {
//            ViewData["ReturnUrl"] = returnUrl;
//            return View();
//        }
//
//        [HttpPost]
//        [AllowAnonymous]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> Register(RegisterViewModel model, string returnUrl = null)
//        {
//            ViewData["ReturnUrl"] = returnUrl;
//            if (ModelState.IsValid)
//            {
//                var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
//                var result = await _userManager.CreateAsync(user, model.Password);
//                if (result.Succeeded)
//                {
//                    await _signInManager.SignInAsync(user, isPersistent: false);
//                    return RedirectToLocal(returnUrl);
//                }
//                AddErrors(result);
//            }
//
//            // If we got this far, something failed, redisplay form
//            return View(model);
//        }
//
//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> Logout()
//        {
//            await _signInManager.SignOutAsync();
//            return RedirectToAction(nameof(HomeController.Index), "Home");
//        }
//
//
//        #region Helpers
//
//        private void AddErrors(IdentityResult result)
//        {
//            foreach (var error in result.Errors)
//            {
//                ModelState.AddModelError(string.Empty, error.Description);
//            }
//        }
//

//
//        #endregion
//    }
//}
