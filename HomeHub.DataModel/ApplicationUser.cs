using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeHub.DataModel
{
    public class ApplicationUser : IdentityUser
    {
        public string Lastname { get; set; }
        public string Firstname { get; set; }
        public string Address { get; set; }

        //public string? BusinessName { get; set; }    

        //public String? Category { get; set; }

        //public bool? BusinessType { get; set; }
    }
}
