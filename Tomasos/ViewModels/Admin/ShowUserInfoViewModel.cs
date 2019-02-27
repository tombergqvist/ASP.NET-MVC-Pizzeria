using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tomasos.IdentityModels;
using Tomasos.Models;

namespace Tomasos.ViewModels.Admin
{
    public class ShowUserInfoViewModel
    {
        public ApplicationUser User { get; set; }
        public List<string> AvailableRoles { get; set; }
        public bool IsAdmin { get; set; }
    }
}
