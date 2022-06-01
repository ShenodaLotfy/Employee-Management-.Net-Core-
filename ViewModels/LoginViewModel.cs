using Microsoft.AspNetCore.Authentication;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace NetCore_Model_View_Cortol_Created.ViewModels
{
    public class LoginViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Display(Name ="Remember Me ")]
        public bool RememberMe { get; set; }

        public string RedirectUrl { get; set; } // this is for external login providers like facebook,google, etc
        public IList<AuthenticationScheme> ExternalLogins { get; set; } // this is the list of external login providers like facebook,google, etc
    }
}
