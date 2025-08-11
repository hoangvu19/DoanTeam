using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace DoAnTeam12.Models.Account
{
    public class UserModel
    {
        [Key]
        public int Id { get; set; }


        [Required(ErrorMessage = "Username cannot be empty.")]
        [StringLength(30, MinimumLength = 4, ErrorMessage = "Username must be between 4 and 30 characters.")]
        public string UserName { get; set; }
        [Required(ErrorMessage = "Email cannot be empty.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        public string Email { get; set; }
        [Required(ErrorMessage = "Phone number cannot be empty.")]
        [Phone(ErrorMessage = "Invalid phone number format.")]
        public string Mobile { get; set; }

        [Required(ErrorMessage = "Password cannot be empty.")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be between 6 and 100 characters.")]
        public string Password { get; set; }

        public bool IsActive { get; set; }

        public bool IsRemember { get; set; }

        public string Role { get; set; }
        [Required]
        public string Token { get; set; }

        [Required(ErrorMessage = "Password cannot be empty.")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be between 6 and 100 characters.")]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; }

        [Required(ErrorMessage = "Please confirm your new password.")]
        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "Passwords do not match.")]
        public string ConfirmPassword { get; set; }
    }
}
