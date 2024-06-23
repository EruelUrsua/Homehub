using HomeHub.App.Models;
using HomeHub.DataModel;
using Microsoft.AspNetCore.Mvc;

namespace HomeHub.App.Controllers
{
    public class UserTypeController : Controller
    {
        private readonly HomeHubContext _context;
        public IActionResult Register()
        {
            return View(new RegisterViewModel());
        }

        public IActionResult RegisterB()
        {
            return View(new RegisterBViewModel());
        }


    }
}
