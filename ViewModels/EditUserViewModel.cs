using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace NetCore_Model_View_Cortol_Created.ViewModels
{
    public class EditUserViewModel
    {
        public EditUserViewModel()
        {
            Roles = new List<string>();
            Claims = new List<string>();
        }
        public string Id { get; set; }
        [Required] [EmailAddress]
        public string Email { get; set; }
        [Required]
        public string Username { get; set; }
        public string City { get; set; }
        public List<string> Roles { get; set; }
        public List<string> Claims { get; set; } 
    }
}
