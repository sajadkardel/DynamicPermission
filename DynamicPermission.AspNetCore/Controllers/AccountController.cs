using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.Web.CodeGeneration.Utils.Messaging;

namespace DynamicPermission.AspNetCore.Controllers
{
    [AllowAnonymous]
    [DisplayName("AccountController(just for show)")]
    public class AccountController : Controller
    {
        private readonly SignInManager<IdentityUser> _signInManager;

        public AccountController(SignInManager<IdentityUser> signInManager)
        {
            _signInManager = signInManager;
        }

        [HttpGet]
        [DisplayName("Login")]
        public IActionResult Login(string returnUrl = null)
        {
            if (_signInManager.IsSignedIn(User))
                return RedirectToAction("Index", "Home");

            ViewData["returnUrl"] = returnUrl;
            return View(model:returnUrl);
        }

        [HttpPost]
        public async Task<IActionResult> Login(string userName,string password,bool rememberMe, string returnUrl = null)
        {
            if (_signInManager.IsSignedIn(User))
                return RedirectToAction("Index", "Home");

            ViewData["returnUrl"] = returnUrl;

            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(userName, password, rememberMe,false);
                if (result.Succeeded)
                {
                    if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                        return Redirect(returnUrl);

                    return RedirectToAction("Index", "Home");
                }

                ModelState.AddModelError(string.Empty, "رمزعبور یا نام کاربری اشتباه است");
            }

            return View(model:returnUrl);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [DisplayName("LogOut")]
        public async Task<IActionResult> LogOut()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

       
        public IActionResult AccessDenied()
        {
            return View();
        }

    }
}