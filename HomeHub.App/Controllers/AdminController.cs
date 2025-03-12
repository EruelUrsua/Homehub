using HomeHub.DataModel;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Linq;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace HomeHub.App.Controllers
{
    public class AdminController : Controller
    {
        private readonly HomeHubContext context;
        private readonly UserManager<ApplicationUser> userManager;


        public AdminController( HomeHubContext context, UserManager<ApplicationUser> userManager)
        {
            this.context = context;
            this.userManager = userManager;
        }

        [HttpGet]
        public async Task<string> GetCurrentUserId()
        {
            ApplicationUser usr = await GetCurrentUserAsync();
            return usr?.Id;
        }

        private Task<ApplicationUser> GetCurrentUserAsync() => userManager.GetUserAsync(HttpContext.User);
        public async Task<IActionResult> AdminHome()
        {
            var uid = await GetCurrentUserId(); 
            ApplicationUser user = new ApplicationUser();
            var userlog = await userManager.FindByIdAsync(uid);
            var email = await userManager.GetEmailAsync(userlog);

            var accounts = userManager.Users.Where(u => u.Email != email).ToList();


            return View(accounts);
        }


        //public IActionResult AdminHome()
        //{

        //    var users = userManager.Users.ToList().Except();



        //    return View(users);
        //}

    }
}
