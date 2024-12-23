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
namespace CAPSTONE_TEAM01_2024.Controllers
{
   public class StatisticsController : Controller
{
    private readonly ApplicationDbContext _context;

    // Constructor để inject DbContext
    public StatisticsController(ApplicationDbContext context)
    {
        _context = context;
    }

    public IActionResult ShowStatistics()
    {
        ViewData["page"] = "ShowStatistics";
        return View();
    }
    //Year
    public async Task<IActionResult> StatisticsClassByYear(int pageIndex = 1, int pageSize = 20, string fromTerm = null, string toTerm = null)
    {
        ViewData["page"] = "StatisticsClassByYear";

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

    // Tạo file Excel
    using var package = new ExcelPackage();
    var worksheet = package.Workbook.Worksheets.Add("Statistics");

    // Định nghĩa tiêu đề
    worksheet.Cells[1, 1].Value = "Mã Lớp";
    worksheet.Cells[1, 2].Value = "Niên Khóa";
    worksheet.Cells[1, 3].Value = "Tên CVHT";
    worksheet.Cells[1, 4].Value = "Email";

    // Thêm dữ liệu
    for (int i = 0; i < classList.Count; i++)
    {
        var row = i + 2;
        worksheet.Cells[row, 1].Value = classList[i].ClassId;
        worksheet.Cells[row, 2].Value = classList[i].Term;
        worksheet.Cells[row, 3].Value = classList[i].AdvisorName;
        worksheet.Cells[row, 4].Value = classList[i].AdvisorEmail;
    }
    
    using (var range = worksheet.Cells[1, 1, 1, 5])
    {
        range.Style.Font.Bold = true;
        range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
        range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
        range.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
    }
    
    worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
    
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
    if (!string.IsNullOrEmpty(fromYear))
    {
        var years = fromYear.Split('-');
        fromYearStart = int.Parse(years[0]);
    }
    if (!string.IsNullOrEmpty(toYear))
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

    // Truyền dữ liệu thống kê vào ViewBag
    ViewBag.Statistics = statistics;

    return View();
} 
    // Export Major
    [HttpGet] 
    public async Task<IActionResult> ExportStatisticsClassByMajor(string fromYear = null, string toYear = null)
{
    // Lấy dữ liệu thống kê từ Session
    var statisticsJson = HttpContext.Session.GetString("Statistics");
    if (string.IsNullOrEmpty(statisticsJson))
    {
        return BadRequest("Không có dữ liệu để xuất.");
    }

    var statistics = JsonConvert.DeserializeObject<List<StatisticResult>>(statisticsJson);

    // Tạo file Excel
    using var package = new ExcelPackage();
    var worksheet = package.Workbook.Worksheets.Add("Statistics");

    worksheet.Cells[1, 1].Value = "Mã Ngành";
    worksheet.Cells[1, 2].Value = "Tên Ngành";
    worksheet.Cells[1, 3].Value = "Số Lớp";

    for (int i = 0; i < statistics.Count; i++)
    {
        var stat = statistics[i];
        worksheet.Cells[i + 2, 1].Value = stat.DepartmentCode;
        worksheet.Cells[i + 2, 2].Value = stat.DepartmentName;
        worksheet.Cells[i + 2, 3].Value = stat.ClassCount;
    }

    using (var range = worksheet.Cells[1, 1, 1, 3])
    {
        range.Style.Font.Bold = true;
        range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
        range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
        range.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
    }

    worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

    var stream = new MemoryStream();
    package.SaveAs(stream);
    stream.Position = 0;

    var fileName = $"ThongKeLopTheoNganh_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
    return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
}

   
    

    
    //Evalution
    public async Task<IActionResult> StatisticsEvalution()
    {
        ViewData["page"] = "StatisticsEvalution";
        var allTerms = await _context.Classes
            .Select(c => c.Term)
            .Distinct()
            .OrderBy(t => t)
            .ToListAsync();
        
        ViewBag.AllTerms = allTerms;
        return View();
    }
}

}
