﻿using Microsoft.AspNetCore.Identity;
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

        public double lat { get; set; }

        public double lng { get; set; }

        public string ValidId { get; set; }

        public bool IsUnderReview { get; set; }

        public DateTime? LastReviewDate { get; set; }

        public bool IsRestricted { get; set; }

        public bool IsVerified { get; set; }

        //public ICollection<ApplicationUserRole> UserRoles { get; set; }

        //public string? BusinessName { get; set; }    

        //public String? Category { get; set; }

        //public bool? BusinessType { get; set; }
    }
}
