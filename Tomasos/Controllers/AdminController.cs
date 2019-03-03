using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tomasos.IdentityModels;
using Tomasos.Models;
using Tomasos.ViewModels.Admin;

namespace Tomasos.Controllers
{
    public class AdminController : Controller
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private TomasosContext _context;

        public AdminController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, TomasosContext context)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _context = context;
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public ActionResult Admin()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public ActionResult SearchUser(string searchValue)
        {
            SelectUserViewModel model = new SelectUserViewModel();
            model.Users = _context.Users.Where(u => u.UserName.Contains(searchValue ?? "")).ToList();
            return PartialView("_SelectUserPartial", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> ShowUserInfo(string id)
        {
            ShowUserInfoViewModel model = new ShowUserInfoViewModel();
            model.User = await _userManager.FindByIdAsync(id);
            model.IsAdmin = await _userManager.IsInRoleAsync(model.User, "Admin");
            model.Orders = _context.Bestallning.Where(b => b.KundId == model.User.Id).OrderByDescending(b => b.BestallningDatum).ToList();

            var roles = _context.Roles.Where(r => r.Name != "Admin").Select(r => r.Name).ToList();
            model.AvailableRoles = new List<string>();
            foreach (var role in roles)
            {
                if (await _userManager.IsInRoleAsync(model.User, role))
                {
                    model.AvailableRoles.Add(role);
                    roles.Remove(role);
                    break;
                }
            }
            model.AvailableRoles.AddRange(roles);
            return PartialView("_ShowUserInfoPartial", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> ChangeUserInfo(string id, string role, string oldRole)
        {
            ApplicationUser user = await _userManager.FindByIdAsync(id);
            await _userManager.AddToRoleAsync(user, role);
            await _userManager.RemoveFromRoleAsync(user, oldRole);
            string msg = $"This user has changed role to {role}.";
            return PartialView("_MessagePartial", msg);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public ActionResult ShowOrder(int id)
        {
            OrderViewModel model = new OrderViewModel();
            var order = _context.Bestallning.Single(o => o.BestallningId == id);
            model.Delivered = order.Levererad;
            model.Id = order.BestallningId;
            model.TotalPrice = order.Totalbelopp;
            return PartialView("_OrderPartial", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public ActionResult SaveOrder(OrderViewModel model, int id )
        {
            if (ModelState.IsValid)
            {
                var order = _context.Bestallning.Single(o => o.BestallningId == id);
                order.Totalbelopp = model.TotalPrice ?? 0;
                order.Levererad = model.Delivered;
                _context.SaveChanges();
                model.Message = "The order has been saved.";
            }
            return PartialView("_OrderPartial", model);
        }

        
    }
}
