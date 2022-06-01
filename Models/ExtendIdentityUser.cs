using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NetCore_Model_View_Cortol_Created.Models
{
    public class ExtendIdentityUser : IdentityUser
    {
        public string City { get; set; }
    }
}
