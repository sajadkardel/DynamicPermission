using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DynamicPermission.AspNetCore.ViewModels.ManageRole;

namespace DynamicPermission.AspNetCore.Services
{
    public interface IUtilities
    {
        public IList<ActionAndControllerName> AreaAndControllerAndActionName();
        public IList<string> GetAllAreasNames();
        public string DataBaseRoleValidationGuid();
    }
}
