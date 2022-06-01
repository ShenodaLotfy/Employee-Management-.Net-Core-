using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Razor.Language.Intermediate;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.Extensions.Logging;
using NetCore_Model_View_Cortol_Created.Models;
using NetCore_Model_View_Cortol_Created.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace NetCore_Model_View_Cortol_Created.Controllers
{
    [Authorize] // cant access actions - atleast u must login first 
    public class AccountController : Controller
    {
        private readonly UserManager</*IdentityUser*/ ExtendIdentityUser> userManager; //change IdentityUser to ExtendIdentityUse thats add City Property
        private readonly SignInManager</*IdentityUser*/ ExtendIdentityUser> signInManager; //change IdentityUser to ExtendIdentityUse thats add City Property
        private readonly ILogger<AccountController> logger;

        //inject UserManager and SignInManager to deal with users and signins 
        public AccountController(UserManager<ExtendIdentityUser> userManager,
                                    SignInManager<ExtendIdentityUser> signInManager,
                                    ILogger<AccountController> logger)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.logger = logger;
        }

        [AllowAnonymous] // this makes ability to access this action while we used [Authorize] on Controller 
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await signInManager.SignOutAsync();
            return RedirectToAction("index", "home");
        }
        [AllowAnonymous] // this makes ability to access this action while we used [Authorize] on Controller 
        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new ExtendIdentityUser { UserName = model.Email, Email = model.Email,
                    City = model.City };

                var Result = await userManager.CreateAsync(user, model.Password);


                if (Result.Succeeded)
                {
                    // if user add to AspNetUsers 
                    // create his token for email confirmation
                    var token1 = await userManager.GenerateEmailConfirmationTokenAsync(user);
                    // then create confirmation link 
                    var confirmationLink = Url.Action("ConfirmEmail", "Account",
                                            new { userId = user.Id, token = token1 }, Request.Scheme); //http or https

                    logger.Log(LogLevel.Warning, confirmationLink);

                    if (signInManager.IsSignedIn(User) && User.IsInRole("Admin"))
                    {
                        return RedirectToAction("listusers", "administration");
                    }

                    ViewBag.ErrorTitle = "Registeration Successful";
                    ViewBag.ErrorMessage = "Before you can Login, please confirm your email, by clicking on" +
                        " the confirmation link we have emailed you!";
                    return View("Error1");
                    //await signInManager.SignInAsync(user, isPersistent: false);
                    //return RedirectToAction("index", "home");
                }

                // if not succed
                foreach (var error in Result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                    // this error appears in Register view in  <div asp-validation-summary="All" class="text-danger"></div>
                }
            }
            return View(model);
        }
        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> ConfirmEmail(string userId , string token)
        {
            if(userId == null || token == null)
            {
                return View("index", "Home");
            }

            var user = await userManager.FindByIdAsync(userId);
            if(user == null)
            {
                ViewBag.ErrorMessage = $"This user id {userId } is not valid!";
                return View("Error1");
            }

            // then confirm the email for this user using token 
            var result = await userManager.ConfirmEmailAsync(user, token);
            if (result.Succeeded)
            {   
                return View();
            }

            ViewBag.ErrorMessage = "Email can not be confirmed successfully";
            return View("Error1");
        }

        [AllowAnonymous] // this makes ability to access this action while we used [Authorize] on Controller 
        [HttpGet]
        public async Task<IActionResult> Login(string returnUrl) // external logins we have to pass returnUrl string, change the Login to async Task<> 
        {
            LoginViewModel model = new LoginViewModel()
            {
                RedirectUrl = returnUrl,
                // getting all external login providers like facebook, google etc - convert it ToList() 
                ExternalLogins = (await signInManager.GetExternalAuthenticationSchemesAsync()).ToList()
            };

            return View(model);
        }
        [AllowAnonymous]
        [HttpPost]
        public IActionResult ExternalLogin(string provider , string returnUrl) // provide is like google,facebook, etc - getting its value while pressing Provider button in the Login View
        {
            string redirectUrl = Url.Action("ExternalLoginCallback", "Account",
                                            new { returnUrl = returnUrl }); // the url to redirect to it when external authentication is success
            var properties = signInManager.
                ConfigureExternalAuthenticationProperties(provider, redirectUrl); // setting properties to external authentication
            return new ChallengeResult(provider, properties);
        }

        [AllowAnonymous]
        
        public async Task<IActionResult> ExternalLoginCallback(string returnUrl = null ,string remoteError= null)
        {
            // if we tried to access ManageRoles action theren returnUrl will catch path /administration/ManageRoles
            // so if returnUrl = null make default path is "~/"
            returnUrl = returnUrl ?? Url.Content("~/");

            LoginViewModel model = new LoginViewModel()
            {
                RedirectUrl = returnUrl,
                ExternalLogins = (await signInManager.GetExternalAuthenticationSchemesAsync()).ToList()
            };

            if(remoteError != null){
                ModelState.AddModelError("", $"Error from external provider: {remoteError}");
                return View("Login", model); // return model to Login view so that user can see the errors
            }

            var externalInformation = await signInManager.GetExternalLoginInfoAsync();

            if( externalInformation == null )
            {
                ModelState.AddModelError("", $"Error loading external login information!");
                return View("Login", model);
            }
            // table UserLogins - success if there;s a row for this user in AspNetUserLogins - 
            var loginResult = await signInManager.ExternalLoginSignInAsync(externalInformation.LoginProvider,
                                     externalInformation.ProviderKey,isPersistent: false, bypassTwoFactor: true);
            
            //get the email address from the user external information (user variable)
            var email = externalInformation.Principal.FindFirstValue(ClaimTypes.Email);
            ExtendIdentityUser user = null;
            
            //if we got the email address successfuly - check if the 
            if(email != null)
            {
                // then check in AspNetUsers table if there's an email like this user email 
                user = await userManager.FindByEmailAsync(email);
                // if user exist and hasnt confirm his email address yet
                if (user != null && !user.EmailConfirmed)
                {
                    ModelState.AddModelError("", "Email is not confirmed yet");
                    return View("Login", model);
                }

            }
            if (loginResult.Succeeded) // if the user has a row in AspNetUserLogins - then login successfuly
            {
                return LocalRedirect(returnUrl);
            }
            else // if the user hasnot register before and has no row in the AspNetUserLogins - then check if he has a Local account in AspNetUsers table.
            {
                // if there's an email - then there's no errors recieving external information 
                if (email != null)
                {

                    //if there's no such user in AspNetUsers table 
                    if(user == null)
                    {
                        // then create a new user 
                        user = new ExtendIdentityUser
                        {
                            UserName = externalInformation.Principal.FindFirstValue(ClaimTypes.Email),
                            Email = externalInformation.Principal.FindFirstValue(ClaimTypes.Email)
                        };

                        await userManager.CreateAsync(user);
                        // if user add to AspNetUsers 
                        // create his token for email confirmation
                        var token1 = await userManager.GenerateEmailConfirmationTokenAsync(user);
                        // then create confirmation link 
                        var confirmationLink = Url.Action("ConfirmEmail", "Account",
                                                new { userId = user.Id, token = token1 }, Request.Scheme); //http or https

                        logger.Log(LogLevel.Warning, confirmationLink);

                        ViewBag.ErrorTitle = "Registeration Successful";
                        ViewBag.ErrorMessage = "Before you can Login, please confirm your email, by clicking on" +
                            " the confirmation link we have emailed you!";
                        return View("Error1");
                        // then add him in AspNetUsers table
                        
                    }

                    //after adding user to AspNetUsers, add him in AspNetUserLogins table
                    await userManager.AddLoginAsync(user, externalInformation);
                    // then sign in user 
                    await signInManager.SignInAsync(user, isPersistent: false);
                    return LocalRedirect(returnUrl);
                }

                // if we didnt recieve the email itself 
                ViewBag.ErrorTitle = $"Email claim not received from: {externalInformation.LoginProvider}";
                ViewBag.ErrorMessage = "Please conatact support shenoda1@gmail.com";
                return View("Error");
            }
        }

        [AllowAnonymous] // this makes ability to access this action while we used [Authorize] on Controller 
        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model, string returnUrl)
        {
            // returnUrl is the url that is passed to login action is from, when the user want to access to create page
            // he cant because we used [Authorize] so he have to login first 
            // so the url changed to localhost/account/login/returnUrl="here is the url that he tried to access in the beginning"
            // so this url is an valuable security hole to make hackers hack the website
            // so we have to check if this url is local url - local url is a url that is local in our application like home/index for example 
            // we use Url.isLocal(returnUrl) or 
            if (ModelState.IsValid)
            {
                //we declared it to avoid null exception in rendering View
                model.ExternalLogins = (await signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
                // result = NotAllowed if the user hasnt confirm his email address
                var result = await signInManager.PasswordSignInAsync(model.Email, model.Password,
                    isPersistent: model.RememberMe, false);

                
                // get the user to check if he has confrirmed his email address or nt yet
                var user = await userManager.FindByEmailAsync(model.Email);
                //if user exist and email not confirmed yet and email-password are correct
                if (user != null && !user.EmailConfirmed && 
                                   (await userManager.CheckPasswordAsync(user,model.Password)))
                {
                    ModelState.AddModelError("", "Email address not confirmed yet.");
                    return View(model);
                }

                if (result.Succeeded) // url.islocalUrl help prevent hackers to hack website - stole use information
                {
                    
                    if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    {

                        return Redirect(returnUrl); // if url has ReturnUrl that's local to our Application
                    }
                    else
                    {
                        // if ReturnUrl is redirect user to another url that is not familar - not localurl so direct use to index action
                        return RedirectToAction("index", "home");
                    }
                }

 
                ModelState.AddModelError("", "Invalid username or password! try again later!");
                // this error appears in Login view in  <div asp-validation-summary="All" class="text-danger"></div>

            }
            return View(model);
        }

        [AllowAnonymous]
        [HttpPost][HttpGet] // responds to both get and post requests - immediatly getting validate while typing the email
        public async Task<IActionResult> IsEmailInUse(string email) // to remote validate if the email is in use or not
        {
            var result = await userManager.FindByEmailAsync(email);
            if (result == null) // if the email is not taken - is not used
            {
                return Json(true);
            }
            else
                return Json($"Email {email} is already in use! ");
        }
        [AllowAnonymous]
        [HttpGet]
        public  IActionResult ForgotPassword()
        {
            return View();
        }
        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordModelView model)
        {
            if (ModelState.IsValid)
            {
                var user = await userManager.FindByEmailAsync(model.Email);
                if (user != null && await userManager.IsEmailConfirmedAsync(user))
                {
                    // if user has a confirmed email address and is exist- then create a token for password reset
                    var token = await userManager.GeneratePasswordResetTokenAsync(user);
                    // then create password reset link including - action method,controller, email, token
                    var passwordResetLink = Url.Action("ResetPassword", "Account",
                                                    new { email = model.Email, token = token }, Request.Scheme);

                    logger.Log(LogLevel.Warning, passwordResetLink); // save the link inside config file

                    return View("ForgotPasswordConfirmation");
                }

                return View("ForgotPasswordConfirmation");
            }
            return View(model);
        }
        [AllowAnonymous]
        [HttpGet]
        public IActionResult ResetPassword(string email, string token)
        {
            if(email == null || token == null)
            {
                return View("Login", "Account");
            }
            return View();
        }
        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            var user = await userManager.FindByEmailAsync(model.Email);
            if (user != null)
            {
                // reset method
                var result = await userManager.ResetPasswordAsync(user, model.Token, model.Password);
                if (result.Succeeded)
                {
                    return View("ResetPasswordConfirmation");
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }

                return View(model);
            }
            // also return him to ResetPasswordConfirmation view even if not succeded to prevent or reduce bruteforce attact 
            return View("ResetPasswordConfirmation");

        }
    }
}
