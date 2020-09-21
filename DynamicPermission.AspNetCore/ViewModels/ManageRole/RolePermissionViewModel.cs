using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DynamicPermission.AspNetCore.ViewModels.ManageRole
{
    public class AddPermissionToRoleViewModel
    {
        #region Constructor

        public AddPermissionToRoleViewModel()
        {
            RolePermissions = new List<RolePermissionViewModel>();
        }

        public AddPermissionToRoleViewModel(string roleId, IList<RolePermissionViewModel> rolePermissions)
        {
            RoleId = roleId;
            RolePermissions = rolePermissions;
        }


        #endregion

        public string RoleId { get; set; }

        public IList<RolePermissionViewModel> RolePermissions { get; set; }
    }

    public class RolePermissionViewModel
    {

        #region Constructor

        public RolePermissionViewModel()
        {
        }

        public RolePermissionViewModel(string permissionName)
        {
            PermissionName = permissionName;
        }


        #endregion

        public string PermissionName { get; set; }
        public bool IsSelected { get; set; }
    }
}
