using CAPSTONE_TEAM01_2024.Models;
using CAPSTONE_TEAM01_2024.Utilities;
using CAPSTONE_TEAM01_2024.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Drawing.Printing;

namespace CAPSTONE_TEAM01_2024.Controllers
{
    public class ProfileManagerController : Controller
    {
        // Database Context
        private readonly ApplicationDbContext _context;
        public ProfileManagerController(ApplicationDbContext context)
        {
            _context = context;
        }
// StudentProfiles actions
    //Render view
        public async Task<IActionResult> StudentProfiles(int pageIndex = 1, int pageSize = 20)
        {
            ViewData["page"] = "StudentProfiles";
            var studentsQuery = _context.ApplicationUsers
                .Where(u => u.Email.EndsWith("@vanlanguni.vn"))
                .Select(user => new StudentProfileViewModel
                {
                    Id = user.Id,
                    Email = user.Email,
                    SchoolId = user.SchoolId,
                    FullName = user.FullName,
                    IsRegistered = user.IsRegistered,
                    LastLoginTime = user.LastLoginTime,
                    Role = "Student"
                });

            var students = await PaginatedList<StudentProfileViewModel>.CreateAsync(studentsQuery.AsNoTracking(), pageIndex, pageSize);

            ViewBag.Warning = TempData["Warning"];
            ViewBag.Success = TempData["Success"];
            ViewBag.Error = TempData["Error"];

            return View(students);
        }
    //Add Students
        [HttpPost]
        public async Task<IActionResult> CreateStudentProfile(ApplicationUser applicationUser)
        {
            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == applicationUser.Email);
            if (existingUser != null)
            {
                TempData["Warning"] = "Email đã tồn tại trong hệ thống!";
                return RedirectToAction(nameof(StudentProfiles));
            }

            applicationUser.Id = Guid.NewGuid().ToString();
            applicationUser.IsRegistered = false;
            applicationUser.SchoolId = applicationUser.SchoolId;
            applicationUser.UserName = applicationUser.Email;

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    _context.Users.Add(applicationUser);
                    await _context.SaveChangesAsync();

                    var role = await _context.Roles.FirstOrDefaultAsync(r => r.NormalizedName == "STUDENT");
                    if (role != null)
                    {
                        _context.UserRoles.Add(new IdentityUserRole<string>
                        {
                            UserId = applicationUser.Id,
                            RoleId = role.Id
                        });
                        await _context.SaveChangesAsync();
                    }

                    await transaction.CommitAsync();
                    TempData["Success"] = $"Sinh Viên {applicationUser.Email} đã được thêm thành công!";
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    TempData["Error"] = $"Xảy ra lỗi: {ex.Message}";
                }
            }

            return RedirectToAction(nameof(StudentProfiles));
        }
    //Delete Students
        [HttpPost]
        public async Task<IActionResult> DeleteStudent(string studentId)
        {
            var student = await _context.Users.FindAsync(studentId);
            if (student != null)
            {
                _context.Users.Remove(student);
                await _context.SaveChangesAsync();
                TempData["Success"] = $"Sinh Viên {student.Email} được xóa thành công!";
            }
            else
            {
                TempData["Error"] = "Không tìm thấy Sinh Viên!";
            }
            return RedirectToAction(nameof(StudentProfiles));
        }

// AdvisorProfiles actions
    //Render view
        [HttpGet]
        public async Task<IActionResult> AdvisorProfiles(int pageIndex = 1, int pageSize = 20)
        {
            ViewData["page"] = "AdvisorProfiles";
            var advisorsQuery = _context.ApplicationUsers
                .Where(u => !u.Email.EndsWith("@vanlanguni.vn"))
                .Select(user => new AdvisorProfileViewModel
                {
                    Id = user.Id,
                    Email = user.Email,
                    SchoolId = user.SchoolId,
                    FullName = user.FullName,
                    IsRegistered = user.IsRegistered,
                    LastLoginTime = user.LastLoginTime,
                    Role = "Advisor"
                });

            var advisors = await PaginatedList<AdvisorProfileViewModel>.CreateAsync(advisorsQuery.AsNoTracking(), pageIndex, pageSize);

            ViewBag.Warning = TempData["Warning"];
            ViewBag.Success = TempData["Success"];
            ViewBag.Error = TempData["Error"];

            return View(advisors);
        }
    //Add Advisors
        [HttpPost]
        public async Task<IActionResult> CreateAdvisorProfile(ApplicationUser applicationUser)
        {
            // Check if email already exists
            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == applicationUser.Email);
            if (existingUser != null)
            {
                TempData["Warning"] = "Email đã tồn tại trong hệ thống!";
                return RedirectToAction(nameof(AdvisorProfiles));
            }

            applicationUser.Id = Guid.NewGuid().ToString(); // Ensure unique ID
            applicationUser.IsRegistered = false; // Placeholder, not registered yet
            applicationUser.SchoolId = "PERSONNEL";
            applicationUser.UserName = applicationUser.Email;

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    // Save user to AspNetUsers
                    _context.Users.Add(applicationUser);
                    await _context.SaveChangesAsync(); // Ensure the user is saved first

                    // Assign Advisor Role
                    var role = await _context.Roles.FirstOrDefaultAsync(r => r.NormalizedName == "ADVISOR");
                    if (role != null)
                    {
                        _context.UserRoles.Add(new IdentityUserRole<string>
                        {
                            UserId = applicationUser.Id,
                            RoleId = role.Id
                        });
                        await _context.SaveChangesAsync();
                    }

                    await transaction.CommitAsync();
                    TempData["Success"] = $"Cố Vấn {applicationUser.Email} đã được thêm thành công!";
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    TempData["Error"] = $"Xảy ra lỗi khi thêm Cố Vấn: {ex.Message}";
                }
            }

            return RedirectToAction(nameof(AdvisorProfiles));
        }
    //Delete Advisor
        [HttpPost]
        public async Task<IActionResult> DeleteAdvisor(string advisorId)
        {
            var advisor = await _context.Users.FindAsync(advisorId);
            if (advisor != null)
            {
                _context.Users.Remove(advisor);
                await _context.SaveChangesAsync();
                TempData["Success"] = $"Cố vấn {advisor.Email} đã được xóa thành công!";
            }
            else
            {
                TempData["Error"] = "Không tìm thấy cố vấn!";
            }
            return RedirectToAction(nameof(AdvisorProfiles));
        }
    }
}
