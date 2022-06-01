using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using NetCore_Model_View_Cortol_Created.Models;
using NetCore_Model_View_Cortol_Created.ViewModels;
using NLog.Web.LayoutRenderers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Security.Claims;
using System.Threading.Tasks;

namespace NetCore_Model_View_Cortol_Created.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdministrationController : Controller
    {
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly UserManager<ExtendIdentityUser> userManager1; // to get the users and display them with Roles 

        public AdministrationController(RoleManager<IdentityRole> roleManager,
                                        UserManager<ExtendIdentityUser> userManager1)
        {
            this.roleManager = roleManager;
            this.userManager1 = userManager1;
        }

        public IActionResult ListUsers()
        {
            var users = userManager1.Users;
            return View(users);
        }

        [HttpGet]
        [Authorize(Policy = "EditRolePolicyUsingCustomRequirement")]
        public async Task<IActionResult> ManageUserRoles(string userId)
        {
            ViewBag.UserId = userId;
            var user = await userManager1.FindByIdAsync(userId);
            if (user == null)
            {
                ViewBag.ErrorMessage = $"This user id {userId} cannot be found!";
                return View("Error");
            }

            var model = new List<ManageUserRolesViewModel>();

            foreach (var role in roleManager.Roles.ToList()) 
            {
                var roleDetails = new ManageUserRolesViewModel()
                {
                    RoleId = role.Id,
                    RoleNanme = role.Name
                };

                if(await userManager1.IsInRoleAsync(user, role.Name))
                {
                    roleDetails.isSelected = true;
                }
                else
                {
                    roleDetails.isSelected = false;
                }
                model.Add(roleDetails);
            } // end foreach 

            return View(model);
        }

        [HttpPost]
        [Authorize(Policy = "EditRolePolicyUsingCustomRequirement")] // custom poilcy that the admin can only edit other admins roles and claims and cant edit himself
        public async Task<IActionResult> ManageUserRoles(List<ManageUserRolesViewModel> model,string userId)
        {
            var user = await userManager1.FindByIdAsync(userId);
            if (user == null)
            {
                ViewBag.ErrorMessage = $"This user id {userId} cannot be found!";
                return View("Error");
            }

            // first we delete existing roles for the user
            var roles = await userManager1.GetRolesAsync(user); // get all user roles 
            var result = await userManager1.RemoveFromRolesAsync(user , roles); // then remove all the roles
            if (!result.Succeeded)
            {
                ViewBag.ErrorMessage = $"this user id {userId} is not found!";
                return View("Error");
            }

            // then we add all new roles which has isSelected = true 
            result = await userManager1.AddToRolesAsync(user,
                model.Where(selected => selected.isSelected).Select(name => name.RoleNanme));

            if (!result.Succeeded)
            {
                ViewBag.ErrorMessage = $"cannot add selected roles to user!:(";
                return View("Error");
            }

            return RedirectToAction("EditUser", new { userId = userId });
        }

        [HttpGet]
        public IActionResult CreateRole()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateRole(CreateRoleViewModel model)
        {
            if (ModelState.IsValid)
            {
                IdentityRole identityRole = new IdentityRole() { Name = model.RoleName }; // declare the role 
                IdentityResult identityResult = await roleManager.CreateAsync(identityRole); // 
                if (identityResult.Succeeded)
                {
                    return RedirectToAction("listroles", "administration");
                }
                // if not succeded
                foreach (IdentityError error in identityResult.Errors)
                {
                    ModelState.AddModelError("", $"There's an error {error.Description}");
                }
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult ListRoles()
        {
            return View(roleManager.Roles);
        }

        [HttpGet]
        [Authorize(Policy = "EditRolePolicy")] // only users with claim EditRole can access EditRole Action - 
        // we added the policy in startup file 
        public async Task<IActionResult> EditRole(string id)
        {
            IdentityRole result = await roleManager.FindByIdAsync(id);
            if (result == null)
            {
                ViewBag.ErrorMessage = $"Role with id {id} is not found!";
                return View("Error");
            }
            EditRoleViewModel role = new EditRoleViewModel()
            {
                Id = result.Id,
                RoleName = result.Name
            };

            role.Users = new List<string>(); // we have to declare this list to avoid Null Exception
            // we converted to list userManager1.Users.ToList() to avoid an error of (There is already an open DataReader )
            foreach (var user in userManager1.Users.ToList()) // we will see if there's users engaged to this role and add it to EditRoleViewModel Users Property
            {
                if (await userManager1.IsInRoleAsync(user, result.Name))
                {
                    role.Users.Add(user.UserName);
                }
            }

            return View(role);
        }

        [HttpPost]
        [Authorize(Policy = "EditRolePolicy")] // only users with claim EditRole can access EditRole Action - 
        // we added the policy in startup file 
        public async Task<IActionResult> EditRole(EditRoleViewModel model)
        {
            var result = await roleManager.FindByIdAsync(model.Id);
            if (result == null)
            {
                ViewBag.ErrorMessage = $"Role with id {model.Id} in not found!";
                return View("Error");
            }
            else
            {
                result.Name = model.RoleName;
                var succeded = await roleManager.UpdateAsync(result);
                if (succeded.Succeeded)
                {
                    return RedirectToAction("ListRoles");
                }

                foreach (var error in succeded.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> EditUsersInRole(string roleID) // method to add or remove users from a specified role using roleID as a parameter
        {
            ViewBag.roleId = roleID;

            var role = await roleManager.FindByIdAsync(roleID);
            if (role == null)
            {
                ViewBag.ErrorMessage = $"This id {roleID} is not found!";
                return View("Error");
            }

            var modelList = new List<UserRoleViewModel>();

            foreach (var user in userManager1.Users.ToList())
            {
                var userRoleViewModel = new UserRoleViewModel
                {
                    UserId = user.Id,
                    UserName = user.UserName,
                };

                if (await userManager1.IsInRoleAsync(user, role.Name))
                {
                    userRoleViewModel.isSelected = true;
                }
                else
                {
                    userRoleViewModel.isSelected = false;
                }
                modelList.Add(userRoleViewModel);
            }

            return View(modelList);

        }


        [HttpPost]
        public async Task<IActionResult> EditUsersInRole(List<UserRoleViewModel> model, string roleID)
        {
            var role = await roleManager.FindByIdAsync(roleID);
            if (role == null)
            {
                ViewBag.ErrorMessage = $"This rold id {roleID} is not found!";
            }

            // we are using for instead of foreach because in the view we used for loop to generate a unique check input names like [1].isSelected
            for (int i = 0; i < model.Count; i++)
            {
                var user = await userManager1.FindByIdAsync(model[i].UserId);

                IdentityResult result;
                if (model[i].isSelected && !(await userManager1.IsInRoleAsync(user, role.Name)))
                {
                    result = await userManager1.AddToRoleAsync(user, role.Name);
                }
                else if (!(model[i].isSelected) && await userManager1.IsInRoleAsync(user, role.Name))
                {
                    result = await userManager1.RemoveFromRoleAsync(user, role.Name);
                }
                else
                {
                    continue;
                }

                if (result.Succeeded)
                {
                    if (i < model.Count - 1)
                    {
                        continue;
                    }
                    else
                    {
                        return RedirectToAction("EditRole", new { id = roleID });
                    }
                }
            }

            return RedirectToAction("EditRole", new { id = roleID });
        }

        [HttpGet]
        public async Task<IActionResult> EditUser(string userId)
        {
            var user = await userManager1.FindByIdAsync(userId);
            if (user == null)
            {
                ViewBag.ErrorMessage = $"This user with id {userId} is not found!";
                return RedirectToAction("listusers", "Administrations");
            }

            var userRoles = await userManager1.GetRolesAsync(user);
            var userClaims = await userManager1.GetClaimsAsync(user);

            EditUserViewModel userToEdit = new EditUserViewModel()
            {
                Id = user.Id,
                Username = user.UserName,
                Email = user.Email,
                City = user.City,
                Roles = userRoles.ToList(),
                Claims = userClaims.Select(c => c.Type +" : " + c.Value).ToList()
            };

            //foreach (var role in userRoles)
            //{
            //    userToEdit.Roles.Add(role);
            //}
            //foreach (var claim in userClaims)
            //{
            //    userToEdit.Roles.Add(claim.ToString());
            //}


            return View(userToEdit);
        }

        [HttpPost]
        public async Task<IActionResult> EditUser(EditUserViewModel model)
        {

            var user = await userManager1.FindByIdAsync(model.Id);
            if (user == null)
            {
                ViewBag.ErrorMessage = $"This user with id {model.Id} is not found!";
                return RedirectToAction("listusers", "Administrations");
            }

            user.Id = model.Id;
            user.UserName = model.Username;
            user.Email = model.Email;
            user.City = model.City;

            var result = await userManager1.UpdateAsync(user);

            if (result.Succeeded)
            {
                return RedirectToAction("listusers", "administration");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);

            }
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var user = await userManager1.FindByIdAsync(id);

            if (user == null)
            {
                ViewBag.ErrorMessage = $"This user id {id} is not found!";
                return Redirect("Error");
            }
            else
            {
                try
                {
                    var result = await userManager1.DeleteAsync(user);
                    if (result.Succeeded)
                    {
                        return RedirectToAction("listusers", "administration");
                    }
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                    return View("listusers");
                }
                catch (DbUpdateException)
                {
                    ViewBag.ErrorTitle = $"This user {user.UserName} is in use! ";
                    ViewBag.ErrorMessage = $"This user {user.UserName} cannot be deleted, first delete the Roles related to, then try to delete again!";
                    return View("Error1","Error");
                }
            }

        }

        [HttpPost]
        [Authorize(Policy = "DeleteRolePolicy")] // only users with claim Delete Claim can access DeleteRole Action - 
        // we added the policy in startup file 
        public async Task<IActionResult> DeleteRole(string id)
        {
            var role = await roleManager.FindByIdAsync(id);

            if (role == null)
            {
                ViewBag.ErrorMessage = $"This role id {id} is not found!";
                return Redirect("Error");
            }
            else
            {
                try
                {
                    var result = await roleManager.DeleteAsync(role);
                    if (result.Succeeded)
                    {
                        return RedirectToAction("listusers", "administration");
                    }
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                    return View("listusers");
                }
                catch (DbUpdateException ex)
                {
                    ViewBag.ErrorTitle = $"This role {role.Name} is in use! ";
                    ViewBag.ErrorMessage = $"This role {role.Name} cannot be deleted, first delete the user related to it then try to delete it again!";
                    return View("Error1","Error");
                }
            }

        }

        [HttpGet]
        public async Task<IActionResult> ManageUserClaims(string userId)
        {
            ViewBag.userId = userId;
            var user = await userManager1.FindByIdAsync(userId);
            if(user == null)
            {
                ViewBag.ErrorMessage = $"This user {userId} is not found";
                return View("Error");
            }
            // get all claims for this user
            var existingUserClaims = await userManager1.GetClaimsAsync(user);

            // make a new modelview object of userClaim 
            var model = new UserClaimsVewModel()
            {
                UserId = userId
            };

            //then looping on our Claims in ClaimStore.AllClaims
            foreach(Claim claim in ClaimStore.AllClaims)
            {
                //create an object of UserClaim that have 2 properties of (ClaimType, isSelected)
                UserClaim userClaim = new UserClaim()
                {
                    ClaimType = claim.Type
                };
                //see if user claims matches OUR custom claims in ClaimStore type
                if(existingUserClaims.Any(claimSelected => claimSelected.Type == claim.Type && claimSelected.Value == "true"))
                {
                    //then make the bool value of isSelected = true
                    userClaim.isSelected = true;
                }
                model.Claims.Add(userClaim);
            }
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ManageUserClaims(UserClaimsVewModel model )
        {
           
            var user = await userManager1.FindByIdAsync(model.UserId);
            if (user == null)
            {
                ViewBag.ErrorMessage = $"This user is not found";
                return View("Error");
            }

            // get all claims for this user
            var existingUserClaims = await userManager1.GetClaimsAsync(user);
            // then remove all user claims because we'll straight forward adding the new claims later
            var resultRemovingClaims = await userManager1.RemoveClaimsAsync(user, existingUserClaims);

            if (!resultRemovingClaims.Succeeded)
            {
                ModelState.AddModelError("","Cant remove existing user claims");
                return View(model);
            }
            //adding new claims where isSelected is true then select the claim to be added 
            var resultAddingSelectedClaims = await userManager1.AddClaimsAsync(user,
                //model.Claims.Where(boolValue => boolValue.isSelected)   -- we commented this because we gonna store Claim Type and Claim Value
                //.Select(x => new Claim(x.ClaimType, x.ClaimType
                model.Claims.Select(x=> new Claim(x.ClaimType , x.isSelected ? "true" : "false")
                ));
            if (!resultAddingSelectedClaims.Succeeded)
            {
                ModelState.AddModelError("", "Cant add these user claims");
                return View(model);
            }
            return RedirectToAction("EditUser", new { userId = model.UserId });
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

    }
}
