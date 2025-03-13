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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUser(string Id)
        {
            //First Fetch the User you want to Delete
            var user = await userManager.FindByIdAsync(Id);
            if (user == null)
            {
                // Handle the case where the user wasn't found
                ViewBag.ErrorMessage = $"User with Id = {Id} cannot be found";
                return View("NotFound");
            }
            else
            {
                //Delete the User Using DeleteAsync Method of UserManager Service
                var result = await userManager.DeleteAsync(user);
                if (result.Succeeded)
                {
                    // Handle a successful delete
                    return RedirectToAction("AdminHome");
                }
                else
                {
                    // Handle failure
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                }
                return View("AdminHome");
            }
        }


        //public IActionResult AdminHome()
        //{

        //    var users = userManager.Users.ToList().Except();



        //    return View(users);
        //}

    }
}
