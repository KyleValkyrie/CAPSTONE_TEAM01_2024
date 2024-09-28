using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CAPSTONE_TEAM01_2024.Controllers
{
    public class UsersController: Controller
    {
        // Sign in the user
        private readonly SignInManager<IdentityUser> signInManager;
        // Create user if not already in database
        private readonly UserManager<IdentityUser> userManager;

        public UsersController(SignInManager<IdentityUser> signInManager, 
                               UserManager<IdentityUser> userManager)
        {
            this.signInManager = signInManager;
            this.userManager = userManager;
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Index(string? message = null)
        {
            //if login error, throw error report
            if(message is not null)
            {
                ViewData["message"] = message;
            }
            return View();
        }

        //redirect to login via 3rd parties
        [AllowAnonymous]
        [HttpGet]
        public ChallengeResult ExternalLogin(string provider, string? returnUrl = null) 
        {
            var redirectUrl = Url.Action("RegisterExternalUser", values: new {returnUrl});
            var properties = signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            var challengeResult = new ChallengeResult(provider, properties);
            return challengeResult;
        }

        //register user into our database
        [AllowAnonymous]
        public async Task<IActionResult> RegisterExternalUser(string? returnURL = null,
        string? remoteError = null)
        {
            returnURL = returnURL ?? Url.Content("~/");
            var message = "";

            if (remoteError != null)
            {
                message = $"Error from external provider: {remoteError}";
                return RedirectToAction("Index", routeValues: new { message });
            }

            var info = await signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                message = "Error loading external login information.";
                return RedirectToAction("Index", routeValues: new { message });
            }

            ExternalLogin(info.LoginProvider, "~/");
            var externalLoginResult = await signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor: true);

            if (externalLoginResult.Succeeded)
            {
                return LocalRedirect("~/Home/HomePage");
            }

            string email = "";

            if (info.Principal.HasClaim(c => c.Type == ClaimTypes.Email))
            {
                email = info.Principal.FindFirstValue(ClaimTypes.Email)!;
            }
            else
            {
                message = "Error while reading the email from the provider.";
                return RedirectToAction("Index", routeValues: new { message });
            }

            var user = new IdentityUser() { Email = email, UserName = email };

            var findUser = await userManager.FindByEmailAsync(email);

            if (findUser != null)
            {
                var loginResult = await userManager.AddLoginAsync(findUser, info);

                if (loginResult.Succeeded || loginResult.Errors.Any(x=>x.Code== "LoginAlreadyAssociated"))
                {
                    await signInManager.SignInAsync(user, isPersistent: true, info.LoginProvider);
                    return LocalRedirect("~/Home/HomePage");
                }

                message = "There was an error while logging you in.";
                return RedirectToAction("Index", routeValues: new { message });
            }

            var createUserResult = await userManager.CreateAsync(user);
            if (!createUserResult.Succeeded)
            {
                message = createUserResult.Errors.First().Description;
                return RedirectToAction("Index", routeValues: new { message });
            }

            var addLoginResult = await userManager.AddLoginAsync(user, info);

            if (addLoginResult.Succeeded)
            {
                await signInManager.SignInAsync(user, isPersistent: true, info.LoginProvider);
                return LocalRedirect("~/Home/HomePage");
            }

            message = "There was an error while logging you in.";
            return RedirectToAction("Index", routeValues: new { message });
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(IdentityConstants.ApplicationScheme);
            return RedirectToAction("Index", "Users");
        }
    }
}
