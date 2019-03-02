using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tomasos.Models;

namespace Tomasos.ViewModels
{
    public class MenuViewModel
    {
        public Dictionary<string, List<Matratt>> DishByType { get; set; }
        public List<Produkt> Products { get; set; }

        public MenuViewModel()
        {
            DishByType = new Dictionary<string, List<Matratt>>();
            Products = new List<Produkt>();
        }
    }
}
