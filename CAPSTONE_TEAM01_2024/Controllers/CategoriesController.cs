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
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;
using System.Numerics;
using Xceed.Words.NET;
using Xceed.Document.NET;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using NuGet.Protocol.Plugins;
using static MimeKit.TextPart;
using System.Linq;


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

//SemesterReport actions
        //Render SemesterReport
        public async Task<IActionResult> EndSemesterReport(int pageIndex = 1, int pageSize = 20)
        {
            ViewData["Page"] =" EndSemesterReport";
            var semesterReportsQuery = _context.SemesterReports
                .Include(sp => sp.Class)
                .ThenInclude(c => c.Advisor)
                .OrderByDescending(sp => sp.CreationTimeReport)
                .AsQueryable();

            var schoolYears = await _context.AcademicPeriods
                .Select(sy => new SelectListItem { Value = sy.PeriodId.ToString(), Text = sy.PeriodName })
                .ToListAsync();

            var paginatedSemesterReports =
                await PaginatedList<SemesterReport>.CreateAsync(semesterReportsQuery, pageIndex, pageSize);

            var currentUser = await _context.ApplicationUsers.FirstOrDefaultAsync(u => u.Email == User.Identity.Name);

            var targetClasses = await _context.Classes
                .Where(c => c.AdvisorId == currentUser.Id) // Filter by current user's AdvisorId
                .Select(c => new SelectListItem
                {
                    Value = c.ClassId,
                    Text = c.ClassId
                })
                .ToListAsync();

            var detailReport = await _context.ReportDetails.ToListAsync();
            var viewModel = new SemesterReportViewModel
            {
                SemesterReports = paginatedSemesterReports,
                SchoolYears = schoolYears,
                Class = targetClasses,
                ReportDetails = detailReport
            };
            ViewBag.Warning = TempData["Warning"];
            ViewBag.Success = TempData["Success"];
            ViewBag.Error = TempData["Error"]; return View(viewModel);
        }   
        // Add SemesterReport
        [HttpPost] 
        public async Task<IActionResult> AddReport(SemesterReport report, IFormCollection form) 
        {
    // Setup data for SemesterReport
    var vietnamTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
    report.CreationTimeReport = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vietnamTimeZone);
    report.AdvisorName = User.Identity.Name;
    var period = await _context.AcademicPeriods.FirstOrDefaultAsync(p => p.PeriodId == report.PeriodId);
    report.PeriodName = period.PeriodName;
    report.StatusReport = "Nháp";

    // Add new SemesterReport
    var existingReport = await _context.SemesterReports.FirstOrDefaultAsync(rp =>
        rp.AcademicPeriod == report.AcademicPeriod &&
        rp.ClassId == report.ClassId &&
        rp.AdvisorName == report.AdvisorName);
    
    if (existingReport == null)
    {
        _context.SemesterReports.Add(report);
        await _context.SaveChangesAsync();

        // Setup data for ReportDetails
        var detailIds = new List<string>
        {
            "1_1", "2_1", "3_1", "3_2", "4_1", "4_2", 
            "5_1", "5_2", "5_3", "6_1", "7_1", "8_1", "8_2"
        };

        var details = detailIds.Select(id => new ReportDetail
        {
            ReportId = report.ReportId,
            CriterionId = int.Parse(id.Split('_')[0]),
            TaskReport = form[$"Task{id}"],
            HowToExecuteReport = form[$"HowToExecute{id}"],
            AttachmentReport = form[$"files{id}[]"]
                .Select(fileName => new AttachmentReport
                {
                    FileNames = fileName,
                    FilePath = Path.Combine("Uploads", fileName), // Điều chỉnh đường dẫn thực tế
                    FileDatas = Array.Empty<byte>(), // Nếu cần lưu dữ liệu thực của tệp
                    DetailReportlId = report.ReportId
                }).ToList(),
            // Add SelfAssessment and SelfRanking
            SelfAssessment = form["selfAssessment"], // Get personal assessment from form
            SelfRanking = Enum.TryParse<ReportDetail.Ranking>(form["ranking"], out var selfRank) ? selfRank : ReportDetail.Ranking.E, // Default to "E" if invalid
            // Add FacultyAssessment and FacultyRanking
            FacultyAssessment = form["FaculityAssessment"], // Get faculty assessment from form
            FacultyRanking = Enum.TryParse<ReportDetail.Ranking>(form["Faculityranking"], out var facultyRank) ? facultyRank : ReportDetail.Ranking.E // Default to "E" if invalid
        }).ToList();


        // Add new ReportDetails
        await _context.ReportDetails.AddRangeAsync(details);
        await _context.SaveChangesAsync();

        TempData["Success"] = "Thêm Báo Cáo thành công! Trạng thái hiện tại là nháp, vui lòng nộp Báo Cáo để thay đổi trạng thái!";
        return RedirectToAction("EndSemesterReport");
    }
    else
    {
        TempData["Warning"] = "Đã tồn tại Báo Cáo trong thời gian và lớp tương tự!";
        return RedirectToAction("EndSemesterReport");
    }
}
        
        
        
        //Delete Report
        [HttpPost]
        public async Task<IActionResult> DeleteReport(int reportId)
        {
            try
            {
                // Tìm báo cáo cần xóa trong cơ sở dữ liệu
                var report = await _context.SemesterReports.FindAsync(reportId);

                if (report == null)
                {
                    // Nếu không tìm thấy báo cáo, trả về thông báo lỗi
                    TempData["Error"] = "Báo cáo không tồn tại.";
                    return RedirectToAction("Index"); // Hoặc trang bạn muốn chuyển hướng
                }

                // Xóa báo cáo khỏi cơ sở dữ liệu
                _context.SemesterReports.Remove(report);
                await _context.SaveChangesAsync();

                // Thông báo xóa thành công
                TempData["Success"] = "Báo cáo đã được xóa thành công.";
                return RedirectToAction("EndSemesterReport"); // Hoặc trang bạn muốn chuyển hướng
            }
            catch (Exception ex)
            {
                // Nếu có lỗi, trả về thông báo lỗi
                TempData["Error"] = $"Đã xảy ra lỗi: {ex.Message}";
                return RedirectToAction("EndSemesterReport");
            }
        }
        
         [HttpPost]
        public async Task<IActionResult> EditReport(int reportId, int periodId, string reportType, string classId)
        {
            var reportToEdit = await _context.SemesterReports.FirstOrDefaultAsync(rp => rp.ReportId == reportId);
            if (reportToEdit == null)
            {
                TempData["Error"] = "Không tìm thấy báo cáo.";
                return RedirectToAction("EndSemesterReport");
            }

            var period = await _context.AcademicPeriods.FirstOrDefaultAsync(p => p.PeriodId == periodId);
            if (period == null)
            {
                TempData["Error"] = "Không tìm thấy học kỳ.";
                return RedirectToAction("EndSemesterReport");
            }

            if (string.IsNullOrWhiteSpace(reportType))
            {
                TempData["Error"] = "Loại báo cáo không được để trống.";
                return RedirectToAction("EndSemesterReport");
            }

            if (string.IsNullOrWhiteSpace(classId))
            {
                TempData["Error"] = "Mã lớp không được để trống.";
                return RedirectToAction("EndSemesterReport");
            }

            reportToEdit.PeriodName = period.PeriodName;
            reportToEdit.ReportType = reportType;
            reportToEdit.ClassId = classId;

            _context.SemesterReports.Update(reportToEdit);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Thông tin Báo Cáo cập nhật thành công";
            return RedirectToAction("EndSemesterReport");
        }

        //Submit Report
        [HttpPost]
        public async Task<IActionResult> SubmitReport(int reportId)
        {
            bool validated = await PlanValidation(reportId);
            if (validated == false)
            {
                TempData["Error"] = "Báo Cáo chưa đầy đủ thông tin, vui lòng điền đầy đủ thông tin trước khi thử lại!";
                return RedirectToAction("EndSemesterReport");
            }

            var reportToSubmit = await _context.SemesterReports.FirstOrDefaultAsync(rp => rp.ReportId == reportId);
            if (reportToSubmit.StatusReport == "Đã Nộp")
            {
                TempData["Warning"] = "Báo Cáo đã được nộp, vui lòng chỉnh lại chi tiết và tiến hành nộp lại!";
                return RedirectToAction("EndSemesterReport");
            }

            reportToSubmit.StatusReport = "Đã Nộp";
            _context.SemesterReports.Update(reportToSubmit);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Nộp Báo Cáo thành công";
            return RedirectToAction("EndSemesterReport");
        }
        
        //Duplicate Plan
        [HttpPost]
        public async Task<IActionResult> DuplicateReport(int reportId)
        {
            //Fetch the original SemesterReport
            var originalReport = _context.SemesterReports
                .Include(rp => rp.ReportDetails) // Load associated PlanDetails
                .FirstOrDefault(sp => sp.ReportId == reportId);
            var vietnamTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            //Duplicated Plan data
            var dupReport = new SemesterReport{ 
                ClassId = originalReport.ClassId,
                ReportType = originalReport.ReportType,
                PeriodId = originalReport.PeriodId,
                CreationTimeReport = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vietnamTimeZone),
                AdvisorName = User.Identity.Name,
                PeriodName = originalReport.PeriodName,
                StatusReport = "Nháp"
            };
            _context.SemesterReports.Add(dupReport);
            await _context.SaveChangesAsync();

            //Duplicate Plan details
            foreach (var detail in originalReport.ReportDetails)
            {
                var duplicatedDetail = new ReportDetail
                {
                    ReportId = dupReport.ReportId, 
                    TaskReport = detail.TaskReport,
                    CriterionId = detail.CriterionId,
                    HowToExecuteReport = detail.HowToExecuteReport,
                    AttachmentReport = detail.AttachmentReport,
                    SelfAssessment = detail.SelfAssessment,
                    FacultyAssessment = detail.FacultyAssessment,
                    SelfRanking = detail.SelfRanking,
                    FacultyRanking = detail.FacultyRanking,
                    
                };

                _context.ReportDetails.Add(duplicatedDetail);
            }
            await _context.SaveChangesAsync();

            TempData["Success"] = "Nhân bản Báo Cáo thành công!";
            return RedirectToAction("EndSemesterReport");
        }
        
        //Edit  Report Detail
        [HttpPost]
        public async Task<IActionResult> GetReportDetails(int reportId) 
        {
            if (reportId <= 0)
            {
                return BadRequest("Invalid Report ID.");
            }

            var reportDetails = await _context.ReportDetails
                .Include(r => r.AttachmentReport)
                .Where(rd => rd.ReportId == reportId)
                .Select(rd => new
                {
                    rd.ReportId,
                    rd.DetailReportlId,
                    rd.CriterionId,
                    rd.TaskReport,
                    rd.HowToExecuteReport,
                    rd.FacultyAssessment,
                    rd.FacultyRanking,
                    rd.SelfAssessment,
                    rd.SelfRanking,
                    Files = rd.AttachmentReport.Select(f => new
                    {
                        f.FileNames,
                        f.FilePath
                    }).ToList()
                })
                .ToListAsync();

            if (!reportDetails.Any())
            {
                return NotFound("No data found.");
            }

            return Json(reportDetails); // Chọn trả về dữ liệu JSON
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
                    range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
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

                return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    "Mẫu Excel DS Lớp.xlsx");
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
                                var existingClass =
                                    await _context.Classes.FirstOrDefaultAsync(c => c.ClassId == classId);
                                if (existingClass != null)
                                {
                                    failCount++;
                                    continue; // Skip existing classes
                                }

                                // Check if advisor exists and create if necessary
                                var advisorEntity =
                                    await _context.Users.FirstOrDefaultAsync(u => u.Email == advisorEmail);
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
                                    var advisorRole =
                                        await _context.Roles.FirstOrDefaultAsync(r => r.NormalizedName == "ADVISOR");
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
                                TempData["Error"] =
                                    $"Lỗi tại lớp {classId}: {ex.InnerException?.Message ?? ex.Message}";
                            }
                        }

                        await _context.SaveChangesAsync();
                    }
                }

                TempData["Success"] =
                    $"Nhập file Excel thành công! Số bản ghi thành công: {successCount}, số bản ghi thất bại: {failCount}.";
                return RedirectToAction("ClassList");
            }
            catch (Exception ex)
            {
                /*  TempData["Error"] = $"Đã xảy ra lỗi khi nhập file Excel. Đảm bảo các cột trong file excel được điền đầy đủ: {ex.InnerException?.Message ?? ex.Message}";*/
                TempData["Error"] = $"Đã xảy ra lỗi khi nhập file Excel. Đảm bảo các cột trong file excel được điền đầy đủ";
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
                    range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                    range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                }

                // Populate data rows
                for (int i = 0; i < classes.Count; i++)
                {
                    var row = i + 2; // Start from row 2
                    var classItem = classes[i];

                    worksheet.Cells[row, 1].Value = classItem.ClassId;
                    worksheet.Cells[row, 2].Value =
                        classItem.Advisor != null ? classItem.Advisor.Email : "Chưa bổ nhiệm";
                    worksheet.Cells[row, 3].Value = classItem.Term;
                    worksheet.Cells[row, 4].Value = classItem.Department;
                    worksheet.Cells[row, 5].Value = classItem.StudentCount;
                }

                // Auto fit columns
                worksheet.Cells.AutoFitColumns();

                var stream = new MemoryStream();
                package.SaveAs(stream);
                var content = stream.ToArray();

                return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    "DS Lớp.xlsx");
            }
        }


