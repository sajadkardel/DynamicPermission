using DynamicPermission.AspNetCore.ViewModels.ManageRole;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using DynamicPermission.AspNetCore.Context;
using DynamicPermission.AspNetCore.Models;
using Microsoft.AspNetCore.Mvc;

namespace DynamicPermission.AspNetCore.Services
{
    public class Utilities : IUtilities
    {
        private readonly AppDbContext _dbContext;

        public Utilities(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public IList<ActionAndControllerName> AreaAndControllerAndActionName()
        {
            Assembly asm = Assembly.GetExecutingAssembly();
            var contradistinction = asm.GetTypes()
                .Where(type => typeof(Controller).IsAssignableFrom(type))
                .SelectMany(type =>
                    type.GetMethods(BindingFlags.Instance | BindingFlags.DeclaredOnly | BindingFlags.Public))
                .Select(x => new
                {
                    Controller = x.DeclaringType?.Name,
                    Action = x.Name,
                    Area = x.DeclaringType?.CustomAttributes.Where(c => c.AttributeType == typeof(AreaAttribute))
                });

            var list = new List<ActionAndControllerName>();

            foreach (var item in contradistinction)
            {
                if (item.Area.Count() != 0)
                {
                    list.Add(new ActionAndControllerName()
                    {
                        ControllerName = item.Controller,
                        ActionName = item.Action,
                        AreaName = item.Area.Select(v => v.ConstructorArguments[0].Value.ToString()).FirstOrDefault()
                    });
                }
                else
                {
                    list.Add(new ActionAndControllerName()
                    {
                        ControllerName = item.Controller,
                        ActionName = item.Action,
                        AreaName = null,
                    });
                }
            }

            return list.Distinct().ToList();
        }

        public IList<string> GetAllAreasNames()
        {
            Assembly asm = Assembly.GetExecutingAssembly();
            var contradistinction = asm.GetTypes()
                .Where(type => typeof(Controller).IsAssignableFrom(type))
                .SelectMany(type =>
                    type.GetMethods(BindingFlags.Instance | BindingFlags.DeclaredOnly | BindingFlags.Public))
                .Select(x => new
                {
                    Area = x.DeclaringType?.CustomAttributes.Where(c => c.AttributeType == typeof(AreaAttribute))

                });

            var list = new List<string>();

            foreach (var item in contradistinction)
            {
                list.Add(item.Area.Select(v => v.ConstructorArguments[0].Value.ToString()).FirstOrDefault());
            }

            if (list.All(string.IsNullOrEmpty))
            {
                return new List<string>();
            }

            list.RemoveAll(x => x == null);

            return list.Distinct().ToList();
        }

        public string DataBaseRoleValidationGuid()
        {
            var roleValidationGuid =
                _dbContext.AppSettings.SingleOrDefault(s => s.Key == "RoleValidationGuid")?.Value;

            while (roleValidationGuid == null)
            {
                _dbContext.AppSettings.Add(new AppSetting
                {
                    Key = "RoleValidationGuid",
                    Value = Guid.NewGuid().ToString(),
                    LastTimeChanged = DateTime.Now
                });

                _dbContext.SaveChanges();

                roleValidationGuid =
                    _dbContext.AppSettings.SingleOrDefault(s => s.Key == "RoleValidationGuid")?.Value;
            }

            return roleValidationGuid;
        }
    }
}
