using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace DynamicPermission.AspNetCore.Controllers
{
    [DisplayName("HomeController(just for show)")]
    public class HomeController : Controller
    {
        [DisplayName("Index(just for show)")]
        public IActionResult Index()
        {
            return View();
        }
    }
}