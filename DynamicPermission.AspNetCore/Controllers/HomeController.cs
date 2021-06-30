using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DynamicPermission.AspNetCore.Controllers
{
    [AllowAnonymous]
    [DisplayName("HomeController(just for show)")]
    public class HomeController : Controller
    {
        [DisplayName("Index")]
        public IActionResult Index()
        {
            return View();
        }
    }
}