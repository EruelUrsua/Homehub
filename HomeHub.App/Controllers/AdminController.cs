using HomeHub.App.Models;
using HomeHub.DataModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Newtonsoft.Json.Linq;
using System.Data;
using System.Linq;
using System.Text.Encodings.Web;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace HomeHub.App.Controllers
{
    [Authorize(Roles = "Admin, HeadAdmin")]
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
            ADashboardVM vm = new ADashboardVM();
            // OrdersLog ordersLog = new OrdersLog();
            //var sales = context.OrdersLogs.Select(d => d.Status == "Delivered").ToList();
           // var od = context.OrdersLogs.Where(d => d.Status == "Delivered").ToList();
            var user = await GetCurrentUserId();
            if (user == null) return Unauthorized();
            var provider = await userManager.GetUsersInRoleAsync("Provider");
            var customer = await userManager.GetUsersInRoleAsync("Customer");
            var uv = userManager.Users
               .Where(u => u.IsVerified == false) // Get all users who needs verification
               .ToList();

            var ur = userManager.Users
              .Where(u => u.IsUnderReview == true) // Get all users who needs verification
              .ToList();
            var rr = userManager.Users
             .Where(u => u.IsRestricted== true) // Get all users who needs verification
             .ToList();

            vm.customers = customer.Count();
            vm.providers = provider.Count();
            vm.userRestricted = rr.Count();
            vm.userVerified = uv.Count();
            vm.userReview = ur.Count();
            //vm.orderLogs = od.Count();
            vm.totalUser = userManager.Users.Count(); ;


            


            return View(vm);

          
 


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

        public async Task<ActionResult> UserProfile()
        {
            ApplicationUser user = await GetCurrentUserAsync();
            var role = await userManager.GetRolesAsync(user);
            var ur = "";
            if (user == null)
                return View("Index");
            else if (role.Contains("Admin"))
            {
                ur = "Admin";
            }
            else if (role.Contains("HeadAdmin"))
            {
                ur = "Head Admin";
            }

            var model = new UserProfileVM
            {
                FirstName = user.Firstname,
                LastName = user.Lastname,
                //Address = user.Address,
                Role = ur,
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUserA(string Id)
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
                    return RedirectToAction("AdminUsers");
                }
                else
                {
                    // Handle failure
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                }
                return View("AdminUsers");
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
            else if (role.Contains("Admin"))
            {
                ur = "Admin";
            }
            else if (role.Contains("HeadAdmin"))
            {
                ur = "HeadAdmin";
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
            Provider provider = new Provider();
            var user = await userManager.FindByIdAsync(Id);
            var details = await context.Providers.FirstOrDefaultAsync(p => p.UserID == user.Id);
            var role = await userManager.GetRolesAsync(user);
            if (user != null && role.Contains("Provider") )
            {
                user.IsVerified = true;
                await userManager.UpdateAsync(user);
                await SendNotificationEmail(user.Email, details.BusinessName);

            }
            else if (user != null && role.Contains("Customer"))
            {
                user.IsVerified = true;
                await userManager.UpdateAsync(user);
                await SendNotificationEmailC(user.Email, user.Firstname, user.Lastname);

            }

            return RedirectToAction("UsersForVerification");
        }

        public async Task<IActionResult> DeclineVerification(string Id)
        {
         
            var user = await userManager.FindByIdAsync(Id);
            var role = await userManager.GetRolesAsync(user);

            if (user != null && role.Contains("Provider"))
            {
                await DSendNotificationEmail(user.Email, user.Firstname, user.Lastname, user.Id);

            }
            else if (user != null && role.Contains("Customer"))
            {
                await DSendNotificationEmailC(user.Email, user.Firstname, user.Lastname, user.Id);
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

        private async Task DSendNotificationEmail(string email, string firstname, string lastname, string Id)
        {

            var subject = "Account Verification Status";
            //Create a professional HTML body
            var confirmationLink = Url.Action("UpdateCred", "Account",
              new { UserId = Id}, protocol: HttpContext.Request.Scheme);
            // Encode the link to prevent XSS and other injection attacks
            var safeLink = HtmlEncoder.Default.Encode(confirmationLink);
            var messageBody = $@"
        <div style=""font-family:Arial,Helvetica,sans-serif;font-size:16px;line-height:1.6;color:#333;"">
           <p>Hi {firstname} {lastname},</p>
            <p>Unfortunately your account verification is unsuccesful</p>
            <p>Please click the button below and send us your credentials again<p>
 <p>
                <a href=""{safeLink}"" 
                   style=""background-color:#007bff;color:#fff;padding:10px 20px;text-decoration:none;
                          font-weight:bold;border-radius:5px;display:inline-block;"">
                    Reupload Credentials
                </a>
            </p>
            <p>Thanks,<br />
            The HomeHub Team</p>
        </div>
    ";
            //Send the Confirmation Email to the User Email Id
            await emailSender.SendEmailAsync(email, subject, messageBody, true);
        }

        private async Task DSendNotificationEmailC(string email, string firstname, string lastname, string Id)
        {

            var subject = "Account Verification Status";
            //Create a professional HTML body
            var confirmationLink = Url.Action("UpdateCredC", "Account",
              new { UserId = Id }, protocol: HttpContext.Request.Scheme);
            // Encode the link to prevent XSS and other injection attacks
            var safeLink = HtmlEncoder.Default.Encode(confirmationLink);
            var messageBody = $@"
        <div style=""font-family:Arial,Helvetica,sans-serif;font-size:16px;line-height:1.6;color:#333;"">
           <p>Hi {firstname} {lastname},</p>
            <p>Unfortunately your account verification is unsuccesful</p>
            <p>Please click the button below and send us your credentials again<p>
 <p>
                <a href=""{safeLink}"" 
                   style=""background-color:#007bff;color:#fff;padding:10px 20px;text-decoration:none;
                          font-weight:bold;border-radius:5px;display:inline-block;"">
                    Reupload Credentials
                </a>
            </p>
            <p>Thanks,<br />
            The HomeHub Team</p>
        </div>
    ";
            //Send the Confirmation Email to the User Email Id
            await emailSender.SendEmailAsync(email, subject, messageBody, true);
        }


        private async Task SendNotificationEmail(string email, string providername)
        {
           
            var subject = "Account Verification Status";
            //Create a professional HTML body
            //Customize inline styles, text, and branding as needed
            var messageBody = $@"
        <div style=""font-family:Arial,Helvetica,sans-serif;font-size:16px;line-height:1.6;color:#333;"">
            <p>Hi {providername},</p>
            <p>Congratulations is now verified</p>
            <p>Thanks,<br />
            The HomeHub Team</p>
        </div>
    ";
            //Send the Confirmation Email to the User Email Id
            await emailSender.SendEmailAsync(email, subject, messageBody, true);
        }

        private async Task SendNotificationEmailC(string email, string firstname, string lastname)
        {

            var subject = "Account Verification Status";
            //Create a professional HTML body
            //Customize inline styles, text, and branding as needed
            var messageBody = $@"
        <div style=""font-family:Arial,Helvetica,sans-serif;font-size:16px;line-height:1.6;color:#333;"">
            <p>Hi {firstname} {lastname},</p>
            <p>Congratulations is now verified</p>
            <p>Thanks,<br />
            The HomeHub Team</p>
        </div>
    ";
            //Send the Confirmation Email to the User Email Id
            await emailSender.SendEmailAsync(email, subject, messageBody, true);
        }


        [Authorize(Roles = "HeadAdmin")]
        public async Task<IActionResult> AdminUsers()
        {
            var uid = await GetCurrentUserId();
            ApplicationUser user = new ApplicationUser();
            var admin = await userManager.GetUsersInRoleAsync("Admin");

            //var accounts = provider.Concat(Customer);

            return View(admin);
        }
    }
}
