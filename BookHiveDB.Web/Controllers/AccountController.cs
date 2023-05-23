using BookHiveDB.Domain.DomainModels;
using BookHiveDB.Domain.DTO;
using BookHiveDB.Domain.Identity;
using BookHiveDB.Service.Interface;
using ExcelDataReader;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace BookHiveDB.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<BookHiveApplicationUser> userManager;
        private readonly SignInManager<BookHiveApplicationUser> signInManager;
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly IAuthorService authorService;

        public AccountController(UserManager<BookHiveApplicationUser> userManager, SignInManager<BookHiveApplicationUser> signInManager, RoleManager<IdentityRole> roleManager)
        {

            this.userManager = userManager;
            this.signInManager = signInManager;
            this.roleManager = roleManager;
        }



        public IActionResult Profile(bool success, bool error, bool passwordShort)
        {
            if (success)
            {
                ViewData["success"] = true;
            }
            if (error)
            {
                ViewData["error"] = true;
            }
            if (passwordShort)
            {
                ViewData["passwordShort"] = true;
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = userManager.FindByIdAsync(userId).Result;

            return View(user);
        }

        public IActionResult EditProfile(string Name, string LastName, string Address, string password, string confirmPassword)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = userManager.FindByIdAsync(userId).Result;

            

            if (password !=null &&!password.Equals(confirmPassword))
            {
                return RedirectToAction("Profile", new { error = true });
            }

            user.FirstName = Name;
            user.LastName = LastName;
            user.Address = Address;

            if (password == null)
            {

                var updateResult = userManager.UpdateAsync(user).Result;
                return RedirectToAction("Profile", new { success = true });
            }



            var token = userManager.GeneratePasswordResetTokenAsync(user).Result;
            var result = userManager.ResetPasswordAsync(user, token, password).Result;
            if (!result.Succeeded)
            {
                return RedirectToAction("Profile", new { passwordShort = true });
            }

            var updateResultWithPasswordChange = userManager.UpdateAsync(user).Result;

            return RedirectToAction("Profile", new { success = true });
        }

        public IActionResult Register()
        {
            UserRegistrationDto model = new UserRegistrationDto();
            return View(model);
        }

        [HttpPost, AllowAnonymous]
        public async Task<IActionResult> Register(UserRegistrationDto request)
        {
            if (ModelState.IsValid)
            {
                var userCheck = await userManager.FindByEmailAsync(request.Email);
                if (userCheck == null)
                {
                    var user = new BookHiveApplicationUser
                    {
                        FirstName = request.Name,
                        LastName = request.LastName,
                        UserName = request.Email,
                        NormalizedUserName = request.Email,
                        Email = request.Email,
                        EmailConfirmed = true,
                        UserCart = new ShoppingCart()
                    };
                    var result = await userManager.CreateAsync(user, request.Password);

                    if (result.Succeeded)
                    {
                        var roleExist = await roleManager.RoleExistsAsync("StandardUser");
                        if (!roleExist)
                        {
                            await roleManager.CreateAsync(new IdentityRole("StandardUser"));
                        }
                        await userManager.AddToRoleAsync(user, "StandardUser");
                        return RedirectToAction("Login");
                    }
                    else
                    {
                        if (result.Errors.Count() > 0)
                        {
                            foreach (var error in result.Errors)
                            {
                                ModelState.AddModelError("message", error.Description);
                            }
                        }
                        return View(request);
                    }
                }
                else
                {
                    ModelState.AddModelError("message", "Email already exists.");
                    return View(request);
                }
            }
            return View(request);

        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Login(string returnUrl)
        {
            UserLoginDto model = new UserLoginDto { ReturnUrl = returnUrl, ExternalLogins = (await signInManager.GetExternalAuthenticationSchemesAsync()).ToList() };
            return View("~/Views/Home/Index.cshtml", model);
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Login(UserLoginDto model)
        {
            if (ModelState.IsValid)
            {
                var user = await userManager.FindByEmailAsync(model.Email);
                
                if (user != null && !user.EmailConfirmed)
                {
                    ModelState.AddModelError("message", "Email not confirmed yet");
                    return View("~/Views/Home/Index.cshtml", model);

                }
                if (await userManager.CheckPasswordAsync(user, model.Password) == false)
                {
                    ModelState.AddModelError("message", "Invalid credentials");
                    return View("~/Views/Home/Index.cshtml", model);

                }

                var result = await signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, true);

                if (result.Succeeded)
                {
                    await userManager.AddClaimAsync(user, new Claim("UserRole", "Admin"));
                    return RedirectToAction("Index", "User");
                }
                else
                {
                    ModelState.AddModelError("message", "Invalid login attempt");
                    return View("~/Views/Home/Index.cshtml", model);
                }
            }
            return View("~/Views/Home/Index.cshtml", model);
        }

        [AllowAnonymous]
        [HttpPost]
        public IActionResult ExternalLogin(string provider, string returnUrl)
        {
            var redirectUrl = Url.Action("ExternalLoginCallback", "Account",
                                new { ReturnUrl = returnUrl });
            var properties = signInManager
                .ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            return new ChallengeResult(provider, properties);
        }

        [AllowAnonymous]
        public async Task<IActionResult>
            ExternalLoginCallback(string returnUrl = null, string remoteError = null)
        {
            returnUrl = returnUrl ?? Url.Content("~/");

            UserLoginDto loginViewModel = new UserLoginDto
            {
                ReturnUrl = returnUrl,
                ExternalLogins =
                        (await signInManager.GetExternalAuthenticationSchemesAsync()).ToList()
            };

            if (remoteError != null)
            {
                ModelState
                    .AddModelError(string.Empty, $"Error from external provider: {remoteError}");

                return View("Login", loginViewModel);
            }

            var info = await signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                ModelState
                    .AddModelError(string.Empty, "Error loading external login information.");

                return View("Login", loginViewModel);
            }

            var signInResult = await signInManager.ExternalLoginSignInAsync(info.LoginProvider,
                info.ProviderKey, isPersistent: false, bypassTwoFactor: true);

            if (signInResult.Succeeded)
            {
                return LocalRedirect(returnUrl);
            }
            else
            {
                var email = info.Principal.FindFirstValue(ClaimTypes.Email);

                if (email != null)
                {
                    var user = await userManager.FindByEmailAsync(email);

                    if (user == null)
                    {
                        user = new BookHiveApplicationUser
                        {
                            UserName = info.Principal.FindFirstValue(ClaimTypes.Email),
                            Email = info.Principal.FindFirstValue(ClaimTypes.Email),
                            UserCart = new ShoppingCart()
                        };

                        await userManager.CreateAsync(user);
                        var roleExist = await roleManager.RoleExistsAsync("StandardUser");
                        if (!roleExist)
                        {
                            await roleManager.CreateAsync(new IdentityRole("StandardUser"));
                        }
                        await userManager.AddToRoleAsync(user, "StandardUser");
                    }

                    await userManager.AddLoginAsync(user, info);
                    await signInManager.SignInAsync(user, isPersistent: false);

                    return LocalRedirect("/user");
                }

                ViewBag.ErrorTitle = $"Email claim not received from: {info.LoginProvider}";
                ViewBag.ErrorMessage = "Please contact support.";

                return View("Error");
            }
        }

        public async Task<IActionResult> Logout()
        {
            await signInManager.SignOutAsync();
            return RedirectToAction("Login", "Account");
        }
    }
}
