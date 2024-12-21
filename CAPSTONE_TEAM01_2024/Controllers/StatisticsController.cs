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
using Microsoft.AspNetCore.Mvc;
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
    
    public async Task<IActionResult> StatisticsClassByMajor(string fromYear = null, string toYear = null)
{
    ViewData["page"] = "StatisticsClassByMajor";

    // Danh sách cố định các ngành
    var fixedDepartments = new List<string>
    {
        "7480104 - Hệ thống Thông tin (CTTC)",
        "7480102 - Mạng máy tính và truyền thông dữ liệu (CTTC)",
        "7480201 - Công nghệ Thông Tin (CTTC)",
        "7480201 - Công nghệ Thông Tin (CTĐB)"
    };

    // Tách mã ngành và tên ngành và loại bỏ dấu cách thừa
    var departmentList = fixedDepartments.Select(d =>
    {
        var parts = d.Split(" - ");
        return new
        {
            DepartmentCode = parts[0].Trim(),
            DepartmentName = parts[1].Trim()
        };
    }).ToList();

    // Lấy tất cả các niên khóa
    var allTerms = await _context.Classes
        .Select(c => c.Term)
        .Distinct()
        .OrderBy(t => t)
        .ToListAsync();
    ViewBag.AllTerms = allTerms;

    // Xử lý niên khóa lọc (fromYear và toYear)
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

    // Lấy dữ liệu từng ngành sử dụng Đoạn 1
    var statistics = new List<object>();

    foreach (var department in departmentList)
    {
        // Lấy danh sách các lớp của từng ngành
        var classesForDepartment = await _context.Classes
            .Where(c => c.Department != null &&
                        c.Department.Trim() == $"{department.DepartmentCode} - {department.DepartmentName}")
            .ToListAsync();

        // Lọc theo niên khóa (nếu có)
        var filteredClasses = classesForDepartment
            .Where(c => (!fromYearStart.HasValue || int.Parse(c.Term.Split('-')[0]) >= fromYearStart) &&
                        (!toYearEnd.HasValue || int.Parse(c.Term.Split('-')[1]) <= toYearEnd));

        // Thêm vào kết quả thống kê
        statistics.Add(new
        {
            DepartmentCode = department.DepartmentCode,
            DepartmentName = department.DepartmentName,
            ClassCount = filteredClasses.Count()
        });
    }

    // Truyền kết quả thống kê đến view
    ViewBag.Statistics = statistics;
    return View();
}

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
