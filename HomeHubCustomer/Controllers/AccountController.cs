using HomeHub.App.Models;
using HomeHub.DataModel;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
        public async Task<IActionResult> SignIn(SignInViewModel model, string? returnUrl)
        {
            ApplicationUser user = await userManager.FindByNameAsync(model.Email);
            if (user != null)
            {
                var result = await signInManager.PasswordSignInAsync(user, model.Password, false, false);

                if (result.Succeeded)
                {
                    if (!string.IsNullOrEmpty(returnUrl))
                        return LocalRedirect(returnUrl);
                    else
                    {
                        //return LocalRedirect("/Home/Index");
                        if (user.Usertype == "Customer")
                        {
                            return LocalRedirect("/Customer/Add");
                        }
                        else if (user.Usertype == "Business")
                        {
                            return LocalRedirect("/Provider/Index");
                        }
                    }
                }
                else
                {
                    ModelState.AddModelError("Login Error", "Invalid Credentials");
                    return View(model);
                }
            }
            else
            {
                ModelState.AddModelError("Login Error", "Invalid Credentials");
                return View(model);
            }

            return RedirectToAction("Index", "Home");
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
