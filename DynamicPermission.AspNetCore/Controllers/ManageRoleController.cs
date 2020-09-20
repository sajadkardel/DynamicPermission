using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using DynamicPermission.AspNetCore.Services;
using DynamicPermission.AspNetCore.ViewModels.ManageRole;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
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
            var model = new List<IndexViewModel>();
            foreach (var role in roles)
            {
                model.Add(new IndexViewModel()
                {
                    RoleName = role.Name,
                    RoleId = role.Id
                });
            }
            return View(model);
        }

        [HttpGet]
        public IActionResult AddRole()
        {
            var allMvcNames =
                _memoryCache.GetOrCreate("AreaAndActionAndControllerNamesList", p =>
                {
                    p.AbsoluteExpiration = DateTimeOffset.MaxValue;
                    return _utilities.AreaAndControllerAndActionName();
                });
            var model = new AddRoleViewModel()
            {
                ActionAndControllerNames = allMvcNames
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> AddRole(AddRoleViewModel model)
        {
            if (ModelState.IsValid)
            {
                var role = new IdentityRole(model.RoleName);
                var result = await _roleManager.CreateAsync(role);
                if (result.Succeeded)
                {
                    var requestRoles =
                        model.ActionAndControllerNames.Where(c => c.IsSelected).ToList();
                    foreach (var requestRole in requestRoles)
                    {
                        var areaName = (string.IsNullOrEmpty(requestRole.AreaName)) ?
                            "NoArea" : requestRole.AreaName;

                        await _roleManager.AddClaimAsync(role,
                            new Claim($"{areaName}|{requestRole.ControllerName}|{requestRole.ActionName}",
                                true.ToString()));
                    }


                    return RedirectToAction("Index");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }

            return View(model);
        }


        public async Task<IActionResult> Accesses(string id)
        {
            var role = await _roleManager.FindByIdAsync(id);
            var claims = await _roleManager.GetClaimsAsync(role);
            return View(claims);
        }

        public async Task<IActionResult> DeleteRole(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();
            var role = await _roleManager.FindByIdAsync(id);
            if (role == null) return NotFound();
            await _roleManager.DeleteAsync(role);

            return RedirectToAction("Index");
        }
    }
}