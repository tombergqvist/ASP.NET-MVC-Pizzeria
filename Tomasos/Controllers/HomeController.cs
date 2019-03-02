using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tomasos.IdentityModels;
using Tomasos.Models;
using Tomasos.ViewModels;

namespace Tomasos.Controllers
{
    public class HomeController : Controller
    {
        private TomasosContext _context;
        private UserManager<ApplicationUser> _userManager;

        public HomeController(TomasosContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpGet]
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public ActionResult Menu()
        {
            MenuViewModel model = new MenuViewModel();

            foreach (var type in _context.MatrattTyp)
            {
                var dishes = new List<Matratt>();
                foreach (var dish in _context.Matratt.Where(m => m.MatrattTypNavigation == type)
                    .Include(p => p.MatrattProdukt).ThenInclude(p => p.Produkt))
                {
                    dishes.Add(dish);
                }
                if(dishes.Count > 0)
                    model.DishByType.Add(type.Beskrivning, dishes);
            }
            return View(model);
        }

        [HttpGet]
        [Authorize(Roles = "Regular, Premium, Admin")]
        public async Task<ActionResult> Cart()
        {
            CartViewModel cart = new CartViewModel();
            string order = HttpContext.Session.GetString("Order");

            var user = await GetCurrentUserAsync();
            cart.Points = user.Points ?? 0;
            if (order != null)
            {
                foreach(var dish in JsonConvert.DeserializeObject<List<Matratt>>(order))
                {
                    var key = cart.Dishes.Keys.Where(d => d.MatrattId == dish.MatrattId);
                    if (key.Count() == 0)
                    {
                        cart.Dishes.Add(dish, 1);
                    }
                    else
                    {
                        var value = cart.Dishes[key.First()];
                        cart.Dishes[key.First()] = ++value;
                    }
                    
                }
                cart.TotalPrice = cart.Dishes.Sum(d => d.Value * d.Key.Pris);
                cart.Points += cart.Dishes.Sum(d => d.Value) * 10;
                // Display altered prices 
                // Free pizza at 100 points (the most expensive)
                if (User.IsInRole("Premium") && cart.Points >= 100)
                    cart.TotalPrice -= cart.Dishes.Max(d => d.Key.Pris);
                // 20% of order when dishes >= 3
                if (User.IsInRole("Premium") && cart.Dishes.Sum(d => d.Value) >= 3)
                    cart.TotalPrice = (int)(cart.TotalPrice * 0.8);
                
            }
            return View(cart);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Regular, Premium, Admin")]
        public ActionResult AddToCart(int id)
        {
            var dish = _context.Matratt.First(m => m.MatrattId == id);

            string dishes = HttpContext.Session.GetString("Order");
            var order = new List<Matratt>();
            if (dishes != null)
            {
                order = JsonConvert.DeserializeObject<List<Matratt>>(dishes);
            }
            order.Add(dish);
            HttpContext.Session.SetString("Order", JsonConvert.SerializeObject(order));
            return RedirectToAction("Menu");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Regular, Premium, Admin")]
        public async Task<ActionResult> PlaceOrder()
        {
            string sessionString = HttpContext.Session.GetString("Order");
            if (sessionString != null)
            {
                BestallningMatratt orderDish = new BestallningMatratt();
                var dishes = JsonConvert.DeserializeObject<List<Matratt>>(sessionString);
                var order = await NewOrderAsync(dishes);

                foreach (var dish in dishes)
                {
                    if(order.BestallningMatratt.Where(o => o.MatrattId == dish.MatrattId).Count() == 0)
                    {
                        orderDish = new BestallningMatratt { MatrattId = dish.MatrattId, Antal = 1};
                        order.BestallningMatratt.Add(orderDish);
                    }
                    else
                    {
                        order.BestallningMatratt.Single(o => o.MatrattId == dish.MatrattId).Antal++;
                    }
                }
                // Change to altered prices, depending on bonus points and how many dishes
                var user = await GetCurrentUserAsync();
                if (User.IsInRole("Premium"))
                {
                    if(user.Points == null)
                        user.Points = 0;
                    user.Points += 10 * dishes.Count;
                    if (user.Points >= 100)
                    {
                        order.Totalbelopp -= dishes.Max(d => d.Pris);
                        user.Points -= 100;
                    }
                    if (dishes.Count >= 3)
                        order.Totalbelopp = (int)(order.Totalbelopp * 0.8);
                }

                _context.Add(order);
                await _context.SaveChangesAsync();
                HttpContext.Session.Remove("Order");
                return View("ThankYou");
            }
            else
            {
                return RedirectToAction("Cart");
            }
        }

        //  Helpers
        private Task<ApplicationUser> GetCurrentUserAsync()
        {
            return _userManager.GetUserAsync(User);
        }

        private async Task<Bestallning> NewOrderAsync(List<Matratt> dishes)
        {
            return new Bestallning
            {
                BestallningDatum = DateTime.Now,
                Levererad = false,
                Kund = await GetCurrentUserAsync(),
                Totalbelopp = dishes.Sum(d => d.Pris)
            };
        }
    }
}
