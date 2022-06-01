using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Razor.Language.Intermediate;
using NLog.Web.LayoutRenderers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Security.Claims;
using System.Threading.Tasks;

namespace NetCore_Model_View_Cortol_Created.Security
{
    public class CanEditOnlyOtherAdminRolesAndClaimsHandler :
        AuthorizationHandler<ManageAdminRolesAndClaimsRequirement> // inheritance of AuthorizationHandler and pass our custom Requirement class 
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context,
            ManageAdminRolesAndClaimsRequirement requirement)
        {
            // resouce hold the actions that we want to authorize access to it like ManageClaims 
            //or ManageRoles in Administration Controller
            var authFilterContext = context.Resource as AuthorizationFilterContext;
            if (authFilterContext == null)
            {
                return Task.CompletedTask; //if there's no actions to authorize then return CompletedTask and deny access
            }
            // then we need loggedin admin ID 
            string loggedInAdminId =
                context.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value;

            //then we need adminBeingEditedId
            string adminBeingEditedID = authFilterContext.HttpContext.Request.Query["userId"];

            // the set the conditions that must be happen to be able to edit other admin roles and claims
            if (context.User.IsInRole("Admin") && context.User.HasClaim(c => c.Type == "Edit Role" && c.Value == "true")
                && adminBeingEditedID.ToLower() != loggedInAdminId.ToLower()) // if the logged user editing him self then he got access denied because he can only edit other admins claims and roles
            {
                context.Succeed(requirement);
            }
            return Task.CompletedTask;


        }
    }
}
