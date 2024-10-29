using System.Linq; 
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

            var studentsQuery = from user in _context.ApplicationUsers
                join userRole in _context.UserRoles on user.Id equals userRole.UserId into userRoles
                from userRole in userRoles.DefaultIfEmpty()
                join role in _context.Roles on userRole.RoleId equals role.Id into roles
                from role in roles.DefaultIfEmpty()
                where user.Email.EndsWith("@vanlanguni.vn") && (role == null ||
                                                                (role.NormalizedName != "ADVISOR" &&
                                                                 role.NormalizedName != "FACULTY"))
                select new StudentProfileViewModel
                {
                    Id = user.Id,
                    Email = user.Email,
                    SchoolId = user.SchoolId,
                    FullName = user.FullName,
                    IsRegistered = user.IsRegistered,
                    LastLoginTime = user.LastLoginTime,
                    Role = role != null ? role.Name : "Chưa phân quyền"
                };

            var students =
                await PaginatedList<StudentProfileViewModel>.CreateAsync(studentsQuery.AsNoTracking(), pageIndex,
                    pageSize);
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
        public async Task<IActionResult> AdvisorProfiles(int pageIndex = 1, int pageSize = 20)
        {
            ViewData["page"] = "AdvisorProfiles";

            var advisorsQuery = from user in _context.ApplicationUsers
                join userRole in _context.UserRoles on user.Id equals userRole.UserId into userRoles
                from userRole in userRoles.DefaultIfEmpty()
                join role in _context.Roles on userRole.RoleId equals role.Id into roles
                from role in roles.DefaultIfEmpty()
                where !user.Email.EndsWith("@vanlanguni.vn") || (role != null &&
                                                                 (role.NormalizedName == "ADVISOR" ||
                                                                  role.NormalizedName == "FACULTY"))
                select new AdvisorProfileViewModel
                {
                    Id = user.Id,
                    Email = user.Email,
                    SchoolId = user.SchoolId,
                    FullName = user.FullName,
                    IsRegistered = user.IsRegistered,
                    LastLoginTime = user.LastLoginTime,
                    Role = role != null ? role.Name : "Chưa phân quyền"
                };

            var advisors =
                await PaginatedList<AdvisorProfileViewModel>.CreateAsync(advisorsQuery.AsNoTracking(), pageIndex,
                    pageSize);
            ViewBag.Warning = TempData["Warning"];
            ViewBag.Success = TempData["Success"];
            ViewBag.Error = TempData["Error"];
            return View(advisors);
        }

        //Add Advisors
        [HttpPost]
        public async Task<IActionResult> CreateAdvisorProfile(ApplicationUser applicationUser, string RoleId)
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
            applicationUser.SchoolId = applicationUser.SchoolId;
            applicationUser.UserName = applicationUser.Email;

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    // Save user to AspNetUsers
                    _context.Users.Add(applicationUser);
                    await _context.SaveChangesAsync(); // Ensure the user is saved first

                    // Assign Advisor Role
                    var role = await _context.Roles.FirstOrDefaultAsync(r => r.Id == RoleId);
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
                    TempData["Success"] = $"CVHT {applicationUser.Email} đã được thêm thành công!";
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    TempData["Error"] = $"Xảy ra lỗi khi thêm CVHT: {ex.Message}";
                }
            }

            return RedirectToAction(nameof(AdvisorProfiles));
        }

        //Edit Advisors
        [HttpPost]
        public async Task<IActionResult> UpdateAdvisorProfile(string Email, string RoleId, string UserId,
            string SchoolId)
        {

            var existingUser = await _context.ApplicationUsers.FirstOrDefaultAsync(u => u.Id == UserId);
            if (existingUser != null)
            {
                existingUser.SchoolId = SchoolId;

                using (var transaction = await _context.Database.BeginTransactionAsync())
                {
                    try
                    {
                        _context.Users.Update(existingUser);
                        await _context.SaveChangesAsync();

                        var currentRole =
                            await _context.UserRoles.FirstOrDefaultAsync(ur => ur.UserId == existingUser.Id);
                        if (currentRole != null)
                        {
                            _context.UserRoles.Remove(currentRole);
                            await _context.SaveChangesAsync();
                        }

                        var role = await _context.Roles.FirstOrDefaultAsync(r => r.Id == RoleId);
                        if (role != null)
                        {
                            _context.UserRoles.Add(new IdentityUserRole<string>
                            {
                                UserId = existingUser.Id,
                                RoleId = role.Id
                            });
                            await _context.SaveChangesAsync();
                        }

                        await transaction.CommitAsync();
                        TempData["Success"] = $"Cập nhật thông tin của {existingUser.Email} thành công!";
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        TempData["Error"] = $"Xảy ra lỗi khi cập nhật thông tin: {ex.Message}";
                    }
                }
            }
            else
            {
                TempData["Error"] = "Người dùng không tồn tại!";
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
                TempData["Success"] = $"CVHT {advisor.Email} đã được xóa thành công!";
            }
            else
            {
                TempData["Error"] = "Không tìm thấy CVHT!";
            }

            return RedirectToAction(nameof(AdvisorProfiles));
        }
// PersonalProfile actions
        //Render view

        public async Task<IActionResult> PersonalProfile()
        {
            ViewData["page"] = "PersonalProfile";

            
            var currentUserEmail = User.Identity.Name;

            
            var user = await _context.Users
                .OfType<ApplicationUser>() 
                .Where(u => u.Email == currentUserEmail)
                .Select(u => new ApplicationUser  
                {
                    Email = u.Email,
                    SchoolId = u.SchoolId,
                    FullName = u.FullName,
                    PhoneNumber = u.PhoneNumber,
                    DateOfBirth = u.DateOfBirth, 
                   
                })
                .FirstOrDefaultAsync();

            
            return View(user);
        }

        // Cập nhật thông tin người dùng
        
        [HttpPost]
        public async Task<IActionResult> UpdateProfile(ApplicationUser model)
        {
            
            var user = await _context.Users
                .OfType<ApplicationUser>()
                .FirstOrDefaultAsync(u => u.Email == model.Email);

            if (user == null)
            {
                TempData["Error"] = "Không tìm thấy người dùng.";
                return RedirectToAction("PersonalProfile");
            }

           
            user.FullName = model.FullName;
            user.SchoolId = model.SchoolId;
            user.PhoneNumber = model.PhoneNumber; 

            try
            {
               
                _context.Users.Update(user);
                await _context.SaveChangesAsync(); 

                TempData["Success"] = "Cập nhật thông tin thành công!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Có lỗi xảy ra: {ex.Message}";
            }

            return RedirectToAction("PersonalProfile");
        }

    }
}
