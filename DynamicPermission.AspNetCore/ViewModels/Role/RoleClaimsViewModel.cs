using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DynamicPermission.AspNetCore.Common.Models;

namespace DynamicPermission.AspNetCore.ViewModels.Role
{
    public class RoleClaimsViewModel
    {
        public string Id { get; set; }
        public List<string> SelectedClaims { get; set; }
        public List<AreaViewModel> AreaViewModels { get; set; }
    }

}
