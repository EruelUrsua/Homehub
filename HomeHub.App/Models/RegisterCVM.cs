﻿using System.ComponentModel.DataAnnotations;

namespace HomeHub.App.Models
{
    public class RegisterCVM
    {
        public RegisterCVM()
        {
            Lastname = "";
            Firstname = "";
            Email = "";
            Password = "";
            ConfirmPassword = "";
            ContactNo = "";
            Address = "";
            lat = 14.58699;
            lng = 120.98634;
        }

        [Required]
        [MaxLength]
        public string Lastname { get; set; }
        [Required]
        public string Firstname { get; set; }
        [Required]
        public string Email { get; set; }
        [Required]
        [Phone]
        public string ContactNo { get; set; }
        [Required]
        public string Address { get; set; }

        [Required]
        public double lat { get; set; }

        [Required]
        public double lng { get; set; }


        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }
        [Required]
        [DataType(DataType.Password)]
        [Compare("Password")]
        public string ConfirmPassword { get; set; }
        public IFormFile ValidId { get; set; }
        public string? ExistingValidId { get; set; }
    }
}
