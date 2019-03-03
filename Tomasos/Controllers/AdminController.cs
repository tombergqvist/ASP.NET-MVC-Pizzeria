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

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public ActionResult Recipe(string buttonType)
        {
            switch (buttonType){
                case "Dish":
                    return Dish();
                case "Ingredient":
                    return Ingredient();
            }
            return new EmptyResult();
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public ActionResult Ingredient()
        {
            return PartialView("_IngredientSelectPartial", _context.Produkt.ToList());
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public ActionResult Dish()
        {
            return PartialView("_DishSelectPartial", _context.Matratt.ToList());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public ActionResult LoadIngredient(int id)
        {
            IngredientViewModel model = new IngredientViewModel();
            var ingredient = _context.Produkt.Single(p => p.ProduktId == id);
            model.Name = ingredient.ProduktNamn;
            model.Id = ingredient.ProduktId;
            return PartialView("_IngredientPartial", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public ActionResult LoadDish(int id)
        {
            DishViewModel model = new DishViewModel();
            var dish = _context.Matratt.Single(p => p.MatrattId == id);
            model.Name = dish.MatrattNamn;
            model.Id = dish.MatrattId;
            var dishProducts = _context.MatrattProdukt.Where(p => p.MatrattId == id);
            model.Ingredients = dishProducts.Select(d => d.Produkt).ToList();
            model.Price = dish.Pris;
            model.Type = _context.MatrattTyp.Single(t => t.MatrattTyp1 == dish.MatrattTyp).MatrattTyp1;
            model.Description = dish.Beskrivning;

            model.AllTypes = _context.MatrattTyp.ToList();
            model.AllIngredients = _context.Produkt.ToList();

            return PartialView("_DishPartial", model);
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public ActionResult NewIngredient()
        {
            IngredientViewModel model = new IngredientViewModel();
            model.Name = "New Ingredient";
            return PartialView("_IngredientPartial", model);
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public ActionResult NewDish()
        {
            DishViewModel model = new DishViewModel();
            model.Name = "New Dish";
            model.Description = "Description";
            model.Price = 0;
            model.AllTypes = _context.MatrattTyp.ToList();
            model.AllIngredients = _context.Produkt.ToList();

            return PartialView("_DishPartial", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public ActionResult SaveIngredient(IngredientViewModel model, int? id)
        {
            if (ModelState.IsValid)
            {
                if(id == null)
                {
                    var ingredient = _context.Produkt.SingleOrDefault(p => p.ProduktNamn == model.Name);
                    if (ingredient == null)
                    {
                        ingredient = new Produkt();
                        ingredient.ProduktNamn = model.Name;
                        _context.Produkt.Add(ingredient);
                        _context.SaveChanges();
                        model.Message = "The ingredient has been added.";
                    }
                    else
                    {
                        model.Message = "That ingredient already exists.";
                    }
                }
                else
                {
                    var ingredient = _context.Produkt.First(p => p.ProduktId == model.Id);
                    ingredient.ProduktNamn = model.Name;
                    _context.SaveChanges();
                    model.Message = "The ingredient has been edited.";
                }
            }
            return PartialView("_IngredientPartial", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public ActionResult SaveDish(DishViewModel model, int? id)
        {
            model.Id = id;
            model.AllTypes = _context.MatrattTyp.ToList();
            model.AllIngredients = _context.Produkt.ToList();
            var dishProducts = _context.MatrattProdukt.Where(p => p.MatrattId == id);
            model.Ingredients = dishProducts.Select(d => d.Produkt).ToList();

            if (ModelState.IsValid)
            {
                if (id == null)
                {
                    var dish = _context.Matratt.SingleOrDefault(m => m.MatrattNamn == model.Name);
                    if (dish == null)
                    {
                        dish = new Matratt();
                        AddAttributesToDish(model, dish);
                        _context.Matratt.Add(dish);
                        _context.SaveChanges();
                        model.Message = "The dish has been added.";
                    }
                    else
                    {
                        model.Message = "That dish already exists.";
                    }
                }
                else
                {
                    var dish = _context.Matratt.First(m => m.MatrattId == model.Id);
                    AddAttributesToDish(model, dish);
                    _context.SaveChanges();
                    model.Message = "The dish has been edited.";
                }
            }
            
            return PartialView("_DishPartial", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public ActionResult DeleteIngredient(IngredientViewModel model, int? id)
        {
            if(id != null)
            {
                var ingredient = _context.Produkt.Where(p => p.ProduktId == id);
                _context.RemoveRange(ingredient);
                _context.SaveChanges();
            }
            return RedirectToAction("Ingredient");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public ActionResult DeleteDish(DishViewModel model, int? id)
        {
            if (id != null)
            {
                var dish = _context.Matratt.Where(m => m.MatrattId == id);
                foreach(var dishIngredient in _context.MatrattProdukt.Where(m => m.MatrattId == id))
                {
                    _context.Remove(dishIngredient);
                }
                _context.RemoveRange(dish);
                _context.SaveChanges();
            }
            return RedirectToAction("Dish");
        }

        // Helpers
        private void AddAttributesToDish(DishViewModel model, Matratt dish)
        {
            dish.MatrattNamn = model.Name;
            foreach (var ingredient in model.Ingredients)
            {
                var dishIngredient = _context.MatrattProdukt.
                    SingleOrDefault(p => p.ProduktId == ingredient.ProduktId 
                    && p.MatrattId == model.Id);
                if (dishIngredient == null)
                {
                    dish.MatrattProdukt.Add(new MatrattProdukt {
                        MatrattId = dish.MatrattId,
                        ProduktId = ingredient.ProduktId
                    });
                }
                else
                {
                    dish.MatrattProdukt.Add(dishIngredient);
                }
            }
            dish.MatrattTyp = model.Type;
            dish.Pris = model.Price;
            dish.Beskrivning = model.Description;
        }
    }
}
