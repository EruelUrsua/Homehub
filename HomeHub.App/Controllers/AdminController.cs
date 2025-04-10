using HomeHub.App.Models;
using HomeHub.DataModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Data;
using System.Linq;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace HomeHub.App.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly HomeHubContext context;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly EmailSenderService emailSender;

        public AdminController( HomeHubContext context, UserManager<ApplicationUser> userManager, EmailSenderService emailSender)
        {
            this.context = context;
            this.userManager = userManager;
            this.emailSender = emailSender;
        }

        public async Task <IActionResult> AdminDashboard()
        {

            var user = await GetCurrentUserId();

            if (user == null) return Unauthorized();
            //var userlog = await userManager.FindByIdAsync(uid);
            //var email = await userManager.GetEmailAsync(userlog);
            var provider = await userManager.GetUsersInRoleAsync("Provider");
            var Customer = await userManager.GetUsersInRoleAsync("Customer");
            var accounts = provider.Concat(Customer);

            return View(accounts);

            //var users = await context.ApplicationUsers
            //    .FirstOrDefaultAsync(p => p.Id != user);

            //if (users == null) return Forbid();
            //var sales = context.ClientOrders.Where(c => c.BusinessId == user).ToList();

 


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
            //var userlog = await userManager.FindByIdAsync(uid);
            //var email = await userManager.GetEmailAsync(userlog);
            var provider = await userManager.GetUsersInRoleAsync("Provider");
            var Customer = await userManager.GetUsersInRoleAsync("Customer");

            var accounts = provider.Concat(Customer);

            return View(accounts);
        }

        public async Task<IActionResult> AdminUsers()
        {
            var uid = await GetCurrentUserId();
            ApplicationUser user = new ApplicationUser();
            var admin= await userManager.GetUsersInRoleAsync("Admin");

            //var accounts = provider.Concat(Customer);

            return View(admin);
        }

        public async Task<ActionResult> UserProfile()
        {
            ApplicationUser user = await GetCurrentUserAsync();
            if (user == null)
                return View("Index");

            var model = new UserProfileVM
            {
                FirstName = user.Firstname,
                LastName = user.Lastname,
                //Address = user.Address,
                lat = user.lat,
                lng = user.lng

            };

            return View(model);
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
        public async Task<ActionResult> UserProfileAdmin(string Id)
        {
            ApplicationUser user = await userManager.FindByIdAsync(Id);
            var role = await userManager.GetRolesAsync(user);
            var ur = "";
            if (user == null)
                return View("Index");

            if (role.Contains("Customer"))
            {
                ur = "Customer";
            }
            else if (role.Contains("Provider"))
            {
                ur = "Provider";
            }

            var model = new UserProfileVM
            {
                FirstName = user.Firstname,
                LastName = user.Lastname,
                //Address = user.Address,
                lat = user.lat,
                lng = user.lng,
                Role = ur, 
            };

            return View(model);
        }


        public async Task<IActionResult> UsersForVerification()
        {
            var usersUnderReview = userManager.Users
                .Where(u => u.IsVerified == false) // Get all users who needs verification
                .ToList();

            return View(usersUnderReview);
        }

        public async Task<IActionResult> CheckUserCred(string Id)
        {
            ApplicationUser user = await userManager.FindByIdAsync(Id);
            if (user == null)
                return View("Index");
            var role = await userManager.GetRolesAsync(user);
            var ur = "";
            string businessPermitPath = null;
            if (user == null)
                return View("Index");

            if (role.Contains("Customer"))
            {
                ur = "Customer";
            }
            else if (role.Contains("Provider"))
            {
                var business = await context.Providers
                    .FirstOrDefaultAsync(b => b.UserID == user.Id);

                businessPermitPath = business?.BusinessPermit;
                ur = "Provider";
            }
            var model = new UserProfileVM
            {
                FirstName = user.Firstname,
                LastName = user.Lastname,
                //Address = user.Address,
                lat = user.lat,
                lng = user.lng,
                Role = ur,
                ValidId = user.ValidId,
                BusinessPermit = businessPermitPath,
                Id = user.Id
            };

            return View(model);
        }

        public async Task<IActionResult> VerifyUser(string Id)
        {
            var user = await userManager.FindByIdAsync(Id);
            if (user != null)
            {
                user.IsVerified = true;
                await userManager.UpdateAsync(user);
            }

            return RedirectToAction("UsersForVerification");
        }


        public async Task<IActionResult> UsersUnderReview()
        {
            var usersUnderReview = userManager.Users
                .Where(u => u.IsUnderReview)
                .ToList();

            return View(usersUnderReview);
        }

        public async Task<IActionResult> UserRatings(string userId)
        {
            var ratings = context.Ratings
                .Where(r => r.CustomerId == userId && r.ReviewerId != userId)
                .ToList();

            var user = await userManager.FindByIdAsync(userId);
            var averageRating = ratings.Any() ? ratings.Average(r => r.Score) : 0;

            var model = new UserRatingViewModel
            {
                User = user,
                Ratings = ratings,
                AverageRating = averageRating
            };

            return View(model);
        }

        public async Task<IActionResult> LiftRestriction(string userId)
        {
            var user = await userManager.FindByIdAsync(userId);
            if (user != null)
            {
                user.IsUnderReview = false;
                user.LastReviewDate = DateTime.Now;
                await userManager.UpdateAsync(user);
            }

            return RedirectToAction("UsersUnderReview");
        }

        public async Task<IActionResult> PermanentlyRestrict(string userId)
        {
            var user = await userManager.FindByIdAsync(userId);
            if (user != null)
            {
                user.IsUnderReview = false;
                user.IsRestricted = true;
                await userManager.UpdateAsync(user);
            }

            return RedirectToAction("UsersUnderReview");
        }
    }
}
