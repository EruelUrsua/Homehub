using HomeHub.App.Models;
using HomeHub.DataModel;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Data;

namespace HomeHub.App.Controllers
{
    public class AccountController : Controller
    {
        private readonly HomeHubContext context;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly SignInManager<ApplicationUser> signInManager;
       
        public AccountController(UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager, HomeHubContext context)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.context = context;
        }


        public IActionResult Selection()
        {

            return View();
          
        }

    

        public IActionResult Register()
        {
            return View(new RegisterViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {

            if (ModelState.IsValid)
            {
                Customer entity = new Customer();
                entity.UserId = model.UserID;
                entity.Email = model.Email;
                entity.Password = model.Password;
                entity.Firstname = model.Firstname;
                entity.Lastname = model.Lastname;
                entity.ContactNo = model.ContactNo;
                entity.Address = model.Address;
                entity.ValidIDno = model.ValidIDno;

                await context.AddAsync(entity);
                await context.SaveChangesAsync();

                return RedirectToAction("SignIn", "Account");
            }
            else
            {
                return View(model);
            }
        }

        public IActionResult RegisterB()
        {
            return View(new RegisterBViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegisterB(RegisterBViewModel model)
        {
            if (ModelState.IsValid)
            {

                //    ApplicationUser user = new ApplicationUser();
                Business entity = new Business();
                entity.UserID = model.UserID;
                entity.Email = model.Email;
                entity.Password = model.Password;
                entity.BusinessName = model.BusinessName;
                entity.RepresentativeName = model.RepresentativeName;
                entity.ContactNo = model.ContactNo;
                entity.CompanyAddress = model.CompanyAddress;
                entity.OfferList = model.OfferList;
                entity.Businesstype = model.Businesstype;
                entity.BusinessPermitNo = model.BusinessPermitNo;

                await context.AddAsync(entity);
                await context.SaveChangesAsync();

                return RedirectToAction("SignIn", "Account");
            }

            else
            {
                return View(model);
            }
        }





        public IActionResult SignIn(string? returnUrl)
        {
            SignInViewModel vm = new SignInViewModel();
            if (!string.IsNullOrEmpty(returnUrl))
                vm.ReturnUrl = returnUrl;

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SignIn(SignInViewModel model)
        {
            // var user = await userManager.FindByNameAsync(model.Email);
            //if (ModelState.IsValid)
            //{

            var email = model.Email;
            var password = model.Password;

            var usersC = await context.Customers.Where(usr => usr.Email == email && usr.Password == password).Select(usr => new{
            
                usr.UserId,
                usr.Email,
                usr.Password,
    
            }).ToListAsync();

            var usersB = await context.Customers.Where(usr => usr.Email == email && usr.Password == password).Select(usr => new {

                usr.UserId,
                usr.Email,
                usr.Password,

            }).ToListAsync();


            if (usersC.Count > 0)
            {

                return RedirectToAction("Index", "Customer");
            }

            else if (usersB.Count > 0)
            {

                return RedirectToAction("ProviderHome", "Provider");
            }

            else
            {
                return View(model);
            }

         
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SignOut()
        {
            await signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

    }
}
