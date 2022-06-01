using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;
using NetCore_Model_View_Cortol_Created.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace NetCore_Model_View_Cortol_Created.ViewModels
{
    public class PhotoViewModel
    {
        [Required]
        [Display(Name = "Name")]
        [MaxLength(50, ErrorMessage = "Cannot exceed 50 characters")]
        public string name { get; set; }

        [Required]
        [Display(Name = "Office Email")]
        [RegularExpression(@"^[a-zA-Z0-9_.+-]+@[a-zA-Z0-9-]+\.[a-zA-Z0-9-.]+$",
            ErrorMessage = "Invalid email format")]
        public string email { get; set; }

        [Required]
        public Dept? Department { get; set; } // we make Dept? to override Required, because by default enum with int values is required by default
                                              // so we make Dept optional by Dept? then add Requird to show the correct error
        public IFormFile Photo { get; set; }

        //public List<IFormFile> Photos { get; set; } to apply multiple files upload
    }
}
