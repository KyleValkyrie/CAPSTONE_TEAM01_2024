using CAPSTONE_TEAM01_2024.Models;
using CAPSTONE_TEAM01_2024.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.IO;
using Microsoft.Extensions.Logging;
using System.Drawing.Printing;
using CAPSTONE_TEAM01_2024.ViewModels;
using Microsoft.AspNetCore.Identity;
using System.Data;
using System.Drawing;

namespace CAPSTONE_TEAM01_2024.Controllers
{
    public class CategoriesController : Controller
    {
// Database Context
        private readonly ApplicationDbContext _context;
		public CategoriesController(ApplicationDbContext context)
        {
            _context = context;
		}
//EndSemesterReport actions
        public IActionResult EndSemesterReport()
        {
            ViewData["page"] = "EndSemesterReport";
            return View();
        }
//ClassList actions
    //Render ClassList
        public async Task<IActionResult> ClassList(int pageIndex = 1, int pageSize = 20)
        {
            ViewData["page"] = "ClassList";
            var classes = await _context.Classes
                .Include(c => c.Advisor)
                .Include(c => c.Students)
                .Select(c => new Class
                {
                    ClassId = c.ClassId,
                    Term = c.Term,
                    Department = c.Department,
                    AdvisorId = c.AdvisorId,
                    Advisor = c.Advisor ?? new ApplicationUser { Email = "N/A", FullName = "Chưa bổ nhiệm" },
                    StudentCount = c.StudentCount,
                    Students = c.Students
                })
                .ToListAsync();

            var paginatedClasses = PaginatedList<Class>.Create(classes, pageIndex, pageSize);

            var viewModel = new ClassListViewModel
            {
                Classes = paginatedClasses
            };
            ViewBag.Warning = TempData["Warning"];
            ViewBag.Success = TempData["Success"];
            ViewBag.Error = TempData["Error"];
            return View(viewModel);
        }
    // Add Class
        [HttpPost]
        public async Task<IActionResult> CreateClass(Class model, string AdvisorEmail)
        {
            // Check if the advisor exists using DbContext
            var advisor = await _context.Users.FirstOrDefaultAsync(u => u.Email == AdvisorEmail);
            if (advisor == null)
            {
                // If the advisor doesn't exist, create a new ApplicationUser
                advisor = new ApplicationUser
                {
                    UserName = AdvisorEmail,
                    Email = AdvisorEmail,
                    SchoolId = "ADVISOR"
                };
                _context.Users.Add(advisor);
                await _context.SaveChangesAsync();

                // Assign advisor role to the newly created advisor
                var advisorRole = await _context.Roles.FirstOrDefaultAsync(r => r.NormalizedName == "ADVISOR");
                _context.UserRoles.Add(new IdentityUserRole<string>
                {
                    UserId = advisor.Id,
                    RoleId = advisorRole.Id
                });
                await _context.SaveChangesAsync();
            }

            // Check if the class already exists using DbContext
            var existingClass = await _context.Classes.FirstOrDefaultAsync(c => c.ClassId == model.ClassId);
            if (existingClass != null)
            {
                // If the class already exists, throw a warning
                TempData["Warning"] = $"Lớp {model.ClassId} đã tồn tại!";
                return RedirectToAction("ClassList");
            }

            // Assign the AdvisorId to the Class model
            model.AdvisorId = advisor.Id;

            // Add the class to the database
            _context.Classes.Add(model);
            await _context.SaveChangesAsync();
            TempData["Success"] = $"Lớp {model.ClassId} thuộc cố vấn {AdvisorEmail} phụ trách đã được thêm thành công!";
            return RedirectToAction("ClassList");
        }
    // Edit Class
        [HttpPost]
        public async Task<IActionResult> EditClass(Class model, string AdvisorEmail)
        {
            // Find the existing class
            var classToUpdate = await _context.Classes
                .Include(c => c.Advisor)
                .FirstOrDefaultAsync(c => c.ClassId == model.ClassId);

            if (classToUpdate == null)
            {
                TempData["Error"] = "Lớp không tồn tại!";
                return RedirectToAction("ClassList");
            }

            // Check if the advisor email has changed or AdvisorId is null
            if (string.IsNullOrEmpty(classToUpdate.AdvisorId) || classToUpdate.Advisor.Email != AdvisorEmail)
            {
                // Find or create the new advisor
                var advisor = await _context.Users.FirstOrDefaultAsync(u => u.Email == AdvisorEmail);
                if (advisor == null)
                {
                    advisor = new ApplicationUser
                    {
                        UserName = AdvisorEmail,
                        Email = AdvisorEmail,
                        SchoolId = "ADVISOR"
                    };
                    _context.Users.Add(advisor);
                    await _context.SaveChangesAsync();

                    // Assign advisor role to the newly created advisor
                    var advisorRole = await _context.Roles.FirstOrDefaultAsync(r => r.NormalizedName == "ADVISOR");
                    _context.UserRoles.Add(new IdentityUserRole<string>
                    {
                        UserId = advisor.Id,
                        RoleId = advisorRole.Id
                    });
                    await _context.SaveChangesAsync();
                }
                classToUpdate.AdvisorId = advisor.Id;
            }

            // Update other class details
            classToUpdate.Term = model.Term;
            classToUpdate.Department = model.Department;
            classToUpdate.StudentCount = model.StudentCount;

            // Save changes
            _context.Classes.Update(classToUpdate);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Lớp {model.ClassId} đã được cập nhật thành công!";
            return RedirectToAction("ClassList");
        }
    //Delete Class
        [HttpPost]
        public async Task<IActionResult> DeleteClass(string id)
        {
            var classToDelete = await _context.Classes.FindAsync(id);
            if (classToDelete != null)
            {
                _context.Classes.Remove(classToDelete);
                await _context.SaveChangesAsync();
                TempData["Success"] = $"Lớp {classToDelete.ClassId} đã được xóa thành công!";
            }
            else
            {
                TempData["Error"] = "Đã xảy ra lỗi khi xóa lớp.";
            }
            return RedirectToAction("ClassList");
        }
    //Download Excel Template for Class
        public IActionResult GenerateClassTemplate()
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Mẫu Lớp");

