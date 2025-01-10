using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CAPSTONE_TEAM01_2024.Controllers
{
    public class CentralController:Controller
    {
        // Database Context
        private readonly ApplicationDbContext _context;
        public CentralController(ApplicationDbContext context)
        {
            _context = context;
        }
        [HttpGet]
        public async Task<IActionResult> GetUserInfo()
        {
            if (string.IsNullOrEmpty(User.Identity?.Name))
                return Unauthorized(); // User is not authenticated

            string email = User.Identity.Name;
            var currentUser = await _context.ApplicationUsers.FirstOrDefaultAsync(u => u.Email == email);

            if (currentUser == null)
                return NotFound(); // User not found

            var roleName = await (from user in _context.Users
                                  join userRole in _context.UserRoles on user.Id equals userRole.UserId
                                  join role in _context.Roles on userRole.RoleId equals role.Id
                                  where user.Email == email
                                  select role.Name).FirstOrDefaultAsync();

            return Ok(new
            {
                FullName = currentUser.FullName,
                Role = roleName,
                Email = email // Include email as fallback
            });
        }
    }
}
