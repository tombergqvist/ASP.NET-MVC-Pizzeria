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
    public class RecipeController : Controller
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private TomasosContext _context;

        public RecipeController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, TomasosContext context)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _context = context;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public ActionResult Recipe(string buttonType)
        {
            switch (buttonType)
            {
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
                if (id == null)
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
            if (id != null)
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
                foreach (var dishIngredient in _context.MatrattProdukt.Where(m => m.MatrattId == id))
                {
                    _context.Remove(dishIngredient);
                }
                _context.RemoveRange(dish);
                _context.SaveChanges();
            }
            return RedirectToAction("Dish");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public ActionResult AddOrRemoveIngredient(int ingredientId, int dishId, string buttonType)
        {
            switch (buttonType)
            {
                case "Add":
                    AddIngredient(ingredientId, dishId);
                    break;
                case "Remove":
                    RemoveIngredient(ingredientId, dishId);
                    break;
            }
            return LoadDish(dishId);
        }

        // Helpers
        private void AddIngredient(int ingredientId, int dishId)
        {
            var dish = _context.MatrattProdukt.SingleOrDefault(m => m.MatrattId == dishId && m.ProduktId == ingredientId);
            if (dish == null)
            {
                _context.Add(new MatrattProdukt { ProduktId = ingredientId, MatrattId = dishId });
                _context.SaveChanges();
            }
        }

        private void RemoveIngredient(int? ingredientId, int? dishId)
        {
            if (dishId != null)
            {
                foreach (var dishIngredient in _context.MatrattProdukt
                    .Where(m => m.MatrattId == dishId && m.ProduktId == ingredientId))
                {
                    _context.Remove(dishIngredient);
                }
                _context.SaveChanges();
            }
        }

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
                    dish.MatrattProdukt.Add(new MatrattProdukt
                    {
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