                // Define the columns based on the provided fields
                worksheet.Cells[1, 1].Value = "Mã Lớp";
                worksheet.Cells[1, 2].Value = "Cố Vấn";
                worksheet.Cells[1, 3].Value = "Niên Khóa";
                worksheet.Cells[1, 4].Value = "Ngành";
                worksheet.Cells[1, 5].Value = "Số Lượng SV";

                // Style the header
                using (var range = worksheet.Cells[1, 1, 1, 5])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(Color.LightGray);
                    range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                }

                // Sample data row (optional)
                worksheet.Cells[2, 1].Value = "71K27CNTT01";
                worksheet.Cells[2, 2].Value = "nam.197pm09478@vanlanguni.vn";
                worksheet.Cells[2, 3].Value = "2022-2025";
                worksheet.Cells[2, 4].Value = "7480201 - Công nghệ Thông tin (CTTC)";
                worksheet.Cells[2, 5].Value = "30";

                // Auto fit columns
                worksheet.Cells.AutoFitColumns();

                var stream = new MemoryStream();
                package.SaveAs(stream);
                var content = stream.ToArray();

                return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Mẫu Excel DS Lớp.xlsx");
            }
        }
    //Import Excel Template for Class
        [HttpPost]
        public async Task<IActionResult> ImportClassExcel(IFormFile ClassExcelFile)
        {
            if (ClassExcelFile == null || ClassExcelFile.Length == 0)
            {
                TempData["Error"] = "Vui lòng chọn file Excel hợp lệ.";
                return RedirectToAction("ClassList");
            }

            try
            {
                int successCount = 0;
                int failCount = 0;

                using (var stream = new MemoryStream())
                {
                    await ClassExcelFile.CopyToAsync(stream);
                    using (var package = new ExcelPackage(stream))
                    {
                        var worksheet = package.Workbook.Worksheets[0];
                        var rowCount = worksheet.Dimension.Rows;

                        for (int row = 2; row <= rowCount; row++)
                        {
                            var classId = worksheet.Cells[row, 1].Value?.ToString();
                            var advisorEmail = worksheet.Cells[row, 2].Value?.ToString();
                            var term = worksheet.Cells[row, 3].Value?.ToString();
                            var department = worksheet.Cells[row, 4].Value?.ToString();
                            var studentCount = worksheet.Cells[row, 5].Value?.ToString();

                            try
                            {
                                // Check if class already exists
                                var existingClass = await _context.Classes.FirstOrDefaultAsync(c => c.ClassId == classId);
                                if (existingClass != null)
                                {
                                    failCount++;
                                    continue; // Skip existing classes
                                }

                                // Check if advisor exists and create if necessary
                                var advisorEntity = await _context.Users.FirstOrDefaultAsync(u => u.Email == advisorEmail);
                                if (advisorEntity == null)
                                {
                                    advisorEntity = new ApplicationUser
                                    {
                                        UserName = advisorEmail,
                                        Email = advisorEmail,
                                        SchoolId = "ADVISOR"
                                    };
                                    _context.Users.Add(advisorEntity);
                                    await _context.SaveChangesAsync();

                                    // Assign advisor role
                                    var advisorRole = await _context.Roles.FirstOrDefaultAsync(r => r.NormalizedName == "ADVISOR");
                                    _context.UserRoles.Add(new IdentityUserRole<string>
                                    {
                                        UserId = advisorEntity.Id,
                                        RoleId = advisorRole.Id
                                    });
                                    await _context.SaveChangesAsync();
                                }

                                // Create new class
                                var newClass = new Class
                                {
                                    ClassId = classId,
                                    AdvisorId = advisorEntity.Id,
                                    Term = term,
                                    Department = department,
                                    StudentCount = int.Parse(studentCount)
                                };

                                _context.Classes.Add(newClass);
                                successCount++;
                            }
                            catch (Exception ex)
                            {
                                failCount++;
                                TempData["Error"] = $"Lỗi tại lớp {classId}: {ex.InnerException?.Message ?? ex.Message}";
                            }
                        }

                        await _context.SaveChangesAsync();
                    }
                }

                TempData["Success"] = $"Nhập file Excel thành công! Số bản ghi thành công: {successCount}, số bản ghi thất bại: {failCount}.";
                return RedirectToAction("ClassList");
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Đã xảy ra lỗi khi nhập file Excel: {ex.InnerException?.Message ?? ex.Message}";
                return RedirectToAction("ClassList");
            }
        }
    //Export Excel file for Class
        public async Task<IActionResult> ExportClassToExcel()
        {
            var classes = await _context.Classes.Include(c => c.Advisor).ToListAsync();

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Danh Sách Lớp");

                // Define columns
                worksheet.Cells[1, 1].Value = "Mã Lớp";
                worksheet.Cells[1, 2].Value = "Cố Vấn";
                worksheet.Cells[1, 3].Value = "Niên Khóa";
                worksheet.Cells[1, 4].Value = "Ngành";
                worksheet.Cells[1, 5].Value = "Số Lượng SV";

                // Style the header
                using (var range = worksheet.Cells[1, 1, 1, 5])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(Color.LightGray);
                    range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                }

                // Populate data rows
                for (int i = 0; i < classes.Count; i++)
                {
                    var row = i + 2; // Start from row 2
                    var classItem = classes[i];

                    worksheet.Cells[row, 1].Value = classItem.ClassId;
                    worksheet.Cells[row, 2].Value = classItem.Advisor != null ? classItem.Advisor.Email : "Chưa bổ nhiệm";
                    worksheet.Cells[row, 3].Value = classItem.Term;
                    worksheet.Cells[row, 4].Value = classItem.Department;
                    worksheet.Cells[row, 5].Value = classItem.StudentCount;
                }

                // Auto fit columns
                worksheet.Cells.AutoFitColumns();

                var stream = new MemoryStream();
                package.SaveAs(stream);
                var content = stream.ToArray();

                return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "DS Lớp.xlsx");
            }
        }

