using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace NetCore_Model_View_Cortol_Created.Models
{
    public static class ClaimStore // we can save our claims in classes or static file or databases - here we used class 
    {
        public static List<Claim> AllClaims = new List<Claim>()
        {
            new Claim("Create Role", "Create Role"),
            new Claim("Delete Role", "Delete Role"),
            new Claim("Edit Role", "Edit Role")
        };
    }
}
