using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DynamicPermission.AspNetCore.Context;

namespace DynamicPermission.AspNetCore.Repository.AppSetting
{
    public class AppSettingService : IAppSettingService
    {
        private readonly AppDbContext _dbContext;
        public AppSettingService(AppDbContext context)
        {
            _dbContext = context;
        }

        public string DataBaseRoleValidationGuid()
        {
            var roleValidationGuid = _dbContext.AppSettings.SingleOrDefault(s => s.Key == "RoleValidationGuid")?.Value;

            while (roleValidationGuid == null)
            {
                _dbContext.AppSettings.Add(new Entities.AppSetting
                {
                    Key = "RoleValidationGuid",
                    Value = Guid.NewGuid().ToString(),
                    LastTimeChanged = DateTime.Now
                });

                _dbContext.SaveChanges();

                roleValidationGuid = _dbContext.AppSettings.SingleOrDefault(s => s.Key == "RoleValidationGuid")?.Value;
            }

            return roleValidationGuid;
        }
    }
}
