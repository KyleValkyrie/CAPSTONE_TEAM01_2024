using CAPSTONE_TEAM01_2024.Models;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace CAPSTONE_TEAM01_2024.Utilities
{
    public static class RoleHelper
    {
        public static async Task<string> GetUserRoleAsync(UserManager<ApplicationUser> userManager, ClaimsPrincipal user)
        {
            var appUser = await userManager.GetUserAsync(user);
            var roles = await userManager.GetRolesAsync(appUser);

            if (roles.Contains("Cố Vấn") || roles.Contains("Khoa"))
            {
                return "_PersonnelLayout";
            }
            else
            {
                return "_StudentLayout";
            }
        }
    }
}
