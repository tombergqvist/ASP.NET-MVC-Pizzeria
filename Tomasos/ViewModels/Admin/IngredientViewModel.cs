using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Tomasos.Models;

namespace Tomasos.ViewModels.Admin
{
    public class IngredientViewModel
    {
        [Required]
        [StringLength(20, ErrorMessage = "The {0} must be between {2} and {1} characters long.", MinimumLength = 1)]
        [Display(Name = "Ingredient name")]
        public string Name { get; set; }

        public int? Id { get; set; }
        public string Message { get; set; }
    }
}