//StudentList actions

//SchoolYear actions
    //Render SchoolYear view
        public async Task<IActionResult> SchoolYear(int pageIndex = 1, int pageSize = 20)
		{
			ViewData["page"] = "SchoolYear";
            var periods = _context.AcademicPeriods.AsQueryable();
            var paginatedPeriods = await PaginatedList<AcademicPeriod>.CreateAsync(periods, pageIndex, pageSize);

            var viewModel = new SchoolYearViewModel
            {
                AcademicPeriods = paginatedPeriods
            };

            ViewBag.Warning = TempData["Warning"];
            ViewBag.Success = TempData["Success"];
            ViewBag.Error = TempData["Error"];
            return View(viewModel);
        }
    //Add School Year
        [HttpPost]
        public async Task<IActionResult> CreatePeriod(AcademicPeriod model)
        {
            // Check if the period name already exists
            var existingPeriod = await _context.AcademicPeriods.FirstOrDefaultAsync(p => p.PeriodName == model.PeriodName);
            if (existingPeriod != null)
            {
                TempData["Error"] = $"Năm học {model.PeriodName} đã tồn tại!";
                return RedirectToAction("SchoolYear");
            }

            if (ModelState.IsValid)
            {
                _context.AcademicPeriods.Add(model);
                await _context.SaveChangesAsync();
                TempData["Success"] = $"Năm học {model.PeriodName} đã được thêm thành công!";
                return RedirectToAction("SchoolYear");
            }

            TempData["Error"] = "Đã xảy ra lỗi khi thêm năm học.";
            return RedirectToAction("SchoolYear");
        }
    //Edit School Year
        [HttpPost]
        public async Task<IActionResult> UpdatePeriod(AcademicPeriod model)
        {
            // Check if the period name already exists but is not the current period being updated
            var existingPeriod = await _context.AcademicPeriods
                .FirstOrDefaultAsync(p => p.PeriodName == model.PeriodName && p.PeriodId != model.PeriodId);
            if (existingPeriod != null)
            {
                TempData["Error"] = $"Năm học {model.PeriodName} đã tồn tại!";
                return RedirectToAction("SchoolYear");
            }

            var periodToUpdate = await _context.AcademicPeriods.FirstOrDefaultAsync(p => p.PeriodId == model.PeriodId);
            if (periodToUpdate == null)
            {
                TempData["Error"] = "Năm học không tồn tại!";
                return RedirectToAction("SchoolYear");
            }

            string oldPeriodName = periodToUpdate.PeriodName;

            if (ModelState.IsValid)
            {
                periodToUpdate.PeriodStart = model.PeriodStart;
                periodToUpdate.PeriodEnd = model.PeriodEnd;
                periodToUpdate.PeriodName = model.PeriodName;

                _context.AcademicPeriods.Update(periodToUpdate);
                await _context.SaveChangesAsync();

                TempData["Success"] = $"Năm học {oldPeriodName} đã được cập nhật thành {model.PeriodName}!";
                return RedirectToAction("SchoolYear");
            }

            TempData["Error"] = "Đã xảy ra lỗi khi cập nhật năm học.";
            return RedirectToAction("SchoolYear");
        }
    //Delete School Year
        [HttpPost]
        public async Task<IActionResult> DeletePeriod(int periodId)
        {
            var periodToDelete = await _context.AcademicPeriods.FirstOrDefaultAsync(p => p.PeriodId == periodId);
            if (periodToDelete == null)
            {
                TempData["Error"] = "Năm học không tồn tại!";
                return RedirectToAction("SchoolYear");
            }

            _context.AcademicPeriods.Remove(periodToDelete);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Năm học {periodToDelete.PeriodName} đã được xóa thành công!";
            return RedirectToAction("SchoolYear");
        }

//SemesterPlan actions
        public IActionResult SemesterPlan()
		{
			ViewData["page"] = "SemesterPlan";
			return View();
		}
	}
}


