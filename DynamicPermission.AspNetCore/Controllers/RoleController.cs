using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using System.Threading.Tasks;
using DynamicPermission.AspNetCore.Common.Extensions;
using DynamicPermission.AspNetCore.ViewModels.Role;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace DynamicPermission.AspNetCore.Controllers
{
    [AllowAnonymous]
    [DisplayName("RoleController")]
    public class RoleController : Controller
    {
        private readonly RoleManager<IdentityRole> _roleManager;

        public RoleController(RoleManager<IdentityRole> roleManager)
        {
            _roleManager = roleManager;
        }

        [DisplayName("Index")]
        public IActionResult Index()
        {
            var roles = _roleManager.Roles.ToList();
            return View(roles);
        }

        [HttpGet]
        [DisplayName("Add")]
        public IActionResult Add()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Add(IdentityRole model)
        {
            var result = await _roleManager.CreateAsync(model);
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return RedirectToAction("Index");
        }

        [DisplayName("Delete")]
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();
            var role = await _roleManager.FindByIdAsync(id);
            if (role == null) return NotFound();
            await _roleManager.DeleteAsync(role);

            return RedirectToAction("Index");
        }

        #region RoleClaims

        [HttpGet]
        [DisplayName("RoleClaims")]
        public async Task<IActionResult> RoleClaims(string id)
        {
            var role = await _roleManager.FindByIdAsync(id);
            var roleClaims = await _roleManager.GetClaimsAsync(role);
            var areaViewModels = Assembly.GetEntryAssembly().GetAreaControllerActionNames(roleClaims);

            return View(new RoleClaimsViewModel
            {
                Id = id,
                AreaViewModels = areaViewModels
            });
        }

        [HttpPost]
        public async Task<IActionResult> RoleClaims(RoleClaimsViewModel model)
        {
            var role = await _roleManager.FindByIdAsync(model.Id);
            var roleClaims = await _roleManager.GetClaimsAsync(role);

            if (!model.SelectedClaims.Any()) throw new Exception("Choose at least one permission");

            foreach (var roleClaim in roleClaims)
            {
                await _roleManager.RemoveClaimAsync(role, roleClaim);
            }
            foreach (var selectedClaim in model.SelectedClaims)
            {
                await _roleManager.AddClaimAsync(role, new Claim(role.Name, selectedClaim));
            }

            return Ok();
        }

        #endregion

    }
}