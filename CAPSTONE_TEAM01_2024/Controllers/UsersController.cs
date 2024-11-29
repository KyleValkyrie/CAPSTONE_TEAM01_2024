using CAPSTONE_TEAM01_2024.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace CAPSTONE_TEAM01_2024.Controllers
{
    public class UsersController : Controller
    {
        private readonly SignInManager<ApplicationUser> signInManager;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly ApplicationDbContext _context;

        public UsersController(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager, ApplicationDbContext context)
        {
            this.signInManager = signInManager;
            this.userManager = userManager;
            this._context = context;
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Index(string? message = null)
        {
            if (message is not null)
            {
                ViewData["message"] = message;
            }
            return View();
        }

        [AllowAnonymous]
        [HttpGet]
        public ChallengeResult ExternalLogin(string provider, string? returnUrl = null)
        {
            var redirectUrl = Url.Action("RegisterExternalUser", values: new { returnUrl });
            var properties = signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            var challengeResult = new ChallengeResult(provider, properties);
            return challengeResult;
        }

        [AllowAnonymous]
        public async Task<IActionResult> RegisterExternalUser(string? returnURL = null, string? remoteError = null)
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

            string email = info.Principal.FindFirstValue(ClaimTypes.Email) ?? string.Empty;
            if (string.IsNullOrEmpty(email))
            {
                message = "Error while reading the email from the provider.";
                return RedirectToAction("Index", routeValues: new { message });
            }

            var existingUser = await _context.ApplicationUsers.FirstOrDefaultAsync(u => u.Email == email);
            if (existingUser != null)
            {
                var vietnamTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
                var applicationUser = await _context.ApplicationUsers.FirstOrDefaultAsync(au => au.Email == email);
                if (applicationUser != null)
                {
                    applicationUser.LastLoginTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vietnamTimeZone);
                    applicationUser.IsRegistered = true;
                    _context.Update(applicationUser);
                    await _context.SaveChangesAsync();
                }

                var roles = await userManager.GetRolesAsync(existingUser);
                HttpContext.Session.SetString("Roles", string.Join(",", roles));

                var loginResult = await userManager.AddLoginAsync(existingUser, info);
                if (loginResult.Succeeded || loginResult.Errors.Any(x => x.Code == "LoginAlreadyAssociated"))
                {
                    await signInManager.SignInAsync(existingUser, isPersistent: true, info.LoginProvider);
                    return LocalRedirect("~/Home/HomePage");
                }
                message = "There was an error while logging you in.";
                return RedirectToAction("Index", routeValues: new { message });
            }

            // New user registration
            var newUser = new ApplicationUser
            {
                Id = Guid.NewGuid().ToString(),
                Email = email,
                UserName = email,
                IsRegistered = true,
                LastLoginTime = DateTime.UtcNow
            };

            var createUserResult = await userManager.CreateAsync(newUser);
            if (!createUserResult.Succeeded)
            {
                message = createUserResult.Errors.First().Description;
                return RedirectToAction("Index", routeValues: new { message });
            }
            var addLoginResult = await userManager.AddLoginAsync(newUser, info);
            if (addLoginResult.Succeeded)
            {
                await signInManager.SignInAsync(newUser, isPersistent: true, info.LoginProvider);
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

        [HttpGet]
        public async Task<IActionResult> FindUser(string query)
        {
            // Query the Users table and join the Roles table via AspNetUserRoles
            var users = _context.ApplicationUsers
                                .Where(u => u.Email.Contains(query) || u.FullName.Contains(query))
                                .Join(_context.UserRoles,
                                      user => user.Id,
                                      userRole => userRole.UserId,
                                      (user, userRole) => new { user, userRole })
                                .Join(_context.Roles,
                                      userRole => userRole.userRole.RoleId,
                                      role => role.Id,
                                      (userRole, role) => new { userRole.user, role })
                                .ToList();

            // Map users to a view model or anonymous object with role information
            var result = users.Select(user => new
            {
                user.user.Email,
                user.user.FullName,
                RoleName = user.role.Name,  // Access role name
            }).ToList();

            return Json(result);
        }
    }

}
