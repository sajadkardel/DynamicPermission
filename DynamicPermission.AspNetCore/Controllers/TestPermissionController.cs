using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DynamicPermission.AspNetCore.Controllers
{
    [Authorize("DynamicPermission")]
    [DisplayName("TestPermissionController(or any Name)")]
    public class TestPermissionController : Controller
    {
        [DisplayName("Index(or any Name)")]
        public IActionResult Index()
        {
            return View();
        }

        [DisplayName("Add(or any Name)")]
        public IActionResult Add()
        {
            return View();
        }

        [DisplayName("Edit(or any Name)")]
        public IActionResult Edit()
        {
            return View();
        }

        [DisplayName("Delete(or any Name)")]
        public IActionResult Delete()
        {
            return View();
        }
    }
}