//StudentList actions
    //Render StudnetLít view
        public async Task<IActionResult> StudentList(int pageIndex = 1, int pageSize = 30)
        {
            ViewData["page"] = "StudentList";

            var studentsQuery = from user in _context.ApplicationUsers
                join userRole in _context.UserRoles on user.Id equals userRole.UserId into userRoles
                from userRole in userRoles.DefaultIfEmpty()
                join role in _context.Roles on userRole.RoleId equals role.Id into roles
                from role in roles.DefaultIfEmpty()
                where user.Email.EndsWith("@vanlanguni.vn") && (role == null ||
                                                                (role.NormalizedName != "ADVISOR" &&
                                                                 role.NormalizedName != "FACULTY"))
                select new StudentListViewModel
                {
                    Id = user.Id,
                    Email = user.Email,
                    SchoolId = user.SchoolId,
                    FullName = user.FullName,
                    DateOfBirth = user.DateOfBirth,
                    Status = user.Status,
                    ClassId = user.ClassId
                };

            var paginatedStudents =
                await PaginatedList<StudentListViewModel>.CreateAsync(studentsQuery.AsNoTracking(), pageIndex,
                    pageSize);

            ViewBag.Warning = TempData["Warning"];
            ViewBag.Success = TempData["Success"];
            ViewBag.Error = TempData["Error"];
            return View(paginatedStudents);
        }

    //Add Student
        [HttpPost]
        public async Task<IActionResult> AddStudent(StudentListViewModel model)
        {
            // Check if email already exists
            var existingUser = await _context.ApplicationUsers.FirstOrDefaultAsync(u => u.Email == model.Email);
            if (existingUser != null)
            {
                TempData["Warning"] = "Email đã tồn tại!";
                return RedirectToAction("StudentList");
            }

            // Check if ClassId exists
            var classExists = await _context.Classes.AnyAsync(c => c.ClassId == model.ClassId);
            if (!classExists)
            {
                TempData["Warning"] = "Mã Lớp không tồn tại!";
                return RedirectToAction("StudentList");
            }

            // Create new student
            var student = new ApplicationUser
            {
                UserName = model.SchoolId,
                Email = model.Email,
                SchoolId = model.SchoolId,
                FullName = model.FullName,
                DateOfBirth = model.DateOfBirth ?? DateTime.MinValue, // Handle null
                ClassId = model.ClassId,
                Status = model.Status,
                IsRegistered = false,
                EmailConfirmed = true // or false if you need confirmation
            };

            // Add student to database
            _context.ApplicationUsers.Add(student);

            try
            {
                await _context.SaveChangesAsync();

                // Assign student role
                var studentRole = await _context.Roles.FirstOrDefaultAsync(r => r.NormalizedName == "STUDENT");
                if (studentRole != null)
                {
                    _context.UserRoles.Add(new IdentityUserRole<string>
                    {
                        UserId = student.Id,
                        RoleId = studentRole.Id
                    });

                    await _context.SaveChangesAsync();
                }

                TempData["Success"] = "Sinh viên đã được thêm thành công!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Đã xảy ra lỗi khi thêm sinh viên: {ex.Message}";
            }

            return RedirectToAction("StudentList");
        }

    //Edit Student
        [HttpPost]
        public async Task<IActionResult> EditStudent(StudentListViewModel model)
        {
            // Check if student exists
            var student = await _context.ApplicationUsers.FirstOrDefaultAsync(u => u.Id == model.Id);
            if (student == null)
            {
                TempData["Warning"] = "Sinh viên không tồn tại!";
                return RedirectToAction("StudentList");
            }

            // Check if ClassId exists
            var classExists = await _context.Classes.AnyAsync(c => c.ClassId == model.ClassId);
            if (!classExists)
            {
                TempData["Warning"] = "Mã Lớp không tồn tại!";
                return RedirectToAction("StudentList");
            }

            // Update student information
            student.FullName = model.FullName;
            student.SchoolId = model.SchoolId;
            student.Email = model.Email;
            student.DateOfBirth = model.DateOfBirth ?? DateTime.MinValue; // Handle null
            student.ClassId = model.ClassId;
            student.Status = model.Status;

            try
            {
                // Save changes
                _context.ApplicationUsers.Update(student);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Thông tin sinh viên đã được cập nhật thành công!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Đã xảy ra lỗi khi cập nhật thông tin sinh viên: {ex.Message}";
            }

            return RedirectToAction("StudentList");
        }

    //Delete Student
        [HttpPost]
        public async Task<IActionResult> DeleteStudent(string id)
        {
            // Check if student exists
            var student = await _context.ApplicationUsers.FirstOrDefaultAsync(u => u.Id == id);
            if (student == null)
            {
                TempData["Error"] = "Sinh viên không tồn tại!";
                return RedirectToAction("StudentList");
            }

            try
            {
                // Remove student from database
                _context.ApplicationUsers.Remove(student);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Sinh viên đã được xóa thành công!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Đã xảy ra lỗi khi xóa sinh viên: {ex.Message}";
            }

            return RedirectToAction("StudentList");
        }

    //Download Excel Template for Student
        public IActionResult GenerateStudentTemplate()
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Mẫu Sinh Viên");

                // Define columns
                worksheet.Cells[1, 1].Value = "Mã Số SV";
                worksheet.Cells[1, 2].Value = "Email";
                worksheet.Cells[1, 3].Value = "Họ và Tên";
                worksheet.Cells[1, 4].Value = "Ngày tháng năm sinh";
                worksheet.Cells[1, 5].Value = "Mã Lớp";
                worksheet.Cells[1, 6].Value = "Tình Trạng";

                // Style the header
                using (var range = worksheet.Cells[1, 1, 1, 6])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                    range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                }

                // Sample data row (optional)
                worksheet.Cells[2, 1].Value = "2174802010856";
                worksheet.Cells[2, 2].Value = "duc.2174802010856@vanlanguni.vn";
                worksheet.Cells[2, 3].Value = "Nguyễn Bùi Minh Đức";
                worksheet.Cells[2, 4].Value = DateTime.Now.ToString("26/01/2003");
                worksheet.Cells[2, 5].Value = "71K27CNTT30";
                worksheet.Cells[2, 6].Value = "Đang học";

                // Auto fit columns
                worksheet.Cells.AutoFitColumns();

                var stream = new MemoryStream();
                package.SaveAs(stream);
                var content = stream.ToArray();

                return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    "Mẫu excel danh sách SV.xlsx");
            }
        }

    //Import Excel Template for Student
        [HttpPost]
        public async Task<IActionResult> ImportStudentExcel(IFormFile StudentExcelFile)
        {
            if (StudentExcelFile == null || StudentExcelFile.Length == 0)
            {
                TempData["Error"] = "Vui lòng chọn file Excel hợp lệ.";
                return RedirectToAction("StudentList");
            }

            try
            {
                int successCount = 0;
                int failCount = 0;
                var failureDetails = new List<string>();

                using (var stream = new MemoryStream())
                {
                    await StudentExcelFile.CopyToAsync(stream);
                    using (var package = new ExcelPackage(stream))
                    {
                        var worksheet = package.Workbook.Worksheets[0];
                        var rowCount = worksheet.Dimension.Rows;

                        for (int row = 2; row <= rowCount; row++)
                        {
                            var schoolId = worksheet.Cells[row, 1].Value?.ToString();
                            var email = worksheet.Cells[row, 2].Value?.ToString();
                            var fullName = worksheet.Cells[row, 3].Value?.ToString();
                            var dobString = worksheet.Cells[row, 4].Value?.ToString();
                            var classId = worksheet.Cells[row, 5].Value?.ToString();
                            var status = worksheet.Cells[row, 6].Value?.ToString();

                            DateTime dateOfBirth;
                            if (!DateTime.TryParseExact(dobString, new[] { "dd/MM/yyyy", "M/d/yyyy h:mm:ss tt" }, null,
                                    System.Globalization.DateTimeStyles.None, out dateOfBirth))
                            {
                                failCount++;
                                failureDetails.Add($"Dòng {row}: Ngày sinh không hợp lệ '{dobString}'.");
                                continue; // Skip rows with invalid date
                            }

                            try
                            {
                                // Check if email already exists
                                var existingUser =
                                    await _context.ApplicationUsers.FirstOrDefaultAsync(u => u.Email == email);
                                if (existingUser != null)
                                {
                                    failCount++;
                                    failureDetails.Add($"Dòng {row}: Email đã tồn tại '{email}'.");
                                    continue; // Skip existing users
                                }

                                // Check if ClassId exists
                                var classExists = await _context.Classes.AnyAsync(c => c.ClassId == classId);
                                if (!classExists)
                                {
                                    failCount++;
                                    failureDetails.Add($"Dòng {row}: Mã Lớp không tồn tại '{classId}'.");
                                    continue; // Skip rows with non-existing ClassId
                                }

                                // Create new student
                                var student = new ApplicationUser
                                {
                                    UserName = schoolId,
                                    Email = email,
                                    SchoolId = schoolId,
                                    FullName = fullName,
                                    DateOfBirth = dateOfBirth,
                                    ClassId = classId,
                                    Status = status,
                                    IsRegistered = false,
                                    EmailConfirmed = true // or false if you need confirmation
                                };

                                // Add student to database
                                _context.ApplicationUsers.Add(student);
                                await _context.SaveChangesAsync();

                                // Assign student role
                                var studentRole =
                                    await _context.Roles.FirstOrDefaultAsync(r => r.NormalizedName == "STUDENT");
                                if (studentRole != null)
                                {
                                    _context.UserRoles.Add(new IdentityUserRole<string>
                                    {
                                        UserId = student.Id,
                                        RoleId = studentRole.Id
                                    });

                                    await _context.SaveChangesAsync();
                                }

                                successCount++;
                            }
                            catch (Exception ex)
                            {
                                failCount++;
                                failureDetails.Add($"Dòng {row}: Lỗi hệ thống - {ex.Message}");
                            }
                        }
                    }
                }

                TempData["Success"] =
                    $"Nhập file Excel thành công! Số bản ghi thành công: {successCount}, số bản ghi thất bại: {failCount}.";
                if (failCount > 0)
                {
                    TempData["Error"] = $"Chi tiết lỗi: {string.Join("; ", failureDetails)}";
                }

                return RedirectToAction("StudentList");
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Đã xảy ra lỗi khi nhập file Excel: {ex.Message}";
                return RedirectToAction("StudentList");
            }
        }

    //Export Excel file for Student
        [HttpPost]
        public async Task<IActionResult> ExportStudentsByClass(string ClassId)
        {
            var students = await _context.ApplicationUsers
                .Where(u => u.ClassId == ClassId)
                .ToListAsync();

            if (!students.Any())
            {
                TempData["Error"] = "Không có sinh viên nào trong mã lớp này!";
                return RedirectToAction("StudentList");
            }

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Danh Sách Sinh Viên");

                // Define columns
                worksheet.Cells[1, 1].Value = "Mã Số SV";
                worksheet.Cells[1, 2].Value = "Email";
                worksheet.Cells[1, 3].Value = "Họ và Tên";
                worksheet.Cells[1, 4].Value = "Ngày tháng năm sinh";
                worksheet.Cells[1, 5].Value = "Mã Lớp";
                worksheet.Cells[1, 6].Value = "Tình Trạng";

                // Style the header
                using (var range = worksheet.Cells[1, 1, 1, 6])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                    range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                }

                // Populate data rows
                for (int i = 0; i < students.Count; i++)
                {
                    var row = i + 2; // Start from row 2
                    var student = students[i];

                    worksheet.Cells[row, 1].Value = student.SchoolId;
                    worksheet.Cells[row, 2].Value = student.Email;
                    worksheet.Cells[row, 3].Value = student.FullName;
                    worksheet.Cells[row, 4].Value = student.DateOfBirth.ToString("dd/MM/yyyy") ?? "N/A";
                    worksheet.Cells[row, 5].Value = student.ClassId;
                    worksheet.Cells[row, 6].Value = student.Status;
                }

                // Auto fit columns
                worksheet.Cells.AutoFitColumns();

                var stream = new MemoryStream();
                package.SaveAs(stream);
                var content = stream.ToArray();

                // Dynamically set the file name to include ClassId
                var fileName = $"Danh Sách SV lớp {ClassId}.xlsx";
                return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
        }


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
            var existingPeriod =
                await _context.AcademicPeriods.FirstOrDefaultAsync(p => p.PeriodName == model.PeriodName);
            if (existingPeriod != null)
            {
                TempData["Error"] = $"Năm học {model.PeriodName} đã tồn tại!";
                return RedirectToAction("SchoolYear");
            }

            _context.AcademicPeriods.Add(model);
            await _context.SaveChangesAsync();
            TempData["Success"] = $"Năm học {model.PeriodName} đã được thêm thành công!";
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
    //Render View
        public async Task<IActionResult> SemesterPlan(int pageIndex = 1, int pageSize = 20)
        {
            ViewData["page"] = "SemesterPlan";

            var semesterPlansQuery = _context.SemesterPlans
                .Include(sp => sp.Class)
                .ThenInclude(c => c.Advisor)
                .OrderByDescending(sp => sp.CreationTime)
                .AsQueryable();

            var schoolYears = await _context.AcademicPeriods
                .Select(sy => new SelectListItem { Value = sy.PeriodId.ToString(), Text = sy.PeriodName })
                .ToListAsync();

            var paginatedSemesterPlans =
                await PaginatedList<SemesterPlan>.CreateAsync(semesterPlansQuery, pageIndex, pageSize);

            var currentUser = await _context.ApplicationUsers.FirstOrDefaultAsync(u => u.Email == User.Identity.Name);

            var targetClasses = await _context.Classes
                .Where(c => c.AdvisorId == currentUser.Id) // Filter by current user's AdvisorId
                .Select(c => new SelectListItem
                {
                    Value = c.ClassId,
                    Text = c.ClassId
                })
                .ToListAsync();

            var detail = await _context.PlanDetails.ToListAsync();

            var viewModel = new SemesterPlanViewModel
            {
                SemesterPlans = paginatedSemesterPlans,
                SchoolYears = schoolYears,
                Class = targetClasses,
                Details = detail
            };

            ViewBag.Warning = TempData["Warning"];
            ViewBag.Success = TempData["Success"];
            ViewBag.Error = TempData["Error"];

            return View(viewModel);
        }

    //Add Plan
        [HttpPost]
        public async Task<IActionResult> AddPlan(SemesterPlan plan, IFormCollection form)
        {
            //Setup data for SemesterPlan
            var vietnamTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            plan.CreationTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vietnamTimeZone);
            plan.AdvisorName = User.Identity.Name;
            var period = await _context.AcademicPeriods.FirstOrDefaultAsync(p => p.PeriodId == plan.PeriodId);
            plan.PeriodName = period.PeriodName;
            plan.Status = "Nháp";

            //Add new SemesterPlan
            var exisitingPlan = await _context.SemesterPlans.FirstOrDefaultAsync(pl =>
                pl.AcademicPeriod == plan.AcademicPeriod &&
                pl.ClassId == plan.ClassId &&
                pl.AdvisorName == plan.AdvisorName);
            if (exisitingPlan == null)
            {
                _context.SemesterPlans.Add(plan);
                await _context.SaveChangesAsync();
                //Setup data for PlanDetail
                List<PlanDetail> Details = new List<PlanDetail>();
                //Detail 1
                var detail1 = new PlanDetail
                {
                    PlanId = plan.PlanId,
                    CriterionId = 1,
                    Task = form["Task1_1"],
                    HowToExecute = form["HowToExecute1_1"],
                    Quantity = form["Quantity1_1"],
                    TimeFrame = form["TimeFrame1_1"],
                    Notes = form["Notes1_1"]
                };
                Details.Add(detail1);
                //Detail 2
                var detail2 = new PlanDetail
                {
                    PlanId = plan.PlanId,
                    CriterionId = 1,
                    Task = form["Task1_2"],
                    HowToExecute = form["HowToExecute1_2"],
                    Quantity = form["Quantity1_2"],
                    TimeFrame = form["TimeFrame1_2"],
                    Notes = form["Notes1_2"]
                };
                Details.Add(detail2);
                //Detail 3
                var detail3 = new PlanDetail
                {
                    PlanId = plan.PlanId,
                    CriterionId = 1,
                    Task = form["Task1_3"],
                    HowToExecute = form["HowToExecute1_3"],
                    Quantity = form["Quantity1_3"],
                    TimeFrame = form["TimeFrame1_3"],
                    Notes = form["Notes1_3"]
                };
                Details.Add(detail3);
                //Detail 4
                var detail4 = new PlanDetail
                {
                    PlanId = plan.PlanId,
                    CriterionId = 2,
                    Task = form["Task2_1"],
                    HowToExecute = form["HowToExecute2_1"],
                    Quantity = form["Quantity2_1"],
                    TimeFrame = form["TimeFrame2_1"],
                    Notes = form["Notes2_1"]
                };
                Details.Add(detail4);
                //Detail 5
                var detail5 = new PlanDetail
                {
                    PlanId = plan.PlanId,
                    CriterionId = 2,
                    Task = form["Task2_2"],
                    HowToExecute = form["HowToExecute2_2"],
                    Quantity = form["Quantity2_2"],
                    TimeFrame = form["TimeFrame2_2"],
                    Notes = form["Notes2_2"]
                };
                Details.Add(detail5);
                //Detail 6
                var detail6 = new PlanDetail
                {
                    PlanId = plan.PlanId,
                    CriterionId = 3,
                    Task = form["Task3_1"],
                    HowToExecute = form["HowToExecute3_1"],
                    Quantity = form["Quantity3_1"],
                    TimeFrame = form["TimeFrame3_1"],
                    Notes = form["Notes3_1"]
                };
                Details.Add(detail6);
                //Detail 7
                var detail7 = new PlanDetail
                {
                    PlanId = plan.PlanId,
                    CriterionId = 3,
                    Task = form["Task3_2"],
                    HowToExecute = form["HowToExecute3_2"],
                    Quantity = form["Quantity3_2"],
                    TimeFrame = form["TimeFrame3_2"],
                    Notes = form["Notes3_1"]
                };
                Details.Add(detail7);
                //Detail 8
                var detail8 = new PlanDetail
                {
                    PlanId = plan.PlanId,
                    CriterionId = 4,
                    Task = form["Task4_1"],
                    HowToExecute = form["HowToExecute4_1"],
                    Quantity = form["Quantity4_1"],
                    TimeFrame = form["TimeFrame4_1"],
                    Notes = form["Notes4_1"]
                };
                Details.Add(detail8);
                //Detail 9
                var detail9 = new PlanDetail
                {
                    PlanId = plan.PlanId,
                    CriterionId = 4,
                    Task = form["Task4_2"],
                    HowToExecute = form["HowToExecute4_2"],
                    Quantity = form["Quantity4_2"],
                    TimeFrame = form["TimeFrame4_2"],
                    Notes = form["Notes4_1"]
                };
                Details.Add(detail9);
                //Detail 9
                var detail10 = new PlanDetail
                {
                    PlanId = plan.PlanId,
                    CriterionId = 5,
                    Task = form["Task5_1"],
                    HowToExecute = form["HowToExecute5_1"],
                    Quantity = form["Quantity5_1"],
                    TimeFrame = form["TimeFrame5_1"],
                    Notes = form["Notes5_1"]
                };
                Details.Add(detail10);

                //Add new PlanDetail
                await _context.PlanDetails.AddRangeAsync(Details);
                await _context.SaveChangesAsync();
                TempData["Success"] =
                    "Thêm kế hoạch thành công! Trạng thái hiện tại là nháp, vui lòng nộp kế hoạch để thay đổi trạng thái!";
                return RedirectToAction("SemesterPlan");
            }
            else
            {
                TempData["Warning"] = "Đã tồn tại kế hoạch trong thời gian và lớp tương tự!";
                return RedirectToAction("SemesterPlan");
            }
        }

    //Edit Plan Detail
        [HttpPost]
        public async Task<IActionResult> EditPlanDetail(IFormCollection form)
        {
            // Get the PlanIds value from the form collection
            var planIdsString = form["DetailIds"].ToString();
            // Split the string into an array of integers
            var detailIds = planIdsString.Split(',').Select(int.Parse).ToArray();

            List<PlanDetail> detailsToEdit = new List<PlanDetail>();
            //Detail 1
            var detail1 = await _context.PlanDetails.FirstOrDefaultAsync(pl => pl.DetailId == detailIds[0]);
            detail1.Task = form["EditTask1_1"];
            detail1.HowToExecute = form["EditHowToExecute1_1"];
            detail1.Quantity = form["EditQuantity1_1"];
            detail1.TimeFrame = form["EditTimeFrame1_1"];
            detail1.Notes = form["EditNotes1_1"];
            detailsToEdit.Add(detail1);
            //Detail 2
            var detail2 = await _context.PlanDetails.FirstOrDefaultAsync(pl => pl.DetailId == detailIds[1]);
            detail2.Task = form["EditTask1_2"];
            detail2.HowToExecute = form["EditHowToExecute1_2"];
            detail2.Quantity = form["EditQuantity1_2"];
            detail2.TimeFrame = form["EditTimeFrame1_2"];
            detail2.Notes = form["EditNotes1_2"];
            detailsToEdit.Add(detail2);
            //Detail 3
            var detail3 = await _context.PlanDetails.FirstOrDefaultAsync(pl => pl.DetailId == detailIds[2]);
            detail3.Task = form["EditTask1_3"];
            detail3.HowToExecute = form["EditHowToExecute1_3"];
            detail3.Quantity = form["EditQuantity1_3"];
            detail3.TimeFrame = form["EditTimeFrame1_3"];
            detail3.Notes = form["EditNotes1_3"];
            detailsToEdit.Add(detail3);
            //Detail 4
            var detail4 = await _context.PlanDetails.FirstOrDefaultAsync(pl => pl.DetailId == detailIds[3]);
            detail4.Task = form["EditTask2_1"];
            detail4.HowToExecute = form["EditHowToExecute2_1"];
            detail4.Quantity = form["EditQuantity2_1"];
            detail4.TimeFrame = form["EditTimeFrame2_1"];
            detail4.Notes = form["EditNotes2_1"];
            detailsToEdit.Add(detail4);
            //Detail 5
            var detail5 = await _context.PlanDetails.FirstOrDefaultAsync(pl => pl.DetailId == detailIds[4]);
            detail5.Task = form["EditTask2_2"];
            detail5.HowToExecute = form["EditHowToExecute2_2"];
            detail5.Quantity = form["EditQuantity2_2"];
            detail5.TimeFrame = form["EditTimeFrame2_2"];
            detail5.Notes = form["EditNotes2_2"];
            detailsToEdit.Add(detail5);
            //Detail 6
            var detail6 = await _context.PlanDetails.FirstOrDefaultAsync(pl => pl.DetailId == detailIds[5]);
            detail6.Task = form["EditTask3_1"];
            detail6.HowToExecute = form["EditHowToExecute3_1"];
            detail6.Quantity = form["EditQuantity3_1"];
            detail6.TimeFrame = form["EditTimeFrame3_1"];
            detail6.Notes = form["EditNotes3_1"];
            detailsToEdit.Add(detail6);
            //Detail 7
            var detail7 = await _context.PlanDetails.FirstOrDefaultAsync(pl => pl.DetailId == detailIds[6]);
            detail7.Task = form["EditTask3_2"];
            detail7.HowToExecute = form["EditHowToExecute3_2"];
            detail7.Quantity = form["EditQuantity3_2"];
            detail7.TimeFrame = form["EditTimeFrame3_2"];
            detail7.Notes = form["EditNotes3_1"];
            detailsToEdit.Add(detail7);
            //Detail 8
            var detail8 = await _context.PlanDetails.FirstOrDefaultAsync(pl => pl.DetailId == detailIds[7]);
            detail8.Task = form["EditTask4_1"];
            detail8.HowToExecute = form["EditHowToExecute4_1"];
            detail8.Quantity = form["EditQuantity4_1"];
            detail8.TimeFrame = form["EditTimeFrame4_1"];
            detail8.Notes = form["EditNotes4_1"];
            detailsToEdit.Add(detail8);
            //Detail 9
            var detail9 = await _context.PlanDetails.FirstOrDefaultAsync(pl => pl.DetailId == detailIds[8]);
            detail9.Task = form["EditTask4_2"];
            detail9.HowToExecute = form["EditHowToExecute4_2"];
            detail9.Quantity = form["EditQuantity4_2"];
            detail9.TimeFrame = form["EditTimeFrame4_2"];
            detail9.Notes = form["EditNotes4_1"];
            detailsToEdit.Add(detail9);
            //Detail 10
            var detail10 = await _context.PlanDetails.FirstOrDefaultAsync(pl => pl.DetailId == detailIds[9]);
            detail10.Task = form["EditTask5_1"];
            detail10.HowToExecute = form["EditHowToExecute5_1"];
            detail10.Quantity = form["EditQuantity5_1"];
            detail10.TimeFrame = form["EditTimeFrame5_1"];
            detail10.Notes = form["EditNotes5_1"];
            detailsToEdit.Add(detail10);

            var semesterPlan = await _context.SemesterPlans.FirstOrDefaultAsync(spl => spl.PlanId == detail1.PlanId);
            semesterPlan.Status = "Nháp";

            _context.SemesterPlans.Update(semesterPlan);
            _context.PlanDetails.UpdateRange(detailsToEdit);

            await _context.SaveChangesAsync();
            TempData["Success"] = "Chi tiết kế hoạch cập nhật thành công";
            return RedirectToAction("SemesterPlan");
        }

    //Edit Plan
        [HttpPost]
        public async Task<IActionResult> EditPlan(int planId, int periodId, string planType, string classId)
        {
            var planToEdit = await _context.SemesterPlans.FirstOrDefaultAsync(pl => pl.PlanId == planId);
            var period = await _context.AcademicPeriods.FirstOrDefaultAsync(p => p.PeriodId == periodId);
            //var vietnamTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            //planToEdit.CreationTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vietnamTimeZone);
            planToEdit.PeriodName = period.PeriodName;
            planToEdit.PlanType = planType;
            planToEdit.ClassId = classId;

            _context.SemesterPlans.Update(planToEdit);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Thông tin kế hoạch cập nhật thành công";
            return RedirectToAction("SemesterPlan");
        }

    //Check Plan Validity
        public async Task<bool> PlanValidation(int planId)
        {
            List<PlanDetail> details = await _context.PlanDetails.Where(pd => pd.PlanId == planId).ToListAsync();
            foreach (var dt in details)
            {
                if (string.IsNullOrWhiteSpace(dt.HowToExecute) || string.IsNullOrWhiteSpace(dt.Quantity) ||
                    string.IsNullOrWhiteSpace(dt.TimeFrame))
                {
                    return false;
                }
            }

            return true;
        }

    //Submit Plan
        [HttpPost]
        public async Task<IActionResult> SubmitPlan(int planId)
        {
            bool validated = await PlanValidation(planId);
            if (validated == false)
            {
                TempData["Error"] = "Kế hoạch chưa đầy đủ thông tin, vui lòng điền đầy đủ thông tin trước khi thử lại!";
                return RedirectToAction("SemesterPlan");
            }

            var planToSubmit = await _context.SemesterPlans.FirstOrDefaultAsync(pl => pl.PlanId == planId);
            if (planToSubmit.Status == "Đã Nộp")
            {
                TempData["Warning"] = "Kế hoạch đã được nộp, vui lòng chỉnh lại chi tiết và tiến hành nộp lại!";
                return RedirectToAction("SemesterPlan");
            }

            planToSubmit.Status = "Đã Nộp";
            _context.SemesterPlans.Update(planToSubmit);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Nộp kế hoạch thành công";
            return RedirectToAction("SemesterPlan");
        }

    //Delete Plan
        [HttpPost]
        public async Task<IActionResult> DeletePlan(int planId)
        {
            var targetPlan = await _context.SemesterPlans.FirstOrDefaultAsync(pl => pl.PlanId == planId);
            if (targetPlan == null)
            {
                TempData["Error"] = "Kế hoạch không tồn tại!";
                return RedirectToAction("SemesterPlan");
            }

            _context.SemesterPlans.Remove(targetPlan);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Kế hoạch đã được xóa thành công!";
            return RedirectToAction("SemesterPlan");
        }

    //Export plan
        [HttpPost]
        public async Task<IActionResult> ExportPlan(int planId)
        {
            var targetPlan = await _context.SemesterPlans.FirstOrDefaultAsync(pl => pl.PlanId == planId);
            var targetAdvisor =
                await _context.ApplicationUsers.FirstOrDefaultAsync(t => t.Email == targetPlan.AdvisorName);
            var details = await _context.PlanDetails.Where(dt => dt.PlanId == planId).ToListAsync();
            // Create a new document
            var doc = DocX.Create($"Kế Hoạch năm học {targetPlan.PeriodName}.docx");
            // Set page orientation to Landscape
            doc.PageLayout.Orientation = Orientation.Landscape;
            // Add content to the document
            doc.InsertParagraph("TRƯỜNG ĐẠI HỌC VĂN LANG").FontSize(14).Alignment = Alignment.left;
            doc.InsertParagraph("KHOA: CÔNG NGHỆ THÔNG TIN").FontSize(14).Bold().Alignment = Alignment.left;
            doc.InsertParagraph("KẾ HOẠCH HOẠT ĐỘNG CỐ VẤN HỌC TẬP").FontSize(16).Bold().Alignment = Alignment.center;
            doc.InsertParagraph($"NĂM HỌC: {targetPlan.PeriodName}").FontSize(14).Bold().Color(System.Drawing.Color.Red)
                .Alignment = Alignment.center;
            var p1 = doc.InsertParagraph();
            p1.Append("Mã giảng viên: ").FontSize(14);
            p1.Append(targetAdvisor.SchoolId).FontSize(14).Color(System.Drawing.Color.Red);
            p1.Alignment = Alignment.left;
            var p2 = doc.InsertParagraph();
            p2.Append("Họ và tên CVHT: ").FontSize(14);
            p2.Append(targetAdvisor.FullName).FontSize(14).Color(System.Drawing.Color.Red);
            p2.Alignment = Alignment.left;
            var p3 = doc.InsertParagraph();
            p3.Append("Lớp CVHT: ").FontSize(14);
            p3.Append(targetPlan.ClassId).FontSize(14).Color(System.Drawing.Color.Red);
            p3.Alignment = Alignment.left;
            // Create a table with 11 rows and 7 columns
            var table = doc.AddTable(11, 7);

            // Table Headers
            var headers = new[]
            {
                "STT", "TIÊU CHÍ", "MÔ TẢ CÔNG VIỆC", "CÁCH THỰC HIỆN",
                "ĐỊNH LƯỢNG", "THỜI GIAN", "GHI CHÚ (về chỉ số đạt theo quy định ở từng tiêu chí)"
            };
            for (int i = 0; i < headers.Length; i++)
            {
                table.Rows[0].Cells[i].Paragraphs[0].Append(headers[i]).FontSize(12).Bold().Alignment =
                    Alignment.center;
                table.Rows[0].Cells[i].SetBorder(TableCellBorderType.Top,
                    new Xceed.Document.NET.Border(BorderStyle.Tcbs_single, BorderSize.one, 0,
                        System.Drawing.Color.Black));
                table.Rows[0].Cells[i].SetBorder(TableCellBorderType.Bottom,
                    new Xceed.Document.NET.Border(BorderStyle.Tcbs_single, BorderSize.one, 0,
                        System.Drawing.Color.Black));
                table.Rows[0].Cells[i].SetBorder(TableCellBorderType.Left,
                    new Xceed.Document.NET.Border(BorderStyle.Tcbs_single, BorderSize.one, 0,
                        System.Drawing.Color.Black));
                table.Rows[0].Cells[i].SetBorder(TableCellBorderType.Right,
                    new Xceed.Document.NET.Border(BorderStyle.Tcbs_single, BorderSize.one, 0,
                        System.Drawing.Color.Black));
            }

            // Merge cells 
            //Row 1
            table.MergeCellsInColumn(0, 1, 3);
            table.Rows[1].Cells[0].Paragraphs[0].Append("1").FontSize(12).Bold().Alignment = Alignment.center;
            table.MergeCellsInColumn(1, 1, 3);
            table.Rows[1].Cells[1].Paragraphs[0].Append("Một số nội dung cơ bản của nhiệm vụ CVHT").FontSize(12).Bold()
                .Alignment = Alignment.center;
            //Row 2
            table.MergeCellsInColumn(0, 4, 5);
            table.Rows[4].Cells[0].Paragraphs[0].Append("2").FontSize(12).Bold().Alignment = Alignment.center;
            table.MergeCellsInColumn(1, 4, 5);
            table.Rows[4].Cells[1].Paragraphs[0]
                .Append("Hướng dẫn sinh viên lập và đăng ký kế hoạch học tập (KHHT), Đăng ký học phần (ĐKHP)")
                .FontSize(12).Bold().Alignment = Alignment.center;
            //Row 3
            table.MergeCellsInColumn(0, 6, 7);
            table.Rows[6].Cells[0].Paragraphs[0].Append("3").FontSize(12).Bold().Alignment = Alignment.center;
            table.MergeCellsInColumn(1, 6, 7);
            table.Rows[6].Cells[1].Paragraphs[0]
                .Append("Tư vấn phương pháp học tập, tổ chức cho sinh viên chia sẻ kinh nghiệm học tập").FontSize(12)
                .Bold().Alignment = Alignment.center;
            table.MergeCellsInColumn(6, 6, 7);
            //Row 4
            table.MergeCellsInColumn(0, 8, 9);
            table.Rows[8].Cells[0].Paragraphs[0].Append("4").FontSize(12).Bold().Alignment = Alignment.center;
            table.MergeCellsInColumn(1, 8, 9);
            table.Rows[8].Cells[1].Paragraphs[0].Append("Chăm sóc sinh viên diện đặc biệt").FontSize(12).Bold()
                .Alignment = Alignment.center;
            table.MergeCellsInColumn(6, 8, 9);
            //Row 5
            table.Rows[10].Cells[0].Paragraphs[0].Append("5").FontSize(12).Bold().Alignment = Alignment.center;
            table.Rows[10].Cells[1].Paragraphs[0].Append("Công tác phối hợp để thực hiện nhiệm vụ CVHT").FontSize(12)
                .Bold().Alignment = Alignment.center;

            // Apply borders and padding to all cells
            for (int row = 0; row < table.RowCount; row++)
            {
                for (int col = 0; col < table.ColumnCount; col++)
                {
                    table.Rows[row].Cells[col].SetBorder(TableCellBorderType.Top,
                        new Xceed.Document.NET.Border(BorderStyle.Tcbs_single, BorderSize.one, 0,
                            System.Drawing.Color.Black));
                    table.Rows[row].Cells[col].SetBorder(TableCellBorderType.Bottom,
                        new Xceed.Document.NET.Border(BorderStyle.Tcbs_single, BorderSize.one, 0,
                            System.Drawing.Color.Black));
                    table.Rows[row].Cells[col].SetBorder(TableCellBorderType.Left,
                        new Xceed.Document.NET.Border(BorderStyle.Tcbs_single, BorderSize.one, 0,
                            System.Drawing.Color.Black));
                    table.Rows[row].Cells[col].SetBorder(TableCellBorderType.Right,
                        new Xceed.Document.NET.Border(BorderStyle.Tcbs_single, BorderSize.one, 0,
                            System.Drawing.Color.Black));
                    table.Rows[row].Cells[col].MarginLeft = 10f;
                    table.Rows[row].Cells[col].MarginRight = 10f;
                    table.Rows[row].Cells[col].MarginTop = 5f;
                    table.Rows[row].Cells[col].MarginBottom = 5f;
                }
            }

            //Inject Data into table
            for (int i = 0; i < 10; i++)
            {
                table.Rows[i + 1].Cells[2].Paragraphs[0].Append(details[i].Task);
                table.Rows[i + 1].Cells[3].Paragraphs[0].Append(details[i].HowToExecute);
                table.Rows[i + 1].Cells[4].Paragraphs[0].Append(details[i].Quantity);
                table.Rows[i + 1].Cells[5].Paragraphs[0].Append(details[i].TimeFrame);
            }

            for (int i = 0; i < 10; i++)
            {
                if (i == 6 || i == 8)
                {
                    continue;
                }

                table.Rows[i + 1].Cells[6].Paragraphs[0].Append(details[i].Notes);
            }

            // Insert the table into the document
            doc.InsertParagraph().InsertTableAfterSelf(table);

            // Save the document to a MemoryStream
            using (var memoryStream = new MemoryStream())
            {
                doc.SaveAs(memoryStream);
                memoryStream.Position = 0;
                // Return the document as a file download
                return File(memoryStream.ToArray(),
                    "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                    $"Kế Hoạch năm học {targetPlan.PeriodName}.docx");
            }
        }

    //Duplicate Plan
        [HttpPost]
        public async Task<IActionResult> DuplicatePlan(int planId)
        {
            //Fetch the original SemesterPlan
            var originalPlan = _context.SemesterPlans
            .Include(sp => sp.PlanDetails) // Load associated PlanDetails
            .FirstOrDefault(sp => sp.PlanId == planId);
            var vietnamTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            //Duplicated Plan data
            var dupPlan = new SemesterPlan{ 
                ClassId = originalPlan.ClassId,
                PlanType = originalPlan.PlanType,
                PeriodId = originalPlan.PeriodId,
                CreationTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vietnamTimeZone),
                AdvisorName = User.Identity.Name,
                PeriodName = originalPlan.PeriodName,
                Status = "Nháp"
            };
            _context.SemesterPlans.Add(dupPlan);
            await _context.SaveChangesAsync();

            //Duplicate Plan details
            foreach (var detail in originalPlan.PlanDetails)
            {
                var duplicatedDetail = new PlanDetail
                {
                    PlanId = dupPlan.PlanId, // Associate with the new SemesterPlan
                    Task = detail.Task,
                    CriterionId = detail.CriterionId,
                    HowToExecute = detail.HowToExecute,
                    Quantity = detail.Quantity,
                    TimeFrame = detail.TimeFrame,
                    Notes = detail.Notes
                    // Copy other fields as necessary
                };

                _context.PlanDetails.Add(duplicatedDetail);
            }
            await _context.SaveChangesAsync();

            TempData["Success"] = "Nhân bản kế hoạch thành công!";
            return RedirectToAction("SemesterPlan");
        }

//SemesterPlanDetail actions
    //Render View 
        public IActionResult SemesterPlanDetail()
        {
            ViewData["page"] = "SemesterPlanDetail";
            return View();
        }

//Receive Mail actions
    //Render View 
        public async Task<IActionResult> ReceiveEmail(int pageIndex = 1, int pageSize = 20)
        {
            ViewData["page"] = "ReceiveEmail";
            var currentUser = await _context.ApplicationUsers.FirstOrDefaultAsync(us => us.UserName == User.Identity.Name);
            var emails = _context.Emails
                    .Include(e => e.Recipients)
                    .ThenInclude(r => r.User)
                    .Where(e => e.Recipients.Any(r => r.User.UserName == User.Identity.Name))
                    .Select(e => new Email
                    {
                        EmailId = e.EmailId,
                        Sender = e.Sender,
                        Recipients = e.Recipients,
                        Subject = e.Subject,
                        SentDate = e.SentDate,
                        Status = e.Status,
                        SenderId = e.SenderId,
                        Content = e.Content,
                        Thread = e.Thread,
                        ThreadId = e.ThreadId,
                        Attachments = e.Attachments
                    }).AsQueryable();

            var paginatedEmails =
                await PaginatedList<Email>.CreateAsync(emails, pageIndex, pageSize);

            var viewmodel = new EmailViewModel
            {
                Emails = paginatedEmails
            };
            ViewBag.Warning = TempData["Warning"];
            ViewBag.Success = TempData["Success"];
            ViewBag.Error = TempData["Error"];

            return View(viewmodel);
        }
//Send Mail actions
    //Render View
        public async Task<IActionResult> SentEmail(int pageIndex = 1, int pageSize = 20)
        {
            ViewData["page"] = "SentEmail";
            var currentUser = await _context.ApplicationUsers.FirstOrDefaultAsync(us => us.UserName == User.Identity.Name);
            var emails = _context.Emails
                    .Include(e => e.Recipients)
                    .ThenInclude(r => r.User)
                    .Where(e => e.SenderId == currentUser.Id)
                    .Select(e => new Email
                    {
                        EmailId = e.EmailId,
                        Sender = e.Sender,
                        Recipients = e.Recipients,
                        Subject = e.Subject,
                        SentDate = e.SentDate,
                        Status = e.Status,
                        SenderId = e.SenderId,
                        Content = e.Content,
                        Thread = e.Thread,
                        ThreadId = e.ThreadId,
                        Attachments = e.Attachments
                    }).AsQueryable();

            var paginatedEmails =
                await PaginatedList<Email>.CreateAsync(emails, pageIndex, pageSize);

            var viewmodel = new EmailViewModel
            {
                Emails = paginatedEmails
            };
            ViewBag.Warning = TempData["Warning"];
            ViewBag.Success = TempData["Success"];
            ViewBag.Error = TempData["Error"];

            // Truyền emails vào view
            return View(viewmodel);
        }
    //Send Mail
        [HttpPost]
        public async Task<IActionResult> SentEmail(IFormCollection form, List<IFormFile> emailAttachment)
        {
            var vietnamTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            if (vietnamTimeZone == null)
            {
                return BadRequest("Vietnam time zone not found");
            }

            var currentUserId = _context.ApplicationUsers
                .Where(u => u.Email == User.Identity.Name)
                .FirstOrDefault()?.Id;

            if (currentUserId == null)
            {
                return BadRequest("User not found.");
            }

            // Create the email once
            Email thisMail = new Email
            {
                SenderId = currentUserId,
                Subject = form["emailSubject"],
                Content = form["emailContent"],
                SentDate = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vietnamTimeZone)
            };

            _context.Emails.Add(thisMail);
            await _context.SaveChangesAsync(); // Save email once

            // Thread Actions
            if (int.TryParse(form["threadId"], out int threadId))
            {
                thisMail.ThreadId = threadId;
            }
            else
            {
                EmailThread newThread = new EmailThread
                {
                    Subject = form["emailSubject"]
                };
                _context.EmailThreads.Add(newThread); // Add thread to DbContext
                await _context.SaveChangesAsync(); // Save thread into the database
                thisMail.ThreadId = newThread.ThreadId;
            }

            // Email Attachment Actions
            foreach (var attachment in emailAttachment)
            {
                if (attachment.Length > 0)
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        await attachment.CopyToAsync(memoryStream);

                        var theseAttachments = new EmailAttachment
                        {
                            FileName = attachment.FileName,
                            FileData = memoryStream.ToArray(),
                            EmailId = thisMail.EmailId
                        };

                        _context.EmailAttachments.Add(theseAttachments);
                    }
                }
            }
            string Tos = form["recipientTo"];
            string[] TosEmails = Tos.Split(new[] { ';', ',' }, StringSplitOptions.RemoveEmptyEntries);
            string Ccs = form["recipientCc"];
            string[] CcsEmails = Ccs.Split(new[] { ';', ',' }, StringSplitOptions.RemoveEmptyEntries);
            string Bccs = form["recipientBcc"];
            string[] BccsEmails = Bccs.Split(new[] { ';', ',' }, StringSplitOptions.RemoveEmptyEntries);
            // Add recipients for To, Cc, Bcc
            await AddRecipients(TosEmails, thisMail.EmailId, "To");
            await AddRecipients(CcsEmails, thisMail.EmailId, "Cc");
            await AddRecipients(BccsEmails, thisMail.EmailId, "Bcc");

            await _context.SaveChangesAsync(); // Save all changes at once

            TempData["Success"] = "Gửi Mail thành công!";
            return RedirectToAction("SentEmail");
        }

        // Helper function to add recipients
        private async Task AddRecipients(string[] emails, int emailId, string recipientType)
        {
            foreach (string userEmail in emails)
            {
                var recipientUser = _context.ApplicationUsers.FirstOrDefault(u => u.Email == userEmail);
                if (recipientUser != null)
                {
                    EmailRecipient thisOne = new EmailRecipient
                    {
                        UserId = recipientUser.Id,
                        EmailId = emailId,
                        RecipientType = recipientType
                    };

                    _context.EmailRecipients.Add(thisOne);
                }
            }
        }

    //Delete Email
        [HttpPost]
        public async Task<IActionResult> DeleteEmail(int emailId)
        {
            var emailToDelete = await _context.Emails.FindAsync(emailId);
            if (emailToDelete != null)
            {
                _context.Emails.Remove(emailToDelete);
                await _context.SaveChangesAsync();
                TempData["Success"] = $"Email đã được xóa thành công!";
            }
            else
            {
                TempData["Error"] = "Không tìm thấy email để xóa.";
            }
            return RedirectToAction("SentEmail");
        }
    //View Detail 
        [HttpGet]
        public async Task<IActionResult> GetEmailDetails(int emailId)
        {
            ViewData["page"] = "SentEmail";
            Console.WriteLine($"EmailId được truyền: {emailId}");

            var email = await _context.Emails
                .Include(e => e.Sender)
                .Include(e => e.Recipients)
                    .ThenInclude(r => r.User)
                .Include(e => e.Attachments)
                .FirstOrDefaultAsync(e => e.EmailId == emailId);

            if (email == null)
            {
                return NotFound("Email not found");
            }

            // Group recipients by type (To, Cc, Bcc)
            var recipientsByType = email.Recipients
                .GroupBy(r => r.RecipientType) // Assuming `r.Type` is the recipient type (To, Cc, Bcc)
                .ToDictionary(
                    group => group.Key, // Group key is the type (To, Cc, Bcc)
                    group => group.Select(r => r.User.Email).ToList() // List of emails per group
                );

            return Json(new
            {
                Recipients = new
                {
                    To = recipientsByType.ContainsKey("To") ? recipientsByType["To"] : new List<string>(),
                    Cc = recipientsByType.ContainsKey("Cc") ? recipientsByType["Cc"] : new List<string>(),
                    Bcc = recipientsByType.ContainsKey("Bcc") ? recipientsByType["Bcc"] : new List<string>()
                },
                Subject = email.Subject,
                Content = email.Content,
                SentDate = email.SentDate,
                Attachments = email.Attachments.Select(a => new { a.FileName }).ToList(),
                Sender = email.Sender.UserName
            });
        }

    }
}
   


