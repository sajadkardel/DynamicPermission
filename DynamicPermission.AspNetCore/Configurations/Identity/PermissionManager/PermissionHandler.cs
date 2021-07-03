using System;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Threading.Tasks;
using DynamicPermission.AspNetCore.Repository.AppSetting;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace DynamicPermission.AspNetCore.Configurations.Identity.PermissionManager
{
    public class PermissionHandler : AuthorizationHandler<PermissionRequirement>
    {
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly IMemoryCache _memoryCache;
        private readonly IDataProtector _protectorToken;
        private readonly IAppSettingService _appSettingService;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public PermissionHandler(IHttpContextAccessor contextAccessor, IMemoryCache memoryCache, IDataProtectionProvider dataProtectionProvider, IAppSettingService appSettingService, SignInManager<IdentityUser> signInManager, UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _contextAccessor = contextAccessor;
            _memoryCache = memoryCache;
            _protectorToken = dataProtectionProvider.CreateProtector("RvgGuid"); ;
            _appSettingService = appSettingService;
            _signInManager = signInManager;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
        {
            var httpContext = _contextAccessor.HttpContext;
            var userId = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier); if (string.IsNullOrEmpty(userId)) return;
            var user = await _userManager.FindByIdAsync(userId); if (user == null) return;
            var userRoleNames = await _userManager.GetRolesAsync(user); if (!userRoleNames.Any()) return;

            var dbRoleValidationGuid = _memoryCache.GetOrCreate("RoleValidationGuid", p =>
            {
                p.AbsoluteExpiration = DateTimeOffset.MaxValue;
                return _appSettingService.DataBaseRoleValidationGuid();
            });

            SplitUserRequestedUrl(httpContext, out var areaAndActionAndControllerName);
            UnprotectRvgCookieData(httpContext, out var unprotectedRvgCookie);

            if (!IsRvgCookieDataValid(unprotectedRvgCookie, userId, dbRoleValidationGuid))
            {
                AddOrUpdateRvgCookie(httpContext, dbRoleValidationGuid, userId);
                await _signInManager.RefreshSignInAsync(user);

                foreach (var userRoleName in userRoleNames)
                {
                    var userRole = await _roleManager.Roles.SingleOrDefaultAsync(identityRole => identityRole.Name == userRoleName);
                    var roleClaims = await _roleManager.GetClaimsAsync(userRole);
                    if (roleClaims.Any(claim => claim.Type == userRole.Name && claim.Value == areaAndActionAndControllerName))
                    {
                        context.Succeed(requirement);
                    }
                }
            }
            else
            {
                foreach (var userRoleName in userRoleNames)
                {
                    var userRole = await _roleManager.Roles.SingleOrDefaultAsync(identityRole => identityRole.Name == userRoleName);
                    var roleClaims = await _roleManager.GetClaimsAsync(userRole);
                    if (roleClaims.Any(claim => claim.Type == userRole.Name && claim.Value == areaAndActionAndControllerName))
                    {
                        context.Succeed(requirement);
                    }
                }
            }

        }

        #region Methods

        private static void SplitUserRequestedUrl(HttpContext httpContext, out string areaAndControllerAndActionName)
        {
            var areaName = httpContext.Request.RouteValues["area"]?.ToString() ?? "NoArea";
            var controllerName = httpContext.Request.RouteValues["controller"] + "Controller";
            var actionName = httpContext.Request.RouteValues["action"].ToString();
            areaAndControllerAndActionName = $"{areaName}|{controllerName}|{actionName}";
        }

        private void UnprotectRvgCookieData(HttpContext httpContext, out string unprotectedRvgCookie)
        {
            var protectedRvgCookie = httpContext.Request.Cookies
                .FirstOrDefault(t => t.Key == "RVG").Value;
            unprotectedRvgCookie = null;
            if (!string.IsNullOrEmpty(protectedRvgCookie))
            {
                try
                {
                    unprotectedRvgCookie = _protectorToken.Unprotect(protectedRvgCookie);
                }
                catch (CryptographicException)
                {
                }
            }
        }

        private bool IsRvgCookieDataValid(string rvgCookieData, string validUserId, string validRvg)
            => !string.IsNullOrEmpty(rvgCookieData) &&
               SplitUserIdFromRvgCookie(rvgCookieData) == validUserId &&
               SplitRvgFromRvgCookie(rvgCookieData) == validRvg;

        private string SplitUserIdFromRvgCookie(string rvgCookieData)
            => rvgCookieData.Split("|||")[1];

        private string SplitRvgFromRvgCookie(string rvgCookieData)
            => rvgCookieData.Split("|||")[0];

        private string CombineRvgWithUserId(string rvg, string userId)
            => rvg + "|||" + userId;

        private void AddOrUpdateRvgCookie(HttpContext httpContext, string validRvg, string validUserId)
        {
            var rvgWithUserId = CombineRvgWithUserId(validRvg, validUserId);
            var protectedRvgWithUserId = _protectorToken.Protect(rvgWithUserId);
            httpContext.Response.Cookies.Append("RVG", protectedRvgWithUserId,
                new CookieOptions
                {
                    MaxAge = TimeSpan.FromDays(90),
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Lax
                });
        }

        #endregion

    }
}