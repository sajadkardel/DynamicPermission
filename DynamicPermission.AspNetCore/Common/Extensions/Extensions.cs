using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using System.Threading.Tasks;
using DynamicPermission.AspNetCore.Common.Models;
using Microsoft.AspNetCore.Mvc;

namespace DynamicPermission.AspNetCore.Common.Extensions
{
    public static class Extensions
    {
        public static List<AreaViewModel> GetAreaControllerActionNames(this Assembly assembly, IList<Claim> roleClaims)
        {
            var areaControllerActionList = assembly.GetTypes().Where(type => typeof(Controller).IsAssignableFrom(type))
                .SelectMany(type => type.GetMethods(BindingFlags.Instance | BindingFlags.DeclaredOnly | BindingFlags.Public))
                .Select(x => new
                {
                    Area = x.DeclaringType?.CustomAttributes.Where(c => c.AttributeType == typeof(AreaAttribute)),
                    Controller = x.DeclaringType,
                    Action = x
                }).ToList();

            var actionViewModels = areaControllerActionList.Select(arg => new ActionViewModel
            {
                Action = new Claim(arg.Action.Name, arg.Action.GetCustomAttribute<DisplayNameAttribute>()?.DisplayName ?? "NULL"),
                ParentController = new Claim(arg.Controller.Name, arg.Controller.GetTypeInfo().GetCustomAttribute<DisplayNameAttribute>()?.DisplayName ?? "NULL"),
                Selected = roleClaims != null && roleClaims.Select(claim => claim.Value).Contains($"{arg.Area.Select(v => v.ConstructorArguments[0].Value?.ToString()).FirstOrDefault() ?? "NoArea"}|{arg.Controller.Name}|{arg.Action.Name}")
            }).ToList();
            var controllerViewModels = areaControllerActionList.Select(arg => new ControllerViewModel
            {
                Controller = new Claim(arg.Controller.Name, arg.Controller.GetTypeInfo().GetCustomAttribute<DisplayNameAttribute>()?.DisplayName ?? "NULL"),
                ParentArea = new Claim(arg.Area.Select(v => v.ConstructorArguments[0].Value?.ToString()).FirstOrDefault() ?? "NoArea", arg.Area.Select(v => v.ConstructorArguments[0].Value?.ToString()).FirstOrDefault() ?? "NoArea"),
                ActionViewModels = actionViewModels.Where(model => model.ParentController.Type == arg.Controller?.Name && model.Action.Value != "NULL").ToList()
            }).ToList().GroupBy(model => model.Controller.Type).Select(models => models.First()).ToList();
            var areaViewModels = areaControllerActionList.Select(arg => new AreaViewModel
            {
                Area = new Claim(arg.Area.Select(v => v.ConstructorArguments[0].Value?.ToString()).FirstOrDefault() ?? "NoArea", arg.Area.Select(v => v.ConstructorArguments[0].Value?.ToString()).FirstOrDefault() ?? "NoArea"),
                ControllerViewModels = controllerViewModels.Where(model =>
                    model.ParentArea.Type == arg.Area.Select(v => v.ConstructorArguments[0].Value?.ToString()).FirstOrDefault() || model.ParentArea.Type == "NoArea").ToList()
                    .Where(model => model.Controller.Value != "NULL").ToList()
            }).ToList().GroupBy(model => model.Area.Type).Select(models => models.First()).ToList();

            return areaViewModels;
        }
    }
}
