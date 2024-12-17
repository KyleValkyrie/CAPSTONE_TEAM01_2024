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
        

        // Thực hiện phân trang
        var classes = await classesQuery.ToListAsync();
        var paginatedClasses = PaginatedList<Class>.Create(classes, pageIndex, pageSize);

        var viewModel = new ClassListViewModel
        {
            Classes = paginatedClasses
        };

        ViewBag.AllTerms = allTerms;

        return View(viewModel);
    }



    public IActionResult StatisticsClassByMajor()
    {
        ViewData["page"] = "StatisticsClassByMajor";
        return View();
    }

    public IActionResult StatisticsEvalution()
    {
        ViewData["page"] = "StatisticsEvalution";
        return View();
    }
}

}
