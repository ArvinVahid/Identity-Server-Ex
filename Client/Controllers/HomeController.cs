using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace Client.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        [Authorize]
        public async Task<IActionResult> Secret()
        {
            var token = HttpContext.GetTokenAsync("access_token");
            return View();
        }

        public IActionResult Token()
        {
            return View();
        }
    }
}