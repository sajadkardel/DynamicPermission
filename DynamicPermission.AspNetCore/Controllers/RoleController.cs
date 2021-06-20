using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using System.Threading.Tasks;
using DynamicPermission.AspNetCore.Common.Extensions;
using DynamicPermission.AspNetCore.ViewModels.Role;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace DynamicPermission.AspNetCore.Controllers
{
    [DisplayName("RoleController(just for show)")]
    public class RoleController : Controller
    {
        private readonly RoleManager<IdentityRole> _roleManager;

        public RoleController(RoleManager<IdentityRole> roleManager)
        {
            _roleManager = roleManager;
        }

        [DisplayName("Index(just for show)")]
        public IActionResult Index()
        {
            var roles = _roleManager.Roles.ToList();
            return View(roles);
        }

        [HttpGet]
        [DisplayName("Add(just for show)")]
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

        [DisplayName("Delete(just for show)")]
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
        [DisplayName("RoleClaims(just for show)")]
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
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RoleClaims(RoleClaimsViewModel model)
        {
            var role = await _roleManager.FindByIdAsync(model.Id);
            var roleClaims = await _roleManager.GetClaimsAsync(role);

            if (model.SelectedClaims.Any())
            {
                foreach (var roleClaim in roleClaims)
                {
                    await _roleManager.RemoveClaimAsync(role, roleClaim);
                }
                foreach (var selectedClaim in model.SelectedClaims)
                {
                    await _roleManager.AddClaimAsync(role, new Claim(role.Name, selectedClaim));
                }
            }

            return RedirectToAction("Index");
        }

        #endregion

    }
}