﻿using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tomasos.Models;

namespace Tomasos.IdentityModels
{
    public class ApplicationUser : IdentityUser
    {
        public string Street { get; set; }
        public string Postcode { get; set; }
        public string City { get; set; }
        public int? Points { get; set; }

        public virtual ICollection<Bestallning> Bestallning { get; set; }
    }
}
