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
        [DataType(DataType.Password)]
        public string Password { get; set; }
        [Required]
        [DataType(DataType.Password)]
        [Compare("Password")]
        public string ConfirmPassword { get; set; } 
    }
}
