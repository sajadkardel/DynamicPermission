using System;
using System.Linq;
using DynamicPermission.AspNetCore.Context;
using DynamicPermission.AspNetCore.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace DynamicPermission.AspNetCore.Controllers
{
    public class AppSettingController : Controller
    {
        private readonly AppDbContext _dbContext;
        private readonly IMemoryCache _memoryCache;

        public AppSettingController(AppDbContext dbContext, IMemoryCache memoryCache)
        {
            _dbContext = dbContext;
            _memoryCache = memoryCache;
        }

        public IActionResult Index()
        {
            var model = _dbContext.AppSettings.ToList();
            return View(model);
        }

        [HttpGet]
        public IActionResult RoleValidationGuid()
        {
            var roleValidationGuidSiteSetting = _dbContext.AppSettings.FirstOrDefault(t => t.Key == "RoleValidationGuid");
            return View(roleValidationGuidSiteSetting);
        }

        [HttpGet]
        public IActionResult GenerateNewGuid()
        {
            var roleValidationGuidSiteSetting = _dbContext.AppSettings.FirstOrDefault(t => t.Key == "RoleValidationGuid");

            if (roleValidationGuidSiteSetting == null)
            {
                _dbContext.AppSettings.Add(new AppSetting
                {
                    Key = "RoleValidationGuid",
                    Value = Guid.NewGuid().ToString(),
                    LastTimeChanged = DateTime.Now
                });
            }
            else
            {
                roleValidationGuidSiteSetting.Value = Guid.NewGuid().ToString();
                roleValidationGuidSiteSetting.LastTimeChanged = DateTime.Now;
                _dbContext.Update(roleValidationGuidSiteSetting);
            }
            _dbContext.SaveChanges();
            _memoryCache.Remove("RoleValidationGuid");

            return RedirectToAction("Index");
        }

    }
}