using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Tomasos.Models;

namespace Tomasos.ViewModels.Admin
{
    public class DishViewModel
    {
        [Required]
        [StringLength(20, ErrorMessage = "The {0} must be between {2} and {1} characters long.", MinimumLength = 1)]
        [Display(Name = "Dish name")]
        public string Name { get; set; }

        public int? Id { get; set; }
        public List<Produkt> Ingredients { get; set; }

        [Required(ErrorMessage = "Total price can not be empty.")]
        [RegularExpression("([0-9]+)", ErrorMessage = "Please enter a valid price.")]
        public int Price { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be between {2} and {1} characters long.", MinimumLength = 1)]
        public string Description { get; set; }

        public int Type { get; set; }

        public List<MatrattTyp> AllTypes { get; set; }
        public List<Produkt> AllIngredients { get; set; }

        public string Message { get; set; }
        
        public DishViewModel()
        {
            Ingredients = new List<Produkt>();
        }
    }
}
