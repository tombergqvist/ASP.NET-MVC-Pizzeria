using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tomasos.IdentityModels;
using Tomasos.Models;
using Tomasos.ViewModels;

namespace Tomasos.Controllers
{
    public class AccountController : Controller
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private TomasosContext _context;

        public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, TomasosContext context)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _context = context;
        }

        [HttpGet]
        public ActionResult Register()
        {
            RegisterViewModel model = new RegisterViewModel();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser { UserName = model.Username, Email = model.Email, City = model.City,
                    Street = model.Street, Postcode = model.Postcode, PhoneNumber = model.Phone };

                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    result = await _userManager.AddToRoleAsync(user, "REGULAR");
                    if (result.Succeeded)
                    {
                        await _signInManager.SignInAsync(user, isPersistent: false);
                        return RedirectToAction("Registered");
                    }
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }
            return View(model);
        }

        [HttpGet]
        public ActionResult Registered()
        {
            return View();
        }


        [HttpGet]
        public ActionResult Login()
        {
            LoginViewModel model = new LoginViewModel();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(model.Username, model.Password, model.IsPersistent, lockoutOnFailure: false);
                if (result.Succeeded)
                {
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ModelState.AddModelError("", "Invalid login.");
                }
            }
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        [Authorize(Roles = "Regular, Premium, Admin")]
        public async Task<ActionResult> UserInfo()
        {
            UserInfoViewModel model = new UserInfoViewModel();
            var currentUser = await GetCurrentUserAsync();

            model.City = currentUser.City;
            model.Email = currentUser.Email;
            model.Phone = currentUser.PhoneNumber;
            model.Postcode = currentUser.Postcode;
            model.Street = currentUser.Street;

            return View(model);
        }
        [HttpPost]
        [Authorize(Roles = "Regular, Premium, Admin")]
        public async Task<ActionResult> UserInfo(UserInfoViewModel model)
        {
            if (ModelState.IsValid)
            {
                var currentUser = await GetCurrentUserAsync();
                if (await _userManager.CheckPasswordAsync(currentUser, model.CurrentPassword))
                {
                    currentUser.City = model.City;
                    currentUser.Email = model.Email;
                    currentUser.PhoneNumber = model.Phone;
                    currentUser.Postcode = model.Postcode;
                    currentUser.Street = model.Street;

                    if(model.Password != null)
                        await _userManager.ChangePasswordAsync(currentUser, model.CurrentPassword, model.Password);

                    await _userManager.UpdateAsync(currentUser);
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Invalid password.");
                }
            }
            return View(model);
        }

        public ActionResult AccessDenied()
        {
            return View();
        }

        //  Helpers
        private Task<ApplicationUser> GetCurrentUserAsync()
        {
            return _userManager.GetUserAsync(User);
        }
    }
}
