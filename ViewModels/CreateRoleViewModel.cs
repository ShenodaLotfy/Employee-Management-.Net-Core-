using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace NetCore_Model_View_Cortol_Created.ViewModels
{
    public class CreateRoleViewModel
    {
        [Display(Name ="Role Name")]
        [Required]
        public string RoleName { get; set; }
    }
}
