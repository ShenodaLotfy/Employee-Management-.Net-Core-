using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace NetCore_Model_View_Cortol_Created.ViewModels
{
    public class EditRoleViewModel
    {
        public string Id { get; set; }
        [Display(Name = "Role Name")]
        [Required(ErrorMessage ="Role name is required!")]
        public string RoleName { get; set; }
        public List<string> Users { get; set; }
    }
}
