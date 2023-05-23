using BookHiveDB.Domain;
using BookHiveDB.Domain.DTO;
using BookHiveDB.Domain.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace BookHiveDB.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly SignInManager<BookHiveApplicationUser> signInManager;
        public HomeController(ILogger<HomeController> logger, SignInManager<BookHiveApplicationUser> signInManager)
        {
            this.signInManager = signInManager;
            _logger = logger;
        }

        public async Task<IActionResult> Index(string returnUrl)
        {
            UserLoginDto model = new UserLoginDto { ReturnUrl = returnUrl, ExternalLogins = (await signInManager.GetExternalAuthenticationSchemesAsync()).ToList() };

            return View(model);
        }


        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
