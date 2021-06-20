using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using DynamicPermission.AspNetCore.ViewModels.User;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace DynamicPermission.AspNetCore.Controllers
{
    [DisplayName("UserController(just for show)")]
    public class UserController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UserController(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        [DisplayName("Index(just for show)")]
        public IActionResult Index()
        {
            var model = _userManager.Users.ToList();
            return View(model);
        }

        [DisplayName("Add(just for show)")]
        public IActionResult Add()
        {
            return View();
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(string userName,string passwordHash)
        {
            var user = new IdentityUser
            {
                UserName = userName
            };
            var result = await _userManager.CreateAsync(user, passwordHash);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                    return View();
                }
            }

            return RedirectToAction("Index");
        }

        [DisplayName("Delete(just for show)")]
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();
            await _userManager.DeleteAsync(user);

            return RedirectToAction("Index");
        }

        [HttpGet]
        [DisplayName("UpdateSecurityStamp(just for show)")]
        public async Task<IActionResult> UpdateSecurityStamp(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();
            await _userManager.UpdateSecurityStampAsync(user);
            return RedirectToAction("Index");
        }

        #region UserRoles

        [HttpGet]
        [DisplayName("UserRoles(just for show)")]
        public async Task<IActionResult> UserRoles(string id,bool isAdd)
        {
            var user = await _userManager.FindByIdAsync(id);
            var userRoles = await _userManager.GetRolesAsync(user);
            var roles = await _roleManager.Roles.AsNoTracking().ToListAsync();
            var validRoles = isAdd ? roles.Where(r => !userRoles.Contains(r.Name)).Select(role => role.Name).ToList() : userRoles.ToList();

            return View(new UserRolesViewModel
            {
                Id = id,
                IsAdd = isAdd,
                ValidRoles = validRoles
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UserRoles(UserRolesViewModel model)
        {
            var user = await _userManager.FindByIdAsync(model.Id);

            if (model.IsAdd)
            {
                await _userManager.AddToRolesAsync(user, model.SelectedRoles);
            }
            else
            {
                await _userManager.RemoveFromRolesAsync(user, model.SelectedRoles);
            }

            return RedirectToAction("Index");
        }

        #endregion
    }
}