using Microsoft.AspNetCore.Mvc;
using NetCore_Model_View_Cortol_Created.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace NetCore_Model_View_Cortol_Created.ViewModels
{
    public class RegisterViewModel
    {
        [Required]
        [EmailAddress]
        [EmailDomain(ourDomain: "gmail.com", ErrorMessage ="Invalid domain!, it must be @gmail.com")]
        [Remote(action: "IsEmailInUse", controller:"Account", ErrorMessage ="This email is  in use!")]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm Password")]
        [Compare("Password",ErrorMessage = "Password and confirm password do not match. ")]
        public string ConfirmPassword { get; set; }
        public string City { get; set; }

    }
}
