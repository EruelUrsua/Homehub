﻿using HomeHub.App.Models;
using HomeHub.DataModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Data;
using System.Net;
using System.Net.Mail;
using System.Reflection.Metadata;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Xml.Linq;
using static System.Net.Mime.MediaTypeNames;

namespace HomeHub.App.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly SignInManager<ApplicationUser> signInManager;
        private readonly EmailSenderService emailSender;
        private readonly HomeHubContext context;
        public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, EmailSenderService emailSender, HomeHubContext context)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.emailSender = emailSender;
            this.context = context;
        }


        public IActionResult Selection()
        {

            return View();

        }
        [HttpGet]
        public async Task<string> GetCurrentUserId()
        {
            ApplicationUser usr = await GetCurrentUserAsync();
            return usr?.Id;
        }

        private Task<ApplicationUser> GetCurrentUserAsync() => userManager.GetUserAsync(HttpContext.User);

        private async Task SendConfirmationEmail(string email, ApplicationUser user)
        {
         

            // Generate the email confirmation token
            var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
            // Build the confirmation callback URL
            var confirmationLink = Url.Action("ConfirmEmail", "Account",
                new { UserId = user.Id, Token = token }, protocol: HttpContext.Request.Scheme);
            // Encode the link to prevent XSS and other injection attacks
            var safeLink = HtmlEncoder.Default.Encode(confirmationLink);
            // Craft a more polished email subject
            var subject = "Welcome to HomeHub! Please Confirm Your Email";
            // Create a professional HTML body
            // Customize inline styles, text, and branding as needed
            var messageBody = $@"
        <div style=""font-family:Arial,Helvetica,sans-serif;font-size:16px;line-height:1.6;color:#333;"">
            <p>Hi {user.Firstname} {user.Lastname},</p>
            <p>Thank you for creating an account at <strong>HomeHub</strong>.
            To start enjoying all of our features, please confirm your email address by clicking the button below:</p>
            <p>
                <a href=""{safeLink}"" 
                   style=""background-color:#007bff;color:#fff;padding:10px 20px;text-decoration:none;
                          font-weight:bold;border-radius:5px;display:inline-block;"">
                    Confirm Email
                </a>
            </p>
            <p>If the button doesn’t work for you, copy and paste the following URL into your browser:
                <br />
                <a href=""{safeLink}"" style=""color:#007bff;text-decoration:none;"">{safeLink}</a>
            </p>
            <p>If you did not sign up for this account, please ignore this email.</p>
            <p>Thanks,<br />
            The HomeHub Team</p>
        </div>
    ";
            //Send the Confirmation Email to the User Email Id
            await emailSender.SendEmailAsync(email, subject, messageBody, true);
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ConfirmEmail(string UserId, string Token)
        {
            if (string.IsNullOrEmpty(UserId) || string.IsNullOrEmpty(Token))
            {
                // Provide a descriptive error message for the view
                ViewBag.ErrorMessage = "The link is invalid or has expired. Please request a new one if needed.";
                return View();
            }
            //Find the User by Id
            var user = await userManager.FindByIdAsync(UserId);
            if (user == null)
            {
                // Provide a descriptive error for a missing user scenario
                ViewBag.ErrorMessage = "We could not find a user associated with the given link.";
                return View();
            }
            // Attempt to confirm the email
            var result = await userManager.ConfirmEmailAsync(user, Token);
            var role = await userManager.GetRolesAsync(user);
            if (result.Succeeded)
            {
                if (role.Contains("Customer"))
                {
                    ViewBag.Message = "Thank you for confirming your email address. Your Email is now confirmed! " +
                    "Kindly wait for your Id to be confirmed for your account to be verified";
                    return View();
                }
                else
                {
                    ViewBag.Message = "Thank you for confirming your email address. Your Email is now confirmed! " +
                       "Kindly wait for your Id and Business Permit to be confirmed for your account to be verified";
                    return View();
                }
            }
            // If confirmation fails
            ViewBag.ErrorMessage = "We were unable to confirm your email address. Please try again or request a new link.";
            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ResendConfirmationEmail(bool IsResend = true)
        {
            if (IsResend)
            {
                ViewBag.Message = "Resend Confirmation Email";
            }
            else
            {
                ViewBag.Message = "Send Confirmation Email";
            }
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResendConfirmationEmail(string Email)
        {
            var user = await userManager.FindByEmailAsync(Email);
            if (user == null || await userManager.IsEmailConfirmedAsync(user))
            {
                // Handle the situation when the user does not exist or Email already confirmed.
                // For security, don't reveal that the user does not exist or Email is already confirmed
                return View("ConfirmationEmailSent");
            }
            //Then send the Confirmation Email to the User
            await SendConfirmationEmail(Email, user);
            return View("ConfirmationEmailSent");
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
            if (model.ValidId == null || model.ValidId.Length == 0)
            {
                ModelState.AddModelError("ValidId", "Please upload a valid ID image.");
                return View(model);
            }

            string uniqueFileName = null;

            if (model.ValidId != null)
            {
                string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/validids");

                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                uniqueFileName = Guid.NewGuid().ToString() + "_" + model.ValidId.FileName;
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await model.ValidId.CopyToAsync(fileStream);
                }
            }

            if (ModelState.IsValid)
            {
                var checkEmail = await userManager.FindByEmailAsync(model.Email);

                if (checkEmail != null)
                {
                    ModelState.AddModelError("Registration Error", "Email has already been used");
                    return View(model);
                }
                else
                {
                    ApplicationUser user = new ApplicationUser
                    {
                        UserName = model.Email,
                        Email = model.Email,
                        Lastname = model.Lastname,
                        Firstname = model.Firstname,
                        PhoneNumber = model.ContactNo,
                        Address = model.Address,
                        lat = model.lat,
                        lng = model.lng,
                        ValidId = $"/images/validids/{uniqueFileName}"  // Corrected path
                    };

                    var result = await userManager.CreateAsync(user, model.Password);
                    await userManager.AddToRoleAsync(user, "Customer");

                    if (result.Succeeded)
                    {
                        await SendConfirmationEmail(model.Email, user);
                        return View("RegistrationSuccessful");
                    }

                    return View(model);
                }
            }

            return View(model);
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
            if (model.BusinessPermitNo == null || model.BusinessPermitNo.Length == 0)
            {
                ModelState.AddModelError("BusinessPermit", "Please upload a business permit image.");
                return View(model);
            }

            // Business Permit Upload
            string businessPermitFileName = null;
            if (model.BusinessPermitNo != null)
            {
                string permitsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/permits");
                Directory.CreateDirectory(permitsFolder); // Creates if doesn't exist

                businessPermitFileName = Guid.NewGuid() + "_" + model.BusinessPermitNo.FileName;
                string permitPath = Path.Combine(permitsFolder, businessPermitFileName);

                using (var stream = new FileStream(permitPath, FileMode.Create))
                {
                    await model.BusinessPermitNo.CopyToAsync(stream);
                }
            }

            // Valid ID Upload
            string validIdFileName = null;
            if (model.ValidId != null)
            {
                string validIdsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/validids");
                Directory.CreateDirectory(validIdsFolder);

                validIdFileName = Guid.NewGuid() + "_" + model.ValidId.FileName;
                string validIdPath = Path.Combine(validIdsFolder, validIdFileName);

                using (var stream = new FileStream(validIdPath, FileMode.Create))
                {
                    await model.ValidId.CopyToAsync(stream);
                }
            }

            // Valid ID Image Upload
            if (model.ValidId == null || model.ValidId.Length == 0)
            {
                ModelState.AddModelError("ValidId", "Please upload a valid ID image.");
                return View(model);
            }

            if (ModelState.IsValid)
            {

                var checkEmail = await userManager.FindByEmailAsync(model.Email);

                if (checkEmail != null)
                {
                    ModelState.AddModelError("Registration Error", "Email has already been used");
                    return View(model);

                }
                else
                {
                    ApplicationUser user = new ApplicationUser();
                    Provider provider = new Provider();

                    user.UserName = model.Email;
                    user.Email = model.Email;
                    user.Lastname = model.Lastname;
                    user.Firstname = model.Firstname;
                    user.PhoneNumber = model.ContactNo;
                    user.Address = model.BusinessAddress;
                    provider.BusinessName = model.BusinessName;
                    provider.Businesstype = model.BusinessType;
                    provider.Category = model.Category;
                    user.lat = model.lat;
                    user.lng = model.lng;
                    user.ValidId = $"/images/validids/{validIdFileName}";
                    provider.BusinessPermit = $"/images/permits/{businessPermitFileName}";
                    var result = await userManager.CreateAsync(user, model.Password);
                    await userManager.AddToRoleAsync(user, "Provider");
                    provider.UserID = user.Id;
                    await context.AddAsync(provider);
                    await context.SaveChangesAsync();
                    if (result.Succeeded)
                    {
                        //Then send the Confirmation Email to the User
                        await SendConfirmationEmail(model.Email, user);

                        return View("RegistrationSuccessful");
                    }
                    return View(model);
                }
            }

            else
            {
                return View(model);
            }
        }


        //Register Admin Account
        [Authorize(Roles = "HeadAdmin")]
        public IActionResult RegisterA()
        {
            return View(new RegisterAVM());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "HeadAdmin")]
        public async Task<IActionResult> RegisterA(RegisterAVM model)
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
                user.lat = model.lat;
                user.lng = model.lng;
                user.ValidId = "N/A";
                user.IsVerified = true;
                var result = await userManager.CreateAsync(user, model.Password);
                await userManager.AddToRoleAsync(user, "Admin");
                if (result.Succeeded)
                {
                    //Then send the Confirmation Email to the User
                    await SendConfirmationEmail(model.Email, user);

                    return RedirectToAction("AdminUsers", "Admin");
                }
                return View(model);
            }
            else
            {
                return View(model);
            }
        }

        //Register Head Admin Account

        public IActionResult RegisterH()
        {
            return View(new RegisterAVM());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegisterH(RegisterAVM model)
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
                user.lat = model.lat;
                user.lng = model.lng;
                user.ValidId = "N/A";
                user.IsVerified = true;
                var result = await userManager.CreateAsync(user, model.Password);
                await userManager.AddToRoleAsync(user, "HeadAdmin");
                if (result.Succeeded)
                {
                    //Then send the Confirmation Email to the User
                    await SendConfirmationEmail(model.Email, user);

                    return RedirectToAction("AdminDashboard", "Admin");
                }
                return View(model);
            }
            else
            {
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> UpdateAcc(string Id)
        {
            var user = await userManager.FindByIdAsync(Id);
            if (user == null) return RedirectToAction("SignIn", "Account");

            var model = new EditUserVM
            {
                // Email = user.Email,
                Id = user.Id,
                Username = user.Email,
                Email = user.Email,
                Lastname = user.Lastname,
                Firstname = user.Firstname,
                ContactNo = user.PhoneNumber,
                Address = user.Address,
                lat = user.lat,
                lng = user.lng

            };
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> UpdateAccA(string Id)
        {
            var user = await userManager.FindByIdAsync(Id);
            if (user == null) return RedirectToAction("SignIn", "Account");       

            var model = new EditUserVM
            {
               // Email = user.Email,
               Id = user.Id,
               Username = user.Email,
               Email = user.Email,
                Lastname = user.Lastname,
                Firstname = user.Firstname,
                ContactNo = user.PhoneNumber,
                Address = user.Address,
                lat = user.lat,
                lng = user.lng
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateAcc(EditUserVM model)
        {
            var user = await userManager.FindByIdAsync(model.Id);
            if (user == null) return RedirectToAction("SignIn", "Account");




            if (!ModelState.IsValid)
            {
                return View(model);
            }

            user.Firstname = model.Firstname;
            user.Lastname = model.Lastname;
            user.Email = model.Email;
            user.UserName = model.Email; 
            user.PhoneNumber = model.ContactNo;
            user.Address = model.Address;
            user.lat = model.lat;
            user.lng = model.lng;

            var result = await userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "Account updated successfully!";
                return RedirectToAction("AdminHome", "Admin");
            }

            // If there's an error, add to ModelState so it can be shown in the view
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            return View(model);
        }




        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateAccA(EditUserVM model)
        {
            var user = await userManager.FindByIdAsync(model.Id);
            if (user == null) return RedirectToAction("SignIn", "Account");

          


            if (!ModelState.IsValid)
            {
                return View(model); 
            }

            user.Firstname = model.Firstname;
            user.Lastname = model.Lastname;
            user.Email = model.Email;
            user.UserName = model.Email; 
            user.PhoneNumber = model.ContactNo;
            user.Address = model.Address;
            user.lat = model.lat;
            user.lng = model.lng;

            var result = await userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "Account updated successfully!";
                return RedirectToAction("AdminUsers", "Admin"); 
            }

            // If there's an error, add to ModelState so it can be shown in the view
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            return View(model);
        }


        [HttpGet]
        public async Task<IActionResult> UpdateCred()
        {
            var user = await userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("SignIn", "Account");

            var provider = await context.Providers.FirstOrDefaultAsync(p => p.UserID == user.Id);

            var model = new RegisterBVM
            {
                ExistingValidId = user.ValidId,
                ExistingBusinessPermit = provider?.BusinessPermit
            };

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> UpdateCredC()
        {
            var user = await userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("SignIn", "Account");

            var provider = await context.Providers.FirstOrDefaultAsync(p => p.UserID == user.Id);

            var model = new RegisterCVM
            {
                ExistingValidId = user.ValidId,
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateCredC(RegisterCVM model)
        {
            var user = await userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("SignIn", "Account");

          //  var provider = await context.ApplicationUsers.FirstOrDefaultAsync(p => p.Id);

            if (user == null)
            {
                ModelState.AddModelError("", "User not found.");
                return View(model);
            }

            // === VALID ID ===
            if (model.ValidId != null && model.ValidId.Length > 0)
            {
                string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/validids");

                // Delete the old valid ID
                if (!string.IsNullOrEmpty(user.ValidId))
                {
                    string oldPath = Path.Combine("wwwroot", user.ValidId.TrimStart('/'));
                    if (System.IO.File.Exists(oldPath)) System.IO.File.Delete(oldPath);
                }

                string validIdFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(model.ValidId.FileName);
                string validIdPath = Path.Combine(uploadsFolder, validIdFileName);
                using (var fileStream = new FileStream(validIdPath, FileMode.Create))
                {
                    await model.ValidId.CopyToAsync(fileStream);
                }
                user.ValidId = "/images/validids/" + validIdFileName;
            }

            user.IsVerified = false; // Optional: reset verification until reviewed again

           // context.Update(provider);
            await userManager.UpdateAsync(user);
            await context.SaveChangesAsync();

            return RedirectToAction("Index", "Customer");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateCred(RegisterBVM model)
        {
            var user = await userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("SignIn", "Account");

            var provider = await context.Providers.FirstOrDefaultAsync(p => p.UserID == user.Id);

            if (user == null || provider == null)
            {
                ModelState.AddModelError("", "User or Provider not found.");
                return View(model);
            }

            // === VALID ID ===
            if (model.ValidId != null && model.ValidId.Length > 0)
            {
                string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/validids");

                // Delete the old valid ID
                if (!string.IsNullOrEmpty(user.ValidId))
                {
                    string oldPath = Path.Combine("wwwroot", user.ValidId.TrimStart('/'));
                    if (System.IO.File.Exists(oldPath)) System.IO.File.Delete(oldPath);
                }

                string validIdFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(model.ValidId.FileName);
                string validIdPath = Path.Combine(uploadsFolder, validIdFileName);
                using (var fileStream = new FileStream(validIdPath, FileMode.Create))
                {
                    await model.ValidId.CopyToAsync(fileStream);
                }
                user.ValidId = "/images/validids/" + validIdFileName;
            }

            // === BUSINESS PERMIT ===
            if (model.BusinessPermitNo != null && model.BusinessPermitNo.Length > 0)
            {
                string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/permits");

                // Delete the old business permit
                if (!string.IsNullOrEmpty(provider.BusinessPermit))
                {
                    string oldPath = Path.Combine("wwwroot", provider.BusinessPermit.TrimStart('/'));
                    if (System.IO.File.Exists(oldPath)) System.IO.File.Delete(oldPath);
                }

                string businessPermitFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(model.BusinessPermitNo.FileName);
                string permitPath = Path.Combine(uploadsFolder, businessPermitFileName);
                using (var fileStream = new FileStream(permitPath, FileMode.Create))
                {
                    await model.BusinessPermitNo.CopyToAsync(fileStream);
                }
                provider.BusinessPermit = "/images/permits/" + businessPermitFileName;
            }

            user.IsVerified = false; // Optional: reset verification until reviewed again

            context.Update(provider);
            await userManager.UpdateAsync(user);
            await context.SaveChangesAsync();

            return RedirectToAction("ProviderHome", "Provider");
        }

        public IActionResult SignIn(string? returnUrl)
        {
           
            SignInVM vm = new SignInVM();
            vm.ReturnUrl = returnUrl;
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]

        public async Task<IActionResult> SignIn(SignInVM model, string? returnUrl)
        {
            if (ModelState.IsValid)
            {
                ApplicationUser user = await userManager.FindByNameAsync(model.Username);
                if (user != null)
                {
                    // Restricted User Popup
                    if (user.IsRestricted)
                    {
                        // Set a flag in TempData or ViewBag
                        TempData["AccountRestricted"] = true;
                        ModelState.AddModelError("Login Error", "Your account has been permanently restricted. Thank you for using HomeHub.");
                        return View(model);
                    }

                    var result = await signInManager.PasswordSignInAsync(user, model.Password, false, false);
                    if (result.Succeeded)
                    {
                        var userlog = await userManager.FindByEmailAsync(model.Username);
                        var role = await userManager.GetRolesAsync(userlog);

                        if (!string.IsNullOrEmpty(returnUrl))
                            return LocalRedirect(returnUrl);
                        else { 
                        if (role.Contains("Customer"))
                        {
                            return RedirectToAction("Index", "Customer");
                        }

                        else if (role.Contains("Provider"))
                        {
                            return RedirectToAction("ProviderHome", "Provider");

                        }

                        else if (role.Contains("Admin"))
                        {
                            return RedirectToAction("AdminDashboard", "Admin");

                        }
                        else if (role.Contains("HeadAdmin"))
                        {
                            return RedirectToAction("AdminDashboard", "Admin");

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
            }
            return View(model);
        
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SignOut()
        {
                
            await signInManager.SignOutAsync();
            return RedirectToAction("SignIn", "Account");
        
        }



    }
}
