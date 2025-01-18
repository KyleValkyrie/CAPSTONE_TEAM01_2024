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
using Microsoft.AspNetCore.StaticFiles;
using System.Net.Mail;
using System.Security.Claims;


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
            ViewBag.Error = TempData["Error"]; 
            return View(viewModel);
        }
    // Add SemesterReport
        [HttpPost]
        public async Task<IActionResult> AddReport(SemesterReport report, IFormCollection form)
        {
        // Check validity
            var existedReport = await _context.SemesterReports.FirstOrDefaultAsync(sr => sr.ClassId == report.ClassId && sr.PeriodId == report.PeriodId);
            if (existedReport != null)
            {
                TempData["Warning"] = "Đã tồn tại báo cáo cùng thời gian và lớp, xin hãy chọn lại thông tin phù hợp";
                return RedirectToAction("EndSemesterReport");
            }
        // Get the SE Asia Standard Time timezone
            var vietnamTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
        // Convert the time to Vietnam time
            var vietnamTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vietnamTimeZone);

        // Handling SemesterReports table
            report.PeriodName = await _context.AcademicPeriods.Where(ap => ap.PeriodId == report.PeriodId)
                                                              .Select(ap => ap.PeriodName)
                                                              .FirstOrDefaultAsync();
            report.AdvisorName = User.FindFirstValue(ClaimTypes.NameIdentifier);
            report.CreationTimeReport = vietnamTime;
            report.StatusReport = "Nháp";
            _context.SemesterReports.Add(report);
            await _context.SaveChangesAsync();

        //Handling ReportDetails table
            var rpID = report.ReportId;
            List<ReportDetail> listToAdd = new List<ReportDetail>();

            // Define the criterion groups (each group contains the task numbers)
            var criterionTasks = new[]
            {
                new { CriterionId = 1, TaskCount = 1 },
                new { CriterionId = 2, TaskCount = 1 },
                new { CriterionId = 3, TaskCount = 2 },
                new { CriterionId = 4, TaskCount = 2 },
                new { CriterionId = 5, TaskCount = 3 },
                new { CriterionId = 6, TaskCount = 1 },
                new { CriterionId = 7, TaskCount = 1 },
                new { CriterionId = 8, TaskCount = 2 }
            };

            foreach (var criterion in criterionTasks)
            {
                for (int i = 1; i <= criterion.TaskCount; i++)
                {
                    listToAdd.Add(new ReportDetail()
                    {
                        ReportId = rpID,
                        CriterionId = criterion.CriterionId,
                        TaskReport = form[$"Task{criterion.CriterionId}_{i}"],
                        HowToExecuteReport = form[$"HowToExecute{criterion.CriterionId}_{i}"]
                    });
                }
            }

            // Add filled list to the context and save
            _context.ReportDetails.AddRange(listToAdd);
            await _context.SaveChangesAsync();


            // Handling AttachmentReport table
            // List to hold all attachment reports
            List<AttachmentReport> attToAdd = new List<AttachmentReport>();

            // List of file name patterns and corresponding indices for DetailReportlId in listToAdd
            var fileNamePatterns = new[]
            {
                new { Pattern = "files1_1[]", Index = 0 },
                new { Pattern = "files2_1[]", Index = 1 },
                new { Pattern = "files3_1[]", Index = 2 },
                new { Pattern = "files3_2[]", Index = 3 },
                new { Pattern = "files4_1[]", Index = 4 },
                new { Pattern = "files4_2[]", Index = 5 },
                new { Pattern = "files5_1[]", Index = 6 },
                new { Pattern = "files5_2[]", Index = 7 },
                new { Pattern = "files5_3[]", Index = 8 },
                new { Pattern = "files6_1[]", Index = 9 },
                new { Pattern = "files7_1[]", Index = 10 },
                new { Pattern = "files8_1[]", Index = 11 },
                new { Pattern = "files8_2[]", Index = 12 }
            };
            // Process each file pattern and its corresponding DetailReportlId
            foreach (var pattern in fileNamePatterns)
            {
                // Get files matching the current pattern
                var files = form.Files.Where(f => f.Name == pattern.Pattern).ToList();

                // Process each file
                foreach (IFormFile f in files)
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        await f.CopyToAsync(memoryStream); // Copy file content to memory stream

                        // Create attachment
                        var attachment = new AttachmentReport()
                        {
                            FileNames = f.FileName,
                            FileDatas = memoryStream.ToArray(), // Convert memory stream to byte array
                            DetailReportlId = listToAdd[pattern.Index].DetailReportlId // Use the corresponding DetailReportlId
                        };

                        attToAdd.Add(attachment); // Add the attachment to the list
                    }
                }
            }
            // After processing all attachments, save them to the database
            _context.AttachmentReports.AddRange(attToAdd);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Thêm báo cáo thành công!";
            return RedirectToAction("EndSemesterReport");
        }
        
        public async Task<IActionResult> GetReports()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userRole = User.IsInRole("Faculty");

            var reports = await _context.SemesterReports
                .Where(sr => userRole || sr.AdvisorName == userId) // Kiểm tra quyền
                .ToListAsync();

            return View(reports);
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
                return RedirectToAction("EndSemesterReport");
            }

            // Kiểm tra trạng thái báo cáo
            if (report.StatusReport == "Đã Nộp" || report.StatusReport == "Đã Duyệt")
            {
                TempData["Error"] = "Báo cáo đã được nộp, không thể xóa!";
                return RedirectToAction("EndSemesterReport");
            }
            if (report.AdvisorName != User.FindFirstValue(ClaimTypes.NameIdentifier))
            {
                TempData["Error"] = "Bạn không có quyền xóa báo cáo này.";
                return RedirectToAction("EndSemesterReport");
            }

            // Xóa báo cáo khỏi cơ sở dữ liệu
            _context.SemesterReports.Remove(report);
            await _context.SaveChangesAsync();

            // Thông báo xóa thành công
            TempData["Success"] = "Báo cáo đã được xóa thành công.";
            return RedirectToAction("EndSemesterReport");
        }
        catch (Exception ex)
        {
            // Nếu có lỗi, trả về thông báo lỗi
            TempData["Error"] = $"Đã xảy ra lỗi: {ex.Message}";
            return RedirectToAction("EndSemesterReport");
        }
    }


    //Edit Report
        [HttpPost]
        public async Task<IActionResult> EditReport(int reportId, int periodId, string reportType, string classId)
        {
            var reportToEdit = await _context.SemesterReports.FirstOrDefaultAsync(rp => rp.ReportId == reportId);
            if (reportToEdit == null)
            {
                TempData["Error"] = "Không tìm thấy báo cáo.";
                return RedirectToAction("EndSemesterReport");
            }
            if (reportToEdit.AdvisorName != User.FindFirstValue(ClaimTypes.NameIdentifier))
            {
                TempData["Error"] = "Bạn không có quyền Chỉnh sửa báo cáo này.";
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
        if (!validated)
        {
            TempData["Error"] = "Báo Cáo chưa đầy đủ thông tin, vui lòng điền đầy đủ thông tin trước khi thử lại!";
            return RedirectToAction("EndSemesterReport");
        }

        var reportToSubmit = await _context.SemesterReports.FirstOrDefaultAsync(rp => rp.ReportId == reportId);
        if (reportToSubmit == null)
        {
            TempData["Error"] = "Không tìm thấy Báo Cáo.";
            return RedirectToAction("EndSemesterReport");
        }

        // Kiểm tra trạng thái hiện tại của báo cáo
        if (reportToSubmit.StatusReport == "Đã Nộp")
        {
            TempData["Warning"] = "Báo Cáo đã được nộp, vui lòng chỉnh lại chi tiết và tiến hành nộp lại!";
            return RedirectToAction("EndSemesterReport");
        }

        if (reportToSubmit.StatusReport == "Đã Duyệt")
        {
            TempData["Error"] = "Báo Cáo đã được duyệt và không thể nộp lại!";
            return RedirectToAction("EndSemesterReport");
        }
        if (reportToSubmit.AdvisorName != User.FindFirstValue(ClaimTypes.NameIdentifier))
        {
            TempData["Error"] = "Bạn không có quyền nộp báo cáo này.";
            return RedirectToAction("EndSemesterReport");
        }

        reportToSubmit.StatusReport = "Đã Nộp";
        _context.SemesterReports.Update(reportToSubmit);
        await _context.SaveChangesAsync();
        TempData["Success"] = "Nộp Báo Cáo thành công";
        return RedirectToAction("EndSemesterReport");
    }

        // Browse Report
        [HttpPost]
        public async Task<IActionResult> BrowseReport(int reportId)
        {

            var reportToBrowse = await _context.SemesterReports.FirstOrDefaultAsync(rp => rp.ReportId == reportId);
            if (reportToBrowse.StatusReport == "Duyệt")
            {
                TempData["Warning"] = "Báo Cáo đã được Duyệt!";
                return RedirectToAction("EndSemesterReport");
            }
            else if (reportToBrowse.StatusReport == "Nháp")
            {
                TempData["Error"] = "Báo Cáo ở trạng thái Nháp, không thể Duyệt!";
                return RedirectToAction("EndSemesterReport");
            }

            reportToBrowse.StatusReport = "Đã Duyệt";
            _context.SemesterReports.Update(reportToBrowse);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Duyệt Báo Cáo thành công";
            return RedirectToAction("EndSemesterReport");
        }
        
    //Duplicate Report
        [HttpPost]
        public async Task<IActionResult> DuplicateReport(int reportId)
        {
            //Fetch the original SemesterReport
            var originalReport = _context.SemesterReports
                .Include(rp => rp.ReportDetails) // Load associated PlanDetails
                .FirstOrDefault(sp => sp.ReportId == reportId);
            var vietnamTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            //Duplicated report data
            var dupReport = new SemesterReport{ 
                ClassId = originalReport.ClassId,
                ReportType = originalReport.ReportType,
                PeriodId = originalReport.PeriodId,
                CreationTimeReport = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vietnamTimeZone),
                AdvisorName = User.FindFirstValue(ClaimTypes.NameIdentifier),
                PeriodName = originalReport.PeriodName,
                StatusReport = "Nháp"
            };
            _context.SemesterReports.Add(dupReport);
            await _context.SaveChangesAsync();

            //Duplicate report details
            foreach (var detail in originalReport.ReportDetails)
            {
                var duplicatedDetail = new ReportDetail
                {
                    ReportId = dupReport.ReportId, 
                    TaskReport = detail.TaskReport,
                    CriterionId = detail.CriterionId,
                    HowToExecuteReport = detail.HowToExecuteReport,
                    AttachmentReport = detail.AttachmentReport                
                };

                _context.ReportDetails.Add(duplicatedDetail);
            }
            await _context.SaveChangesAsync();

            TempData["Success"] = "Nhân bản Báo Cáo thành công!";
            return RedirectToAction("EndSemesterReport");
        }
        
    //View Report Detail
        [HttpGet]
        public async Task<IActionResult> GetReportDetails(int reportId) 
        {
            if (reportId <= 0)
            {
                return BadRequest("Invalid Report ID.");
            }

            var assessmentDatas = await _context.SemesterReports
                .Where(sr => sr.ReportId == reportId)
                .Select(sr => new
                {
                    sr.SelfAssessment,
                    sr.SelfRanking,
                    sr.FacultyAssessment,
                    sr.FacultyRanking
                })
                .FirstOrDefaultAsync();

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
                })
                .ToListAsync();

            //var attachments =await _context.AttachmentReports
            //        .Where(a => a.DetailReport.ReportId == reportId)
            //        .ToListAsync();
            if (!reportDetails.Any())
            {
                return NotFound("No data found.");
            }
            // Combine both assessment data and report details into a single object
            var result = new
            {
                AssessmentData = assessmentDatas,
                ReportDetails = reportDetails,
                //Attachments = attachments
            };

            return Json(result); // Return the combined data as JSON
        }

    //Edit Report Details
        [HttpPost]
        public async Task<IActionResult> EditReportDetail(int ReportId ,IFormCollection form)
        {
            // edit report
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var report = await _context.SemesterReports.FirstOrDefaultAsync(rp => rp.ReportId == ReportId);
            if (report == null)
            {
                TempData["Error"] = "Không tìm thấy báo cáo.";
                return RedirectToAction("EndSemesterReport");
            }
            bool isOtherFieldsModified = !string.IsNullOrEmpty(form["EditselfAssessment"]) ||
                                         !string.IsNullOrEmpty(form["Editranking"]);
            bool isFacultyFieldsModified = !string.IsNullOrEmpty(form["EditFaculityAssessment"]) ||
                                           !string.IsNullOrEmpty(form["EditFaculityranking"]);
            

            bool isCreator = report.AdvisorName == userId;
            if (isOtherFieldsModified && !isCreator)
            {
                TempData["Error"] = "Bạn không có quyền chỉnh sửa các trường khác ngoài FacultyAssessment và FacultyRanking.";
                return RedirectToAction("EndSemesterReport");
            }
            if (isCreator)
            {
                report.SelfAssessment = form["EditselfAssessment"];
                report.FacultyAssessment = form["EditFaculityAssessment"];
                report.SelfRanking = form["Editranking"].ToString().FirstOrDefault();
                report.FacultyRanking = form["EditFaculityranking"].ToString().FirstOrDefault();

                _context.SemesterReports.Update(report);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Cập nhật báo cáo thành công!";
                return RedirectToAction("EndSemesterReport");
            }
            if (isFacultyFieldsModified)
            {
                report.FacultyAssessment = form["EditFaculityAssessment"];
                report.FacultyRanking = form["EditFaculityranking"].ToString().FirstOrDefault();

                _context.SemesterReports.Update(report);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Cập nhật đánh giá thành công!";
                return RedirectToAction("EndSemesterReport");
            }


// Logic dành cho người tạo báo cáo
            report.SelfAssessment = form["EditselfAssessment"];
            report.FacultyAssessment = form["EditFaculityAssessment"];
            report.SelfRanking = form["Editranking"].ToString().FirstOrDefault();
            report.FacultyRanking = form["EditFaculityranking"].ToString().FirstOrDefault();

            _context.SemesterReports.Update(report);
            // edit details
            var details = await _context.ReportDetails.Where(dt=> dt.ReportId == ReportId).ToListAsync();
            var patterns = new[]
            {
                "HowToExecute1_1",
                "HowToExecute2_1", 
                "HowToExecute3_1",
                "HowToExecute3_2",
                "HowToExecute4_1",
                "HowToExecute4_2",
                "HowToExecute5_1",
                "HowToExecute5_2",
                "HowToExecute5_3",
                "HowToExecute6_1",
                "HowToExecute7_1",
                "HowToExecute8_1",
                "HowToExecute8_2"
            };
            for(int i = 0; i < details.Count; i++)
            {
                details[i].HowToExecuteReport = form[patterns[i]];
            }
            _context.ReportDetails.UpdateRange(details);


            // Handling attachments
            // List to hold all attachment reports
            List<AttachmentReport> attToAdd = new List<AttachmentReport>();

            // List of file name patterns and corresponding indices for DetailReportlId in listToAdd
            var fileNamePatterns = new[]
            {
                new { Pattern = "files1_1[]", Index = 0 },
                new { Pattern = "files2_1[]", Index = 1 },
                new { Pattern = "files3_1[]", Index = 2 },
                new { Pattern = "files3_2[]", Index = 3 },
                new { Pattern = "files4_1[]", Index = 4 },
                new { Pattern = "files4_2[]", Index = 5 },
                new { Pattern = "files5_1[]", Index = 6 },
                new { Pattern = "files5_2[]", Index = 7 },
                new { Pattern = "files5_3[]", Index = 8 },
                new { Pattern = "files6_1[]", Index = 9 },
                new { Pattern = "files7_1[]", Index = 10 },
                new { Pattern = "files8_1[]", Index = 11 },
                new { Pattern = "files8_2[]", Index = 12 }
            };
            // Process each file pattern and its corresponding DetailReportlId
            foreach (var pattern in fileNamePatterns)
            {
                // Get files matching the current pattern
                var files = form.Files.Where(f => f.Name == pattern.Pattern).ToList();

                // Process each file
                foreach (IFormFile f in files)
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        await f.CopyToAsync(memoryStream); // Copy file content to memory stream

                        // Create attachment
                        var attachment = new AttachmentReport()
                        {
                            FileNames = f.FileName,
                            FileDatas = memoryStream.ToArray(), // Convert memory stream to byte array
                            DetailReportlId = details[pattern.Index].DetailReportlId // Use the corresponding DetailReportlId
                        };

                        attToAdd.Add(attachment); // Add the attachment to the list
                    }
                }
            }
            _context.AttachmentReports.AddRange(attToAdd);       
            await _context.SaveChangesAsync();
            TempData["Success"] = "Sửa đổi chi tiết báo cáo thành công!";

            return RedirectToAction("EndSemesterReport");
        }

    //Get Attachments
        [HttpGet]
        public async Task<IActionResult> GetReportAttachments(int detailId)
        {
            // Fetch the list of attachments for the specified report and detail
            var attachments = await _context.AttachmentReports
                .Where(a => a.DetailReportlId == detailId)
                .Select(a => new
                {
                    a.FileNames,
                    a.DetailReportlId,
                    a.AttachmentReportId
                })
                .ToListAsync();
            // Return the attachments as JSON
            return Json(attachments);
        }

    //Download Proofs
        [HttpGet]
        public async Task<IActionResult> DownloadProof(int proofId)
        {
            var attachment = await _context.AttachmentReports
                .FirstOrDefaultAsync(a => a.AttachmentReportId == proofId);

            if (attachment == null)
            {
                return NotFound();
            }

            // Return the file as a download
            // Set the file name and the content type (e.g., PDF, Image, etc.)
            var fileName = attachment.FileNames;
            var fileBytes = attachment.FileDatas; // Byte array containing the file data

            // Using FileExtensionContentTypeProvider to get MIME type dynamically
            var provider = new FileExtensionContentTypeProvider();
            var contentType = "application/octet-stream"; // Default content type

            if (!provider.TryGetContentType(fileName, out contentType))
            {
                contentType = "application/octet-stream"; // Use generic binary type if no match
            }

            // Set the Content-Disposition header to prompt the user to download the file
            Response.Headers.Add("Content-Disposition", $"attachment; filename*=UTF-8''{Uri.EscapeDataString(fileName)}");
            return File(fileBytes, contentType, fileName);
        }
    //Delete Proofs
        [HttpPost]
        public async Task<IActionResult> DeleteProof(int proofId)
        {
            var attachment = await _context.AttachmentReports
                .FirstOrDefaultAsync(a => a.AttachmentReportId == proofId);

            if (attachment == null)
            {
                return Json(new { success = false, message = "File not found." });
            }

            _context.AttachmentReports.Remove(attachment);
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }
    //Export Report
        [HttpPost]
        public async Task<IActionResult> ExportReport(int reportId)
        {
            var currentUser = await _context.ApplicationUsers.FirstOrDefaultAsync(u => u.Email == User.Identity.Name);
            var targetReport = await _context.SemesterReports.FirstOrDefaultAsync(rp=> rp.ReportId == reportId);
            var targetAdvisor =
                await _context.ApplicationUsers.FirstOrDefaultAsync(t => t.Email == targetReport.AdvisorName);
            var targetClasses = await _context.Classes
               .Where(c => c.AdvisorId == currentUser.Id) // Filter by current user's AdvisorId
               .Select(c => new SelectListItem
               {
                   Value = c.ClassId,
                   Text = c.ClassId
               })
               .ToListAsync(); 
            string classesString = string.Join(" - ", targetClasses.Select(c => c.Text));
            var details = await _context.ReportDetails.Where(dt => dt.ReportId == reportId).ToListAsync();
            // Create a new document
            var doc = DocX.Create($"Báo cáo năm học {targetReport.PeriodName}.docx");
            // Set page orientation to Landscape
            doc.PageLayout.Orientation = Orientation.Landscape;
            // Add content to the document
            doc.InsertParagraph("BẢNG TỰ ĐÁNH GIÁ HOẠT ĐỘNG CỐ VẤN HỌC TẬP ").FontSize(16).Bold().Alignment = Alignment.center;
            doc.InsertParagraph($"NĂM HỌC: {targetReport.PeriodName}").FontSize(13).Bold().Alignment = Alignment.center;
            doc.InsertParagraph("Khoa: Công nghệ Thông tin ").FontSize(14).Bold().Alignment = Alignment.left;
            doc.InsertParagraph("Họ và tên giảng viên: " + targetAdvisor.FullName).FontSize(14).Bold().Alignment = Alignment.left;
            doc.InsertParagraph("Mã số giảng viên: " + targetAdvisor.SchoolId).FontSize(14).Bold().Alignment = Alignment.left;
            var classNames = doc.InsertParagraph();
            classNames.Append("Lớp phụ trách: ")
                .FontSize(14); 

            classNames.Append(classesString)
                .FontSize(14)
                .Color(System.Drawing.Color.Red)
                .Bold();

            classNames.Alignment = Alignment.left; // Set alignment for the entire paragraph

            // Create a table with 11 rows and 7 columns
            var table = doc.AddTable(14, 4);

            // Table Headers
            var headers = new[]
            {
                "Mục ", "Tiêu chí đánh giá ", "Mô tả công việc cụ thể ", "Hồ sơ, minh chứng "
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
            table.MergeCellsInColumn(0, 3, 4);
            table.MergeCellsInColumn(0, 5, 6);
            table.MergeCellsInColumn(0, 7, 9);
            table.MergeCellsInColumn(0, 12, 13);
            table.MergeCellsInColumn(1, 3, 4);
            table.MergeCellsInColumn(1, 5, 6);
            table.MergeCellsInColumn(1, 7, 9);
            table.MergeCellsInColumn(1, 12, 13);

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


            // Table Content
            //1st column
            table.Rows[1].Cells[0].Paragraphs[0].Append("1").FontSize(12).Alignment = Alignment.center;
            table.Rows[2].Cells[0].Paragraphs[0].Append("2").FontSize(12).Alignment = Alignment.center;
            table.Rows[3].Cells[0].Paragraphs[0].Append("3").FontSize(12).Alignment = Alignment.center;
            table.Rows[5].Cells[0].Paragraphs[0].Append("4").FontSize(12).Alignment = Alignment.center;
            table.Rows[7].Cells[0].Paragraphs[0].Append("5").FontSize(12).Alignment = Alignment.center;
            table.Rows[10].Cells[0].Paragraphs[0].Append("6").FontSize(12).Alignment = Alignment.center;
            table.Rows[11].Cells[0].Paragraphs[0].Append("7").FontSize(12).Alignment = Alignment.center;
            table.Rows[12].Cells[0].Paragraphs[0].Append("8").FontSize(12).Alignment = Alignment.center;

            //2nd column
            table.Rows[1].Cells[1].Paragraphs[0].Append("Có kế hoạch hoạt động về công tác CVHT của năm học ").FontSize(12).Alignment = Alignment.center;
            table.Rows[2].Cells[1].Paragraphs[0].Append("CVHT khai thác hệ thống quản lý học tập của SV để nắm rõ tình trạng học tập của SV ").FontSize(12).Alignment = Alignment.center;
            table.Rows[3].Cells[1].Paragraphs[0].Append("Tổ chức các cuộc họp lớp, tư vấn cho sinh viên các nội dung học tập như: quy chế, quy định về tổ chức hoạt động đào tạo theo học chế tín chỉ; cấu trúc, lưu đồ của CTĐT, mục tiêu đào tạo, chuẩn đầu ra của ngành/chuyên ngành…").FontSize(12).Alignment = Alignment.center;
            table.Rows[5].Cells[1].Paragraphs[0].Append("Hướng dẫn SV lập kế hoạch học tập phù hợp với cá nhân từng học kỳ/năm học/khóa học. Xem xét và phê duyệt kế hoạch học tập của SV đối với những trường hợp có yêu cầu. ").FontSize(12).Alignment = Alignment.center;
            table.Rows[7].Cells[1].Paragraphs[0].Append("Trao đổi kinh nghiệm cá nhân trong học tập và nghề nghiệp, định hướng nghề nghiệp cho SV có nhu cầu. Tư vấn cho SV lựa chọn chuyên ngành (nếu có) ").FontSize(12).Alignment = Alignment.center;
            table.Rows[10].Cells[1].Paragraphs[0].Append("Tư vấn SV thuộc diện cảnh báo học vụ (Quy trình cảnh báo sớm). ").FontSize(12).Alignment = Alignment.center;
            table.Rows[11].Cells[1].Paragraphs[0].Append("Các ý kiến, đề xuất, khiếu nại của SV được phản ánh kịp thời đến các cấp có thẩm quyền. ").FontSize(12).Alignment = Alignment.center;
            table.Rows[12].Cells[1].Paragraphs[0].Append("Tham gia đầy đủ các buổi tập huấn/hội nghị/cuộc họp liên quan đến công tác CVHT do Khoa, Trường tổ chức (nếu có). ").FontSize(12).Alignment = Alignment.center;

            //3rd column
            table.Rows[1].Cells[2].Paragraphs[0].Append("Có kế hoạch hoạt động về công tác CVHT của năm học ").FontSize(12).Alignment = Alignment.center;
            table.Rows[2].Cells[2].Paragraphs[0].Append("CVHT sử dụng thành thạo tài khoản online đã được phân quyền để khai thác hiệu quả và đánh giá được kết quả học tập của SV đối với lớp do mình làm cố vấn. ").FontSize(12).Alignment = Alignment.center;
            table.Rows[3].Cells[2].Paragraphs[0].Append("3.1 CVHT thu thập và hiểu các văn bản liên quan quy định, quy chế đào tạo, chuẩn đầu ra, cấu trúc, lưu đồ CTĐT…phổ biến nội dung trong các buổi sinh hoạt lớp và tư vấn trực tiếp cho SV khi cần. Tổ chức sinh hoạt lớp SV bằng hình thức trực tiếp hoặc trực tuyến (online) ít nhất 2 lần/học kỳ; có biên bản các cuộc họp đầy đủ, đáp ứng yêu cầu về nội dung họp. ").FontSize(12).Alignment = Alignment.center;
            table.Rows[4].Cells[2].Paragraphs[0].Append("3.2 Tiếp SV trực tiếp theo lịch trực tại văn phòng khoa hoặc thông qua các phương tiện khác ").FontSize(12).Alignment = Alignment.center;
            table.Rows[5].Cells[2].Paragraphs[0].Append("4.1 Hướng dẫn sinh viên cách xây dựng kế hoạch học tập cho khóa học hoặc năm học, tư vấn cho sinh viên lập kế hoạch học tập của năm học hoặc khóa học, xem xét và phê duyệt kế hoạch học tập của sinh viên đối với những trường hợp có yêu cầu từ Phòng Đào tạo. ").FontSize(12).Alignment = Alignment.center;
            table.Rows[6].Cells[2].Paragraphs[0].Append("4.2 Hướng dẫn sinh viên điều chỉnh kế hoạch học tập và đăng ký môn học phù hợp với năng lực và hoàn cảnh của từng sinh viên. ").FontSize(12).Alignment = Alignment.center;
            table.Rows[7].Cells[2].Paragraphs[0].Append("5.1 Tư vấn sinh viên lựa chọn chuyên ngành").FontSize(12).Alignment = Alignment.center;
            table.Rows[8].Cells[2].Paragraphs[0].Append("5.2 Trao đổi kinh nghiệm cá nhân trong học tập và nghề nghiệp, định hướng nghề nghiệp cho sinh viên có nhu cầu. ").FontSize(12).Alignment = Alignment.center;
            table.Rows[9].Cells[2].Paragraphs[0].Append("5.3 Giới thiệu nơi thực tập và việc làm cho SV (nếu có). ").FontSize(12).Alignment = Alignment.center;
            table.Rows[10].Cells[2].Paragraphs[0].Append("Tư vấn sinh viên thuộc diện cảnh báo học vụ hoặc có nguy cơ bị cảnh báo học vụ ở từng học kỳ có phương án học tập tiếp theo").FontSize(12).Alignment = Alignment.center;
            table.Rows[11].Cells[2].Paragraphs[0].Append("CVHT làm cầu nối giữa SV và Khoa, Trường trong đề đạt nguyện vọng, ý kiến, phản ánh nhằm xây dựng môi trường học tập lành mạnh. Không có trường hợp SV gửi đơn thư khiếu nại vượt cấp. ").FontSize(12).Alignment = Alignment.center;
            table.Rows[12].Cells[2].Paragraphs[0].Append("8.1 Tham dự họp, hội nghị theo triệu tập của của các cấp: Khoa - Phòng chức năng - Trường.").FontSize(12).Alignment = Alignment.center;
            table.Rows[13].Cells[2].Paragraphs[0].Append("8.2 Tham gia họp tổng kết, đánh giá công tác CVHT do Hội đồng Khoa triệu tập. ").FontSize(12).Alignment = Alignment.center;

            //4rd column
            for(int i = 1; i <= details.Count; i++)
            {
                table.Rows[i].Cells[3].Paragraphs[0].Append(details[i-1].HowToExecuteReport).FontSize(12).Alignment = Alignment.left;
                table.Rows[i].Cells[3].Paragraphs[0].AppendLine(""); // Insert a line break
                table.Rows[i].Cells[3].Paragraphs[0].AppendLine(""); // Insert a line break
                table.Rows[i].Cells[3].Paragraphs[0].Append("Danh sách minh chứng: ").FontSize(12).Alignment = Alignment.left;
                table.Rows[i].Cells[3].Paragraphs[0].AppendLine(""); // Insert a line break
                var proofs = await _context.AttachmentReports
                    .Where(p => p.DetailReportlId == details[i-1].DetailReportlId)
                    .ToListAsync();
                foreach(var file in proofs)
                {
                    table.Rows[i].Cells[3].Paragraphs[0].Append("-" + file.FileNames).FontSize(12).Alignment = Alignment.left;
                    table.Rows[i].Cells[3].Paragraphs[0].AppendLine(""); // Insert a line break
                }
            }

            // Insert the table into the document
            doc.InsertParagraph().InsertTableAfterSelf(table);

            // Cá nhân tự đánh giá
            doc.InsertParagraph("Cá nhân tự đánh giá: " + targetReport.SelfAssessment)
               .FontSize(14)
               .Bold()
               .Alignment = Alignment.left;

            // Xếp loại
            doc.InsertParagraph("Xếp loại: " + targetReport.SelfRanking)
               .FontSize(14)
               .Bold()
               .Alignment = Alignment.left;

            // TP.Hồ Chí Minh, ngày 10 tháng 08 năm 2024
            doc.InsertParagraph("TP.Hồ Chí Minh, ngày  tháng  năm ")
               .FontSize(14)
               .Alignment = Alignment.right;

            // Add a blank paragraph for spacing
            doc.InsertParagraph().SpacingBefore(10);

            // Insert the first part of the text
            var headerParagraph = doc.InsertParagraph()
                .Append("PHÓ TRƯỞNG KHOA")
                .FontSize(14)
                .Bold();

            // Set alignment after appending the text
            headerParagraph.Alignment = Alignment.left; // Correct way

            // Insert the second part of the text
            var secondParagraph = doc.InsertParagraph()
                .Append("\t\t\t\t\t\tCỐ VẤN HỌC TẬP")
                .FontSize(14)
                .Bold();

            // Set the alignment for the second paragraph
            secondParagraph.Alignment = Alignment.right; // Correct wa

            // Add multiple blank lines for spacing
            for (int i = 0; i < 4; i++)
            {
                doc.InsertParagraph().SpacingBefore(10);
            }

            // Save the document to a MemoryStream
            using (var memoryStream = new MemoryStream())
            {
                doc.SaveAs(memoryStream);
                memoryStream.Position = 0;
                // Return the document as a file download
                return File(memoryStream.ToArray(),
                    "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                    $"Tự đánh giá CVHT_{targetAdvisor.FullName}.docx");
            }
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
            plan.AdvisorName = User.FindFirstValue(ClaimTypes.NameIdentifier);
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
        
        public async Task<IActionResult> GetPlan()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userRole = User.IsInRole("Faculty");

            var plans = await _context.SemesterPlans
                .Where(sr => userRole || sr.AdvisorName == userId) 
                .ToListAsync();

            return View(plans);
        }

    //Edit Plan Detail
        [HttpPost]
        public async Task<IActionResult> EditPlanDetail(IFormCollection form)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            // Get the PlanIds value from the form collection
            var planIdsString = form["DetailIds"].ToString();
            // Split the string into an array of integers
            var detailIds = planIdsString.Split(',').Select(int.Parse).ToArray();
            List<PlanDetail> detailsToEdit = new List<PlanDetail>();
            //Detail 1
            var detail1 = await _context.PlanDetails
                .Include(pd => pd.SemesterPlan)
                .FirstOrDefaultAsync(pl => pl.DetailId == detailIds[0]);

            if (detail1.SemesterPlan.AdvisorName != userId)
            {
                TempData["Error"] = "Bạn không có quyền chỉnh sửa kế hoạch này!";
                return RedirectToAction("SemesterPlan");
            }
            detail1.Task = form["EditTask1_1"];
            detail1.HowToExecute = form["EditHowToExecute1_1"];
            detail1.Quantity = form["EditQuantity1_1"];
            detail1.TimeFrame = form["EditTimeFrame1_1"];
            detail1.Notes = form["EditNotes1_1"];
            detailsToEdit.Add(detail1);
            //Detail 2
            var detail2 = await _context.PlanDetails.FirstOrDefaultAsync(pl => pl.DetailId == detailIds[1]);
            if (detail2.SemesterPlan.AdvisorName != userId)
            {
                TempData["Error"] = "Bạn không có quyền chỉnh sửa kế hoạch này!";
                return RedirectToAction("SemesterPlan");
            }
            detail2.Task = form["EditTask1_2"];
            detail2.HowToExecute = form["EditHowToExecute1_2"];
            detail2.Quantity = form["EditQuantity1_2"];
            detail2.TimeFrame = form["EditTimeFrame1_2"];
            detail2.Notes = form["EditNotes1_2"];
            detailsToEdit.Add(detail2);
            //Detail 3
            var detail3 = await _context.PlanDetails.FirstOrDefaultAsync(pl => pl.DetailId == detailIds[2]);
            if (detail3.SemesterPlan.AdvisorName != userId)
            {
                TempData["Error"] = "Bạn không có quyền chỉnh sửa kế hoạch này!";
                return RedirectToAction("SemesterPlan");
            }
            detail3.Task = form["EditTask1_3"];
            detail3.HowToExecute = form["EditHowToExecute1_3"];
            detail3.Quantity = form["EditQuantity1_3"];
            detail3.TimeFrame = form["EditTimeFrame1_3"];
            detail3.Notes = form["EditNotes1_3"];
            detailsToEdit.Add(detail3);
            //Detail 4
            var detail4 = await _context.PlanDetails.FirstOrDefaultAsync(pl => pl.DetailId == detailIds[3]);
            if (detail4.SemesterPlan.AdvisorName != userId)
            {
                TempData["Error"] = "Bạn không có quyền chỉnh sửa kế hoạch này!";
                return RedirectToAction("SemesterPlan");
            }
            detail4.Task = form["EditTask2_1"];
            detail4.HowToExecute = form["EditHowToExecute2_1"];
            detail4.Quantity = form["EditQuantity2_1"];
            detail4.TimeFrame = form["EditTimeFrame2_1"];
            detail4.Notes = form["EditNotes2_1"];
            detailsToEdit.Add(detail4);
            //Detail 5
            var detail5 = await _context.PlanDetails.FirstOrDefaultAsync(pl => pl.DetailId == detailIds[4]);
            if (detail5.SemesterPlan.AdvisorName != userId)
            {
                TempData["Error"] = "Bạn không có quyền chỉnh sửa kế hoạch này!";
                return RedirectToAction("SemesterPlan");
            }
            detail5.Task = form["EditTask2_2"];
            detail5.HowToExecute = form["EditHowToExecute2_2"];
            detail5.Quantity = form["EditQuantity2_2"];
            detail5.TimeFrame = form["EditTimeFrame2_2"];
            detail5.Notes = form["EditNotes2_2"];
            detailsToEdit.Add(detail5);
            //Detail 6
            var detail6 = await _context.PlanDetails.FirstOrDefaultAsync(pl => pl.DetailId == detailIds[5]);
            if (detail6.SemesterPlan.AdvisorName != userId)
            {
                TempData["Error"] = "Bạn không có quyền chỉnh sửa kế hoạch này!";
                return RedirectToAction("SemesterPlan");
            }
            detail6.Task = form["EditTask3_1"];
            detail6.HowToExecute = form["EditHowToExecute3_1"];
            detail6.Quantity = form["EditQuantity3_1"];
            detail6.TimeFrame = form["EditTimeFrame3_1"];
            detail6.Notes = form["EditNotes3_1"];
            detailsToEdit.Add(detail6);
            //Detail 7
            var detail7 = await _context.PlanDetails.FirstOrDefaultAsync(pl => pl.DetailId == detailIds[6]);
            if (detail7.SemesterPlan.AdvisorName != userId)
            {
                TempData["Error"] = "Bạn không có quyền chỉnh sửa kế hoạch này!";
                return RedirectToAction("SemesterPlan");
            }
            detail7.Task = form["EditTask3_2"];
            detail7.HowToExecute = form["EditHowToExecute3_2"];
            detail7.Quantity = form["EditQuantity3_2"];
            detail7.TimeFrame = form["EditTimeFrame3_2"];
            detail7.Notes = form["EditNotes3_1"];
            detailsToEdit.Add(detail7);
            //Detail 8
            var detail8 = await _context.PlanDetails.FirstOrDefaultAsync(pl => pl.DetailId == detailIds[7]);
            if (detail8.SemesterPlan.AdvisorName != userId)
            {
                TempData["Error"] = "Bạn không có quyền chỉnh sửa kế hoạch này!";
                return RedirectToAction("SemesterPlan");
            }
            detail8.Task = form["EditTask4_1"];
            detail8.HowToExecute = form["EditHowToExecute4_1"];
            detail8.Quantity = form["EditQuantity4_1"];
            detail8.TimeFrame = form["EditTimeFrame4_1"];
            detail8.Notes = form["EditNotes4_1"];
            detailsToEdit.Add(detail8);
            //Detail 9
            var detail9 = await _context.PlanDetails.FirstOrDefaultAsync(pl => pl.DetailId == detailIds[8]);
            if (detail9.SemesterPlan.AdvisorName != userId)
            {
                TempData["Error"] = "Bạn không có quyền chỉnh sửa kế hoạch này!";
                return RedirectToAction("SemesterPlan");
            }
            detail9.Task = form["EditTask4_2"];
            detail9.HowToExecute = form["EditHowToExecute4_2"];
            detail9.Quantity = form["EditQuantity4_2"];
            detail9.TimeFrame = form["EditTimeFrame4_2"];
            detail9.Notes = form["EditNotes4_1"];
            detailsToEdit.Add(detail9);
            //Detail 10
            var detail10 = await _context.PlanDetails.FirstOrDefaultAsync(pl => pl.DetailId == detailIds[9]);
            if (detail10.SemesterPlan.AdvisorName != userId)
            {
                TempData["Error"] = "Bạn không có quyền chỉnh sửa kế hoạch này!";
                return RedirectToAction("SemesterPlan");
            }
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
            
            if (planToEdit.AdvisorName != User.FindFirstValue(ClaimTypes.NameIdentifier))
            {
                TempData["Error"] = "Bạn không có quyền chỉnh sửa kế hoạch này.";
                return RedirectToAction("SemesterPlan");
            }

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
            
            if (planToSubmit.Status == "Đã Duyệt")
            {
                TempData["Error"] = "Báo Cáo đã được duyệt và không thể nộp lại!";
                return RedirectToAction("SemesterPlan");
            }
            if (planToSubmit.AdvisorName != User.FindFirstValue(ClaimTypes.NameIdentifier))
            {
                TempData["Error"] = "Bạn không có quyền nộp kế hoạch này.";
                return RedirectToAction("SemesterPlan");
            }

            planToSubmit.Status = "Đã Nộp";
            _context.SemesterPlans.Update(planToSubmit);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Nộp kế hoạch thành công";
            return RedirectToAction("SemesterPlan");
        }
        
        //Browse Plan
        [HttpPost]
        public async Task<IActionResult> BrowsePlan(int planId)
        {
            var planToBrowse = await _context.SemesterPlans.FirstOrDefaultAsync(pl => pl.PlanId == planId);
            if (planToBrowse.Status == "Đã Duyệt")
            {
                TempData["Warning"] = "Kế hoạch đã được Duyệt!";
                return RedirectToAction("SemesterPlan");
            }
            else if (planToBrowse.Status == "Nháp")
            {
                TempData["Error"] = "Kế hoạch ở trạng thái Nháp, không thể Duyệt!";
                return RedirectToAction("SemesterPlan");
            }

            planToBrowse.Status = "Đã Duyệt";
            _context.SemesterPlans.Update(planToBrowse);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Duyệt kế hoạch thành công";
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
            
            if (targetPlan.Status == "Đã Nộp" || targetPlan.Status == "Đã Duyệt")
            {
                TempData["Error"] = "Kế Hoạch đã được nộp, không thể xóa!";
                return RedirectToAction("SemesterPlan");
            }
            if (targetPlan.AdvisorName != User.FindFirstValue(ClaimTypes.NameIdentifier))
            {
                TempData["Error"] = "Bạn không có quyền xóa kế hoạch này.";
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
                AdvisorName = User.FindFirstValue(ClaimTypes.NameIdentifier),
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
    //Render view
        public async Task<IActionResult> ReceiveEmail(int pageIndex = 1, int pageSize = 20)
        {
            ViewData["page"] = "ReceiveEmail";
            var currentUser = await _context.ApplicationUsers.FirstOrDefaultAsync(us => us.UserName == User.Identity.Name);

            var emails = _context.Emails
                .Include(e => e.Recipients)
                    .ThenInclude(r => r.User)
                .Where(e => e.Recipients.Any(r => r.User.UserName == User.Identity.Name))
                .OrderByDescending(e => e.SentDate)
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

            var paginatedEmails = await PaginatedList<Email>.CreateAsync(emails, pageIndex, pageSize);

            var viewmodel = new EmailViewModel
            {
                Emails = paginatedEmails
            };
            ViewBag.Warning = TempData["Warning"];
            ViewBag.Success = TempData["Success"];
            ViewBag.Error = TempData["Error"];

            return View(viewmodel);
        }

    //Get unread email
        public async Task<IActionResult> GetUnreadEmailsCount()
        {
            // Find the current user's ID or username
            var currentUserName = User.Identity.Name;

            // Query emails where the current user is a recipient and the email is unread
            var unreadEmailsCount = await _context.Emails
                .Include(e => e.Recipients) // Include the Recipients collection
                .Where(e => e.Recipients.Any(r => r.User.UserName == currentUserName && !r.IsRead)) // Filter for unread emails
                .CountAsync(); // Count the unread emails

            // Return the count as JSON
            return Json(new { count = unreadEmailsCount });
        }

    // Mark Email as read
        [HttpPost]
        public async Task<IActionResult> MarkEmailAsRead(int emailId)
        {
            // Get the current user's ID
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Find the recipient's entry for the specific email and user
            var emailRecipient = await _context.EmailRecipients
                .FirstOrDefaultAsync(er => er.EmailId == emailId && er.UserId == userId);

            if (emailRecipient != null)
            {
                // Set the email recipient's IsRead property to true
                emailRecipient.IsRead = true;

                // Save the changes to the database
                await _context.SaveChangesAsync();

                // Set TempData messages for success or failure
                TempData["Success"] = "Email đã chuyển trạng thái thành đã xem!";
                return Json(new { success = true, redirectUrl = Url.Action("ReceiveEmail") }); // Return a JSON response with the redirect URL
            }

            // Return a failure response if the email recipient was not found
            TempData["Error"] = "Không tìm thấy email!";
            return Json(new { success = false, redirectUrl = Url.Action("ReceiveEmail") }); // Return the redirect URL even on failure
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
                    .OrderByDescending(e=>e.SentDate)
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
                EmailId = email.EmailId,
                Recipients = new
                {
                    To = recipientsByType.ContainsKey("To") ? recipientsByType["To"] : new List<string>(),
                    Cc = recipientsByType.ContainsKey("Cc") ? recipientsByType["Cc"] : new List<string>(),
                    Bcc = recipientsByType.ContainsKey("Bcc") ? recipientsByType["Bcc"] : new List<string>()
                },
                Subject = email.Subject,
                Content = email.Content,
                SentDate = email.SentDate,
                Attachments = email.Attachments.Select(a => new
                {
                    a.AttachmentId, // Include the attachmentId
                    a.FileName           // Return the file name
                }).ToList(),
                Sender = email.Sender.UserName
            });
        }
    //Download files
        [HttpGet]
        public async Task<IActionResult> DownloadAttachment(int attachmentId)
        {
            var attachment = await _context.EmailAttachments
                .FirstOrDefaultAsync(a => a.AttachmentId == attachmentId);

            if (attachment == null)
            {
                return NotFound();
            }

            // Return the file as a download
            // Set the file name and the content type (e.g., PDF, Image, etc.)
            var fileName = attachment.FileName;
            var fileBytes = attachment.FileData; // Byte array containing the file data
            
            // Using FileExtensionContentTypeProvider to get MIME type dynamically
            var provider = new FileExtensionContentTypeProvider();
            var contentType = "application/octet-stream"; // Default content type

            if (!provider.TryGetContentType(fileName, out contentType))
            {
                contentType = "application/octet-stream"; // Use generic binary type if no match
            }

            // Set the Content-Disposition header to prompt the user to download the file
            Response.Headers.Add("Content-Disposition", $"attachment; filename*=UTF-8''{Uri.EscapeDataString(fileName)}");
            return File(fileBytes, contentType, fileName);
        }

    }
}
   


