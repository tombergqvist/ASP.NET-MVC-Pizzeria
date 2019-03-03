using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Tomasos.ViewModels.Admin
{
    public class OrderViewModel
    {
        [Display(Name = "Order Id")]
        public int Id { get; set; }
        public bool Delivered { get; set; }
        [Required(ErrorMessage = "Total price can not be empty.")]
        [RegularExpression("([0-9]+)", ErrorMessage = "Please enter a valid price")]
        public int? TotalPrice { get; set; }
        public string Message { get; set; }
    }
}
