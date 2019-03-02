using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tomasos.Models;

namespace Tomasos.ViewModels
{
    public class CartViewModel
    {
        public Dictionary<Matratt, int> Dishes { get; set; }
        public int TotalPrice { get; set; }
        public int Points { get; set; }

        public CartViewModel()
        {
            Dishes = new Dictionary<Matratt, int>();
        }
    }
}
