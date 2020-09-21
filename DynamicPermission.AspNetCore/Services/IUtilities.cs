using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DynamicPermission.AspNetCore.ViewModels.ManageRole;
using DynamicPermission.AspNetCore.ViewModels.ServiceViewModel;

namespace DynamicPermission.AspNetCore.Services
{
    public interface IUtilities
    {
        public IList<AreaControllerActionName> AreaAndControllerAndActionName();
        public IList<string> GetAllAreasNames();
        public string DataBaseRoleValidationGuid();
    }
}
