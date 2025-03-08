using HomeHub.DataModel;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace HomeHub.App.Controllers
{
    public class AdminController : Controller
    {
        private readonly HomeHubContext context;

        public AdminController( HomeHubContext context)
        {
            this.context = context;
        }

        public async Task<IActionResult> AdminHome()
        {
            return View("AdminHome");
        }
        public IActionResult Index()
        {
            return View();
        }
    }
}
