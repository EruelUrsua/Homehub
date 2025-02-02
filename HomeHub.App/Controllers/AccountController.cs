using HomeHub.App.Models;
using HomeHub.DataModel;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace HomeHub.App.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly SignInManager<ApplicationUser> signInManager;

        public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
        }

        //Register Customer Accounts
        public IActionResult RegisterC()
        {
            return View(new RegisterCVM());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegisterC(RegisterCVM model)
        {
           
            if (ModelState.IsValid)
            {

                ApplicationUser user = new ApplicationUser();
                user.UserName = model.Email;
                user.Email = model.Email;
                user.Lastname = model.Lastname;
                user.Firstname = model.Firstname;
                user.PhoneNumber = model.ContactNo;
                user.Address = model.Address;


                await userManager.CreateAsync(user, model.Password);
                await userManager.AddToRoleAsync(user, "Customer");
                return RedirectToAction("Index", "Home");
            }
            else
            {
                return View(model);
            }
        
        }

        //Register Business Accounts
        public IActionResult RegisterB()
        {
            return View(new RegisterBVM());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegisterB(RegisterBVM model)
        {

            if (ModelState.IsValid)
            {

                ApplicationUser user = new ApplicationUser();
                user.UserName = model.Email;
                user.Email = model.Email;
                user.Lastname = model.Lastname;
                user.Firstname = model.Firstname;
                user.PhoneNumber = model.ContactNo;
                user.Address = model.BusinessAddress;


                await userManager.CreateAsync(user, model.Password);
                await userManager.AddToRoleAsync(user, "Business");
                return RedirectToAction("Index", "Home");
            }
            else
            {
                return View(model);
            }

        }
        public IActionResult SignIn()
        {
            SignInVM vm = new SignInVM();

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]

        public async Task<IActionResult> SignIn(SignInVM model)
        {

            ApplicationUser user = await userManager.FindByNameAsync(model.Username);
            if (user != null)
            {
                var result = await signInManager.PasswordSignInAsync(user, model.Password, false, false);
                if (result.Succeeded)
                {
                    return RedirectToAction("Index", "Home");
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
