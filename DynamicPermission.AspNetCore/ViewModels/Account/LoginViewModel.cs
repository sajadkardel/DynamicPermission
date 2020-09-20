using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace DynamicPermission.AspNetCore.ViewModels.Account
{
    public class LoginViewModel
    {
        public string UserName { get; set; }

        public string Password { get; set; }

        public bool RememberMe { get; set; }

        public string ReturnUrl { get; set; }
    }
}
