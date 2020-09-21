using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using DynamicPermission.AspNetCore.Context;
using DynamicPermission.AspNetCore.Services;
using DynamicPermission.AspNetCore.ViewModels.ManageRole;
using DynamicPermission.AspNetCore.ViewModels.ManageUser;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace DynamicPermission.AspNetCore.Controllers
{
    public class ManageRoleController : Controller
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IUtilities _utilities;
        private readonly IMemoryCache _memoryCache;

        public ManageRoleController(RoleManager<IdentityRole> roleManager, IUtilities utilities, IMemoryCache memoryCache)
        {
            _roleManager = roleManager;
            _utilities = utilities;
            _memoryCache = memoryCache;
        }

        public IActionResult Index()
        {
            var roles = _roleManager.Roles.ToList();
            return View(roles);
        }

        [HttpGet]
        public IActionResult AddRole()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddRole(IdentityRole model)
        {
            var result = await _roleManager.CreateAsync(model);
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> DeleteRole(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();
            var role = await _roleManager.FindByIdAsync(id);
            if (role == null) return NotFound();
            await _roleManager.DeleteAsync(role);

            return RedirectToAction("Index");
        }

        #region AddPermissionToRole

        [HttpGet]
        public async Task<IActionResult> AddPermissionToRole(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();
            var role = await _roleManager.FindByIdAsync(id);
            if (role == null) return NotFound();
            var permissions = _utilities.AreaAndControllerAndActionName().ToList();
            List<string> permissionTypes = new List<string>();
            foreach (var actionAndControllerName in permissions)
            {
                var areaName = (string.IsNullOrEmpty(actionAndControllerName.AreaName)) ? "NoArea" : actionAndControllerName.AreaName;
                permissionTypes.Add($"{areaName}|{actionAndControllerName.ControllerName}|{actionAndControllerName.ActionName}");
            }
            var rolePermissions = await _roleManager.GetClaimsAsync(role);
            var rolePermissionTypes = rolePermissions.Select(claim => claim.Type).ToList();
            var validPermissions = permissionTypes.Where(r => !rolePermissionTypes.Contains(r))
                .Select(r => new RolePermissionViewModel(r)).ToList();
            var model = new AddPermissionToRoleViewModel(id, validPermissions);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddPermissionToRole(AddPermissionToRoleViewModel model)
        {
            if (model == null) return NotFound();
            var role = await _roleManager.FindByIdAsync(model.RoleId);
            if (role == null) return NotFound();
            var requestPermissions = model.RolePermissions.Where(r => r.IsSelected).ToList();
            foreach (var requestPermission in requestPermissions)
            {
                var result = await _roleManager.AddClaimAsync(role, new Claim(requestPermission.PermissionName, true.ToString()));
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                    return View(model);
                }
            }

            return RedirectToAction("Index");
        }

        #endregion

        #region RemovePermissionFromRole

        [HttpGet]
        public async Task<IActionResult> RemovePermissionFromRole(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();
            var role = await _roleManager.FindByIdAsync(id);
            if (role == null) return NotFound();

            var rolePermissions = await _roleManager.GetClaimsAsync(role);
            var validRoles = rolePermissions.Select(r => new RolePermissionViewModel(r.Type)).ToList();
            var model = new AddPermissionToRoleViewModel(id, validRoles);

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> RemovePermissionFromRole(AddPermissionToRoleViewModel model)
        {
            if (model == null) return NotFound();
            var role = await _roleManager.FindByIdAsync(model.RoleId);
            if (role == null) return NotFound();
            var requestPermissions = model.RolePermissions.Where(r => r.IsSelected)
                .Select(u => u.PermissionName).ToList();
            foreach (var requestPermission in requestPermissions)
            {
                var result = await _roleManager.RemoveClaimAsync(role, new Claim(requestPermission, true.ToString()));
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }

            }

            return RedirectToAction("index");
        }

        #endregion

    }
}