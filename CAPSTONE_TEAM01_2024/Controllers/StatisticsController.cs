using CAPSTONE_TEAM01_2024.Models;
using CAPSTONE_TEAM01_2024.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using Newtonsoft.Json;
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
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using Color = System.Drawing.Color;

namespace CAPSTONE_TEAM01_2024.Controllers
{
   public class StatisticsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IWebHostEnvironment _webHostEnvironment;

    // Constructor để inject DbContext
    public StatisticsController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
    {
        _context = context;
        _webHostEnvironment = webHostEnvironment;
    }

    public async Task<IActionResult> ShowStatistics()
    {
        ViewData["page"] = "ShowStatistics";
        var noOfStudent = (from user in _context.ApplicationUsers
                           join userRole in _context.UserRoles on user.Id equals userRole.UserId into userRoles
                           from userRole in userRoles.DefaultIfEmpty()
                           join role in _context.Roles on userRole.RoleId equals role.Id into roles
                           from role in roles.DefaultIfEmpty()
                           where user.Email.EndsWith("@vanlanguni.vn") && (role == null ||
                                                                           (role.NormalizedName != "ADVISOR" &&
                                                                            role.NormalizedName != "FACULTY"))
                           select user).Count();
        
        var noOfAdvisor = (from user in _context.ApplicationUsers
                          join userRole in _context.UserRoles on user.Id equals userRole.UserId into userRoles
                          from userRole in userRoles.DefaultIfEmpty()
                          join role in _context.Roles on userRole.RoleId equals role.Id into roles
                          from role in roles.DefaultIfEmpty()
                          where (role != null &&
                                 (role.NormalizedName == "ADVISOR"))
                          select user).Count();

        var noOfFaculty = (from user in _context.ApplicationUsers
                           join userRole in _context.UserRoles on user.Id equals userRole.UserId into userRoles
                           from userRole in userRoles.DefaultIfEmpty()
                           join role in _context.Roles on userRole.RoleId equals role.Id into roles
                           from role in roles.DefaultIfEmpty()
                           where (role != null &&
                                 (role.NormalizedName == "FACULTY"))
                           select user).Count();

        var noOfClasses = await _context.Classes.CountAsync();

        var latest5Years = await _context.AcademicPeriods.OrderBy(ap => ap.PeriodStart).Take(5).ToListAsync();

        List<int> plansEachYear = new List<int>();
        List<int> reportsEachYear = new List<int>();

        foreach (var year in latest5Years)
        {
            var thisYearPlanCount = await GetPlanNumberForYear(year.PeriodName);
            plansEachYear.Add(thisYearPlanCount);
        }

        foreach (var year in latest5Years)
        {
            var thisYearReportCount = await GetReportNumberForYear(year.PeriodName);
            reportsEachYear.Add(thisYearReportCount);
        }

            var viewmodel = new ShowStatisticViewModel
        {
            FacultyNumber = noOfFaculty,
            AdvisorNumber = noOfAdvisor,
            StudentNumber = noOfStudent,
            ClassNumber = noOfClasses,
            Years = latest5Years,
            PlanChart = plansEachYear,
            ReportChart = reportsEachYear
        };
        return View(viewmodel);
    }

   public async Task<int> GetPlanNumberForYear(string yearName)
   {
       // Perform your query, for example, counting the number of SemesterPlans for the specified year
       var planCount = await _context.SemesterPlans
           .Where(sp => sp.PeriodName == yearName) // Filter by the provided year (adjust this based on your condition)
           .CountAsync();

       return planCount;
   }

   public async Task<int> GetReportNumberForYear(string yearName)
   {
       // Perform your query, for example, counting the number of SemesterPlans for the specified year
       var reportCount = await _context.SemesterReports
           .Where(sp => sp.PeriodName == yearName) // Filter by the provided year (adjust this based on your condition)
           .CountAsync();

       return reportCount;
   }

   //Year
   public async Task<IActionResult> StatisticsClassByYear(int pageIndex = 1, int pageSize = 20, string fromTerm = null, string toTerm = null)
    {
        ViewData["page"] = "StatisticsClassByYear";
        ViewBag.SelectedFromYear = fromTerm;
        ViewBag.SelectedToYear = toTerm;

        // Lấy danh sách niên khóa từ database
        var allTerms = await _context.Classes
            .Select(c => c.Term)
            .Distinct()
            .OrderBy(t => t)
            .ToListAsync();

        // Lấy thông tin lớp từ database
        var classesQuery = _context.Classes
            .Include(c => c.Advisor)
            .Include(c => c.Students)
            .Select(c => new Class
            {
                ClassId = c.ClassId,
                Term = c.Term,
                AdvisorId = c.AdvisorId,
                Advisor = c.Advisor ?? new ApplicationUser { Email = "N/A", FullName = "Chưa bổ nhiệm" },
            });

        // Lọc dữ liệu theo khoảng niên khóa (nếu có)
        if (!string.IsNullOrEmpty(fromTerm))
        {
            classesQuery = classesQuery.Where(c => string.Compare(c.Term, fromTerm) >= 0);
        }
        if (!string.IsNullOrEmpty(toTerm))
        {
            classesQuery = classesQuery.Where(c => string.Compare(c.Term, toTerm) <= 0);
        }

        // Phân trang
        var classes = await classesQuery.ToListAsync();
        var paginatedClasses = PaginatedList<Class>.Create(classes, pageIndex, pageSize);

        var viewModel = new ClassListViewModel
        {
            Classes = paginatedClasses
        };

        // Gửi dữ liệu niên khóa đã chọn về view
        ViewBag.AllTerms = allTerms;
        ViewBag.CurrentFromTerm = fromTerm;
        ViewBag.CurrentToTerm = toTerm;

        return View(viewModel);
    }
    //export year
    [HttpGet] 
    public async Task<IActionResult> ExportStatisticsToExcel(string fromTerm = null, string toTerm = null)
{
    // Lấy dữ liệu theo niên khóa
    var classesQuery = _context.Classes
        .Include(c => c.Advisor)
        .Include(c => c.Students)
        .Select(c => new
        {
            ClassId = c.ClassId,
            Term = c.Term,
            AdvisorName = c.Advisor != null ? c.Advisor.FullName : "Chưa bổ nhiệm",
            AdvisorEmail = c.Advisor != null ? c.Advisor.Email : "N/A",
        });

    if (!string.IsNullOrEmpty(fromTerm))
    {
        classesQuery = classesQuery.Where(c => string.Compare(c.Term, fromTerm) >= 0);
    }
    if (!string.IsNullOrEmpty(toTerm))
    {
        classesQuery = classesQuery.Where(c => string.Compare(c.Term, toTerm) <= 0);
    }

    var classList = await classesQuery.ToListAsync();
    
    string fromKhoa = !string.IsNullOrEmpty(fromTerm)
        ? $"K{int.Parse(fromTerm.Split('-')[0]) - 1994}"
        : "Không xác định";
    string toKhoa = !string.IsNullOrEmpty(toTerm)
        ? $"K{int.Parse(toTerm.Split('-')[0]) - 1994}"
        : "Không xác định";

    // Tạo file Excel
    using var package = new ExcelPackage();
    var worksheet = package.Workbook.Worksheets.Add("Statistics");

    // Add university headers
    worksheet.Cells["B1:D1"].Merge = true;
    worksheet.Cells["B2:D2"].Merge = true;
    worksheet.Cells["B1"].Value = "TRƯỜNG ĐẠI HỌC VĂN LANG";
    worksheet.Cells["B2"].Value = "KHOA CÔNG NGHỆ THÔNG TIN";
    worksheet.Cells["B4:E4"].Merge = true;
    worksheet.Cells["B4"].Value = "THỐNG KÊ DANH SÁCH CVHT THEO NIÊN KHÓA";
    worksheet.Cells["B4"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
    
    // Add logo image
    string imagePath = Path.Combine(_webHostEnvironment.WebRootPath, "images", "logo.png");

    if (System.IO.File.Exists(imagePath))
    {
        var picture = worksheet.Drawings.AddPicture("Logo", new FileInfo(imagePath));
        picture.SetPosition(0, 5, 4, 90); 
        picture.SetSize(16);

    }


    
    // Add term range
    worksheet.Cells["B5"].Value = "TỪ KHÓA:";
    worksheet.Cells["C5"].Value = fromKhoa; 
    worksheet.Cells["D5"].Value = "ĐẾN KHÓA:";
    worksheet.Cells["E5"].Value = toKhoa; 
    
    // Style the headers
    using (var range = worksheet.Cells["B1,B2,B4"])
    {
        range.Style.Font.Bold = true;
        range.Style.Font.Size = 12;
    }



    // Add table headers
    worksheet.Cells["B6"].Value = "Mã Lớp";
    worksheet.Cells["C6"].Value = "Niên Khóa";
    worksheet.Cells["D6"].Value = "Tên CVHT";
    worksheet.Cells["E6"].Value = "Email";

    // Style the table headers
    using (var range = worksheet.Cells["B6:E6"])
    {
        range.Style.Fill.PatternType = ExcelFillStyle.Solid;
        range.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(192, 0, 0)); // Dark red
        range.Style.Font.Color.SetColor(Color.White);
        range.Style.Font.Bold = true;
        range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
    }
    using (var range = worksheet.Cells["B5,D5"])
    {
        range.Style.Fill.PatternType = ExcelFillStyle.Solid;
        range.Style.Font.Bold = true;
    }

    // Add data
    int row = 7;
    foreach (var item in classList)
    {
        worksheet.Cells[row, 2].Value = item.ClassId;
        worksheet.Cells[row, 3].Value = item.Term;
        worksheet.Cells[row, 4].Value = item.AdvisorName;
        worksheet.Cells[row, 5].Value = item.AdvisorEmail;
        
        // Style data rows
        using (var range = worksheet.Cells[row, 2, row, 5])
        {
            range.Style.Border.Top.Style = ExcelBorderStyle.Thin;
            range.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
            range.Style.Border.Left.Style = ExcelBorderStyle.Thin;
            range.Style.Border.Right.Style = ExcelBorderStyle.Thin;
        }
        
        row++;
    }
    
    // Thêm 3 dòng sau dữ liệu
    worksheet.Cells[row, 3].Value = "TP. Hồ Chí Minh";
    worksheet.Cells[row, 3, row, 5].Merge = true;  
    worksheet.Cells[row, 3].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
    worksheet.Cells[row, 3].Style.Font.Italic = true;

    row++;  // Tăng lên dòng tiếp theo
    worksheet.Cells[row, 3].Value = DateTime.Now.ToString("dd/MM/yyyy");  
    worksheet.Cells[row, 3, row, 5].Merge = true;  
    worksheet.Cells[row, 3].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
    worksheet.Cells[row, 3].Style.Font.Italic = true;

    row++;  // Tăng lên dòng tiếp theo
    worksheet.Cells[row, 3].Value = "Người lập danh sách (ký, ghi rõ họ tên)";
    worksheet.Cells[row, 3, row, 5].Merge = true;  
    worksheet.Cells[row, 3].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
    worksheet.Cells[row, 3].Style.Font.Italic = true;


    // Auto-fit columns
    worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

    // Add borders to the table
    using (var range = worksheet.Cells[6, 2, row - 3, 5])
    {
        range.Style.Border.Top.Style = ExcelBorderStyle.Thin;
        range.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
        range.Style.Border.Left.Style = ExcelBorderStyle.Thin;
        range.Style.Border.Right.Style = ExcelBorderStyle.Thin;
    }

    var stream = new MemoryStream();
    package.SaveAs(stream);
    stream.Position = 0;

    var fileName = $"ThongKeCVHT_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
    return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
}
    
    //Major
    public class StatisticResult
{
    public string DepartmentCode { get; set; }
    public string DepartmentName { get; set; }
    public int ClassCount { get; set; }
}
    private async Task<List<StatisticResult>> GetStatistics(string fromYear, string toYear)
{
    var fixedDepartments = new List<string>
    {
        "7480104 - Hệ thống Thông tin (CTTC)",
        "7480102 - Mạng máy tính và truyền thông dữ liệu (CTTC)",
        "7480201 - Công nghệ Thông Tin (CTTC)",
        "7480201 - Công nghệ Thông Tin (CTĐB)"
    };

    var departmentList = fixedDepartments.Select(d =>
    {
        var parts = d.Split(" - ");
        return new
        {
            DepartmentCode = parts[0].Trim(),
            DepartmentName = parts[1].Trim()
        };
    }).ToList();

    int? fromYearStart = null, toYearEnd = null;
    if (!string.IsNullOrEmpty(fromYear) && fromYear.Contains('-'))
    {
        var years = fromYear.Split('-');
        fromYearStart = int.Parse(years[0]);
    }
    if (!string.IsNullOrEmpty(toYear) && toYear.Contains('-'))
    {
        var years = toYear.Split('-');
        toYearEnd = int.Parse(years[1]);
    }


    var statistics = new List<StatisticResult>();

    foreach (var department in departmentList)
    {
        var classesForDepartment = await _context.Classes
            .Where(c => c.Department != null &&
                        c.Department.Trim() == $"{department.DepartmentCode} - {department.DepartmentName}")
            .ToListAsync();

        var filteredClasses = classesForDepartment
            .Where(c => (!fromYearStart.HasValue || int.Parse(c.Term.Split('-')[0]) >= fromYearStart) &&
                        (!toYearEnd.HasValue || int.Parse(c.Term.Split('-')[1]) <= toYearEnd));



        statistics.Add(new StatisticResult
        {
            DepartmentCode = department.DepartmentCode,
            DepartmentName = department.DepartmentName,
            ClassCount = filteredClasses.Count()
        });
    }

    return statistics;
} 
    // Major
    public async Task<IActionResult> StatisticsClassByMajor(string fromYear = null, string toYear = null)
{
    ViewData["page"] = "StatisticsClassByMajor";
    ViewBag.SelectedFromYear = fromYear;
    ViewBag.SelectedToYear = toYear;
    // Lấy danh sách niên khóa
    var allTerms = await _context.Classes
        .Select(c => c.Term)
        .Distinct()
        .OrderBy(t => t)
        .ToListAsync();
    ViewBag.AllTerms = allTerms;
    
    // Tính toán dữ liệu thống kê
    var statistics = await GetStatistics(fromYear, toYear);

    // Lưu dữ liệu vào Session
    HttpContext.Session.SetString("Statistics", JsonConvert.SerializeObject(statistics));
    HttpContext.Session.SetString("FromYear", fromYear ?? string.Empty);
    HttpContext.Session.SetString("ToYear", toYear ?? string.Empty);


    // Truyền dữ liệu thống kê vào ViewBag
    ViewBag.Statistics = statistics;

    return View();
} 
    // Export Major
    [HttpGet] 
    public async Task<IActionResult> ExportStatisticsClassByMajor()
{
    string fromYear = HttpContext.Session.GetString("FromYear");
    string toYear = HttpContext.Session.GetString("ToYear");
    var statisticss = await GetStatistics(fromYear, toYear);

    // Kiểm tra nếu dữ liệu không tồn tại
    if (string.IsNullOrEmpty(fromYear) || string.IsNullOrEmpty(toYear))
    {
        return BadRequest("Không có thông tin niên khóa để xuất.");
    }
    // Lấy dữ liệu thống kê từ Session
    var statisticsJson = HttpContext.Session.GetString("Statistics");
    if (string.IsNullOrEmpty(statisticsJson))
    {
        return BadRequest("Không có dữ liệu để xuất.");
    }
    var statistics = JsonConvert.DeserializeObject<List<StatisticResult>>(statisticsJson);
    
    
    string fromKhoa = !string.IsNullOrEmpty(fromYear)
        ? $"K{int.Parse(fromYear.Split('-')[0]) - 1994}"
        : "Không xác định";
    string toKhoa = !string.IsNullOrEmpty(toYear)
        ? $"K{int.Parse(toYear.Split('-')[0]) - 1994}"
        : "Không xác định";


    // Tạo file Excel
    using var package = new ExcelPackage();
    var worksheet = package.Workbook.Worksheets.Add("Statistics");
    
    // Add university headers
    worksheet.Cells["B1:D1"].Merge = true;
    worksheet.Cells["B2:D2"].Merge = true;
    worksheet.Cells["B1"].Value = "TRƯỜNG ĐẠI HỌC VĂN LANG";
    worksheet.Cells["B2"].Value = "KHOA CÔNG NGHỆ THÔNG TIN";
    worksheet.Cells["B4:E4"].Merge = true;
    worksheet.Cells["B4"].Value = "THỐNG KÊ SỐ LỚP THEO NGÀNH";
    worksheet.Cells["B4"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
    
    // Add logo image
    string imagePath = Path.Combine(_webHostEnvironment.WebRootPath, "images", "logo.png");

    if (System.IO.File.Exists(imagePath))
    {
        var picture = worksheet.Drawings.AddPicture("Logo", new FileInfo(imagePath));
        picture.SetPosition(0, 5, 3, 300); 
        picture.SetSize(16);

    }
    
    // Add term range
    worksheet.Cells["B5"].Value = "TỪ KHÓA:";
    worksheet.Cells["C5"].Value = fromKhoa; 
    worksheet.Cells["D5"].Value = "ĐẾN KHÓA:";
    worksheet.Cells["E5"].Value = toKhoa; 
    
    // Style the headers
    using (var range = worksheet.Cells["B1,B2,B4"])
    {
        range.Style.Font.Bold = true;
        range.Style.Font.Size = 12;
    }
    
    worksheet.Cells["B6"].Value = "#";
    worksheet.Cells["C6"].Value = "Mã Ngành";
    worksheet.Cells["D6"].Value = "Tên Ngành";
    worksheet.Cells["E6"].Value = "Số Lớp";
    
    using (var range = worksheet.Cells["B6:E6"])
    {
        range.Style.Fill.PatternType = ExcelFillStyle.Solid;
        range.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(192, 0, 0)); // Dark red
        range.Style.Font.Color.SetColor(Color.White);
        range.Style.Font.Bold = true;
        range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
    }
    using (var range = worksheet.Cells["B5,D5"])
    {
        range.Style.Fill.PatternType = ExcelFillStyle.Solid;
        range.Style.Font.Bold = true;
    }
    
    int row = 7; 
    for (int i = 0; i < statistics.Count; i++)
    {
        var stat = statistics[i];
        worksheet.Cells[row, 2].Value = i + 1;
        worksheet.Cells[row, 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
        worksheet.Cells[row, 3].Value = stat.DepartmentCode;
        worksheet.Cells[row, 4].Value = stat.DepartmentName;
        worksheet.Cells[row, 5].Value = stat.ClassCount;
        worksheet.Cells[row, 5].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

        // Style cho các hàng dữ liệu
        using (var range = worksheet.Cells[row, 2, row, 5])
        {
            range.Style.Border.Top.Style = ExcelBorderStyle.Thin;
            range.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
            range.Style.Border.Left.Style = ExcelBorderStyle.Thin;
            range.Style.Border.Right.Style = ExcelBorderStyle.Thin;
        }

        row++;
    }
    // Thêm 3 dòng sau dữ liệu
    worksheet.Cells[row, 4].Value = "TP. Hồ Chí Minh";
    worksheet.Cells[row, 4, row, 5].Merge = true;  
    worksheet.Cells[row, 4].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
    worksheet.Cells[row, 4].Style.Font.Italic = true;

    row++;  // Tăng lên dòng tiếp theo
    worksheet.Cells[row, 4].Value = DateTime.Now.ToString("dd/MM/yyyy");  
    worksheet.Cells[row, 4, row, 5].Merge = true;  
    worksheet.Cells[row, 4].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
    worksheet.Cells[row, 4].Style.Font.Italic = true;

    row++;  // Tăng lên dòng tiếp theo
    worksheet.Cells[row, 4].Value = "Người lập danh sách (ký, ghi rõ họ tên)";
    worksheet.Cells[row, 4, row, 5].Merge = true;  
    worksheet.Cells[row, 4].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
    worksheet.Cells[row, 4].Style.Font.Italic = true;
// Thêm viền bao quanh toàn bộ bảng dữ liệu (từ hàng 6 đến cuối bảng)
    using (var range = worksheet.Cells[6, 2, row - 3, 5])
    {
        range.Style.Border.Top.Style = ExcelBorderStyle.Thin;
        range.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
        range.Style.Border.Left.Style = ExcelBorderStyle.Thin;
        range.Style.Border.Right.Style = ExcelBorderStyle.Thin;
    }

// Auto-fit columns
    worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();


    var stream = new MemoryStream();
    package.SaveAs(stream);
    stream.Position = 0;

    var fileName = $"ThongKeLopTheoNganh_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
    return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
}

   
    

    
    //Evalution
    public async Task<IActionResult> StatisticsEvalution(string fromTerm = null, string toTerm = null, int pageIndex = 1)
    {
        ViewData["page"] = "StatisticsEvalution";
        
        ViewBag.SelectedFromYear = fromTerm;
        ViewBag.SelectedToYear = toTerm;

        // Lấy tất cả các Term (Khóa học)
        var allTerms = await _context.Classes
            .Select(c => c.Term)
            .Distinct()
            .OrderBy(t => t)
            .ToListAsync();
        ViewBag.AllTerms = allTerms;
        ViewBag.CurrentFromTerm = fromTerm;
        ViewBag.CurrentToTerm = toTerm;

        // Lấy dữ liệu các báo cáo học kỳ
        var query = _context.SemesterReports
            .Include(sr => sr.Class)
            .ThenInclude(c => c.Advisor)
            .AsQueryable();

        // Áp dụng bộ lọc nếu có
        if (!string.IsNullOrEmpty(fromTerm) && !string.IsNullOrEmpty(toTerm))
        {
            query = query.Where(sr => string.Compare(sr.Class.Term, fromTerm) >= 0 &&
                                      string.Compare(sr.Class.Term, toTerm) <= 0);
        }

        // Phân trang
        int pageSize = 10; // Số lượng bản ghi mỗi trang
        var semesterReports = await PaginatedList<SemesterReport>.CreateAsync(query, pageIndex, pageSize);

        // Tạo ViewModel
        var viewModel = new StatisticsEvalutionViewModel
        {
            SemesterReports = semesterReports
        };

        return View(viewModel);
    }
    //export evalution
    [HttpGet]
     public async Task<IActionResult> ExportStatisticsEvalution(string fromTerm, string toTerm)
    {
        // Lấy dữ liệu các báo cáo học kỳ theo bộ lọc
        var query = _context.SemesterReports
            .Include(sr => sr.Class)
            .ThenInclude(c => c.Advisor)
            .AsQueryable();

        if (!string.IsNullOrEmpty(fromTerm) && !string.IsNullOrEmpty(toTerm))
        {
            query = query.Where(sr => string.Compare(sr.Class.Term, fromTerm) >= 0 &&
                                      string.Compare(sr.Class.Term, toTerm) <= 0);
        }

        var semesterReports = await query.ToListAsync();
        
        string fromKhoa = !string.IsNullOrEmpty(fromTerm)
            ? $"K{int.Parse(fromTerm.Split('-')[0]) - 1994}"
            : "Không xác định";
        string toKhoa = !string.IsNullOrEmpty(toTerm)
            ? $"K{int.Parse(toTerm.Split('-')[0]) - 1994}"
            : "Không xác định";

        // Tạo file Excel
        using (var package = new ExcelPackage())
        {
            var worksheet = package.Workbook.Worksheets.Add("Statistics");
            
            // Add university headers
            worksheet.Cells["B1:D1"].Merge = true;
            worksheet.Cells["B2:D2"].Merge = true;
            worksheet.Cells["B1"].Value = "TRƯỜNG ĐẠI HỌC VĂN LANG";
            worksheet.Cells["B2"].Value = "KHOA CÔNG NGHỆ THÔNG TIN";
            worksheet.Cells["B4:E4"].Merge = true;
            worksheet.Cells["B4"].Value = "THỐNG KÊ ĐÁNH GIÁ BÁO CÁO";
            worksheet.Cells["B4"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
    
            // Add logo image
            string imagePath = Path.Combine(_webHostEnvironment.WebRootPath, "images", "logo.png");

            if (System.IO.File.Exists(imagePath))
            {
                var picture = worksheet.Drawings.AddPicture("Logo", new FileInfo(imagePath));
                picture.SetPosition(0, 5, 3, 300); 
                picture.SetSize(16);

            }
    
            // Add term range
            worksheet.Cells["B5"].Value = "TỪ KHÓA:";
            worksheet.Cells["C5"].Value = fromKhoa; 
            worksheet.Cells["D5"].Value = "ĐẾN KHÓA:";
            worksheet.Cells["E5"].Value = toKhoa; 
    
            // Style the headers
            using (var range = worksheet.Cells["B1,B2,B4"])
            {
                range.Style.Font.Bold = true;
                range.Style.Font.Size = 12;
            }
            
            // Add table headers
            worksheet.Cells["B6"].Value = "Mã Lớp";
            worksheet.Cells["C6"].Value = "CVHT";
            worksheet.Cells["D6"].Value = "Năm Học";
            worksheet.Cells["E6"].Value = "Xếp Loại";
            
            // Style the table headers
            using (var range = worksheet.Cells["B6:E6"])
            {
                range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                range.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(192, 0, 0)); // Dark red
                range.Style.Font.Color.SetColor(Color.White);
                range.Style.Font.Bold = true;
                range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            }
            using (var range = worksheet.Cells["B5,D5"])
            {
                range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                range.Style.Font.Bold = true;
            }

            // Điền dữ liệu vào các hàng
            int row = 7;  // Dữ liệu bắt đầu từ hàng thứ 3
            foreach (var report in semesterReports)
            {
                worksheet.Cells[row, 2].Value = report.ClassId;
                worksheet.Cells[row, 3].Value = report.Class.Advisor?.FullName ?? report.Class.Advisor?.Email;
                worksheet.Cells[row, 4].Value = report.PeriodName;
                worksheet.Cells[row, 5].Value = report.FacultyRanking;
                using (var range = worksheet.Cells[row, 2, row, 5])
                {
                    range.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                    range.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                    range.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                    range.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                }
                row++;
            }
            // Thêm 3 dòng sau dữ liệu
            worksheet.Cells[row, 3].Value = "TP. Hồ Chí Minh";
            worksheet.Cells[row, 3, row, 5].Merge = true;  
            worksheet.Cells[row, 3].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            worksheet.Cells[row, 3].Style.Font.Italic = true;

            row++;  // Tăng lên dòng tiếp theo
            worksheet.Cells[row, 3].Value = DateTime.Now.ToString("dd/MM/yyyy");  
            worksheet.Cells[row, 3, row, 5].Merge = true;  
            worksheet.Cells[row, 3].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            worksheet.Cells[row, 3].Style.Font.Italic = true;

            row++;  // Tăng lên dòng tiếp theo
            worksheet.Cells[row, 3].Value = "Người lập danh sách (ký, ghi rõ họ tên)";
            worksheet.Cells[row, 3, row, 5].Merge = true;  
            worksheet.Cells[row, 3].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            worksheet.Cells[row, 3].Style.Font.Italic = true;
            
            worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

            
            var stream = new MemoryStream();
            await package.SaveAsAsync(stream);
            stream.Position = 0;

            
            var fileName = $"ThongKeDanhGia_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }
    }
}


    }
    
    
