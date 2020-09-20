using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DynamicPermission.AspNetCore.ViewModels.Account;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.Web.CodeGeneration.Utils.Messaging;

namespace DynamicPermission.AspNetCore.Controllers
{
    [AllowAnonymous]
    public class AccountController : Controller
    {
        private readonly SignInManager<IdentityUser> _signInManager;

        public AccountController(SignInManager<IdentityUser> signInManager)
        {
            _signInManager = signInManager;
        }

        [HttpGet]
        public async Task<IActionResult> Login(string returnUrl = null)
        {
            if (_signInManager.IsSignedIn(User))
                return RedirectToAction("Index", "Home");

            var model = new LoginViewModel()
            {
                ReturnUrl = returnUrl
            };

            ViewData["returnUrl"] = returnUrl;
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
        {
            if (_signInManager.IsSignedIn(User))
                return RedirectToAction("Index", "Home");

            model.ReturnUrl = returnUrl;
            ViewData["returnUrl"] = returnUrl;

            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(model.UserName, model.Password, model.RememberMe,false);
                if (result.Succeeded)
                {
                    if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                        return Redirect(returnUrl);

                    return RedirectToAction("Index", "Home");
                }

                ModelState.AddModelError(string.Empty, "رمزعبور یا نام کاربری اشتباه است");
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LogOut()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }
    }
}