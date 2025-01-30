using HomeHub.App.Models;
using Microsoft.AspNetCore.Mvc;

namespace HomeHub.App.Controllers
{
    public class AccountController : Controller
    {
        public IActionResult RegisterC()
        {
            return View(new RegisterCVM());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RegisterC(RegisterCVM model)
        {
                return RedirectToAction("Index", "Home");
        }
    }
}
