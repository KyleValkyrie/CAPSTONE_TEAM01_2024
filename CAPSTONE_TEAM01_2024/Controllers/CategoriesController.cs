using CAPSTONE_TEAM01_2024.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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
        public IActionResult ClassList()
        {
            ViewData["page"] = "ClassList";
            return View();
        }
//ClassInfo actions
        public IActionResult ClassInfo()
        {
            ViewData["page"] = "ClassInfo";
            return View();
        }
//SchoolYear actions
		//Render SchoolYear view
        public async Task<IActionResult> SchoolYear()
        {
            ViewData["page"] = "SchoolYear";
			var academicPeriods = await _context.AcademicPeriods.ToListAsync();
			return View(academicPeriods);
        }
		//Edit Academic Year
		[HttpPost]
		public async Task<IActionResult> EditPeriod(int periodId, string periodName, DateTime periodStart, DateTime periodEnd)
		{
			var modifiedPeriod = await _context.AcademicPeriods.FindAsync(periodId);
			// Check if another record with the same year and semester exists
			var existingPeriod = await _context.AcademicPeriods
				.FirstOrDefaultAsync(ap => ap.PeriodName == periodName && 
										   ap.PeriodStart == periodStart && 
										   ap.PeriodEnd == periodEnd &&
										   ap.PeriodId != periodId);
			if (modifiedPeriod != null && existingPeriod == null)
			{
				TempData["MessageEditYear"] = "Cập nhật thành công!";
				TempData["MessageEditType"] = "success";
				modifiedPeriod.PeriodName = periodName;
				modifiedPeriod.PeriodStart = periodStart;
				modifiedPeriod.PeriodEnd = periodEnd;
				await _context.SaveChangesAsync();
				return RedirectToAction("SchoolYear");
			}
			else
			{
				TempData["MessageEditYear"] = "Trùng thời gian, cập nhật đã bị hủy!";
				TempData["MessageEditType"] = "danger";
				return RedirectToAction("SchoolYear");
			}
		}
		//Delete Academic Year
		[HttpPost]
		public async Task<IActionResult> DeletePeriod(int periodId)
		{
			var periodToDelete = await _context.AcademicPeriods.FindAsync(periodId);
			if (periodToDelete != null)
			{
				_context.AcademicPeriods.Remove(periodToDelete);
				await _context.SaveChangesAsync();
				// Redirect to the Page to see update
				TempData["MessageDeleteYear"] = "Xóa năm học thành công!";
				TempData["MessageDeleteType"] = "success";
				return RedirectToAction("SchoolYear");				
			}
			else
			{
				TempData["MessageDeleteYear"] = "Xảy ra lỗi khi xóa!";
				TempData["MessageDeleteType"] = "danger";
				return RedirectToAction("SchoolYear");
			}
			
		}
		//Add new Academic Year
		[HttpPost]
		public async Task<IActionResult> AddPeriod(string periodName, DateTime periodStart, DateTime periodEnd)
		{
			// Create a new AcademicPeriod object
			var newPeriod = new AcademicPeriod
			{
				PeriodName = periodName,
				PeriodStart = periodStart,
				PeriodEnd = periodEnd
			};
			// Add the new object to the database
			bool isFound = _context.AcademicPeriods
					 .Where(r => r.PeriodName == newPeriod.PeriodName && 
								 r.PeriodStart == newPeriod.PeriodStart &&
								 r.PeriodEnd == newPeriod.PeriodEnd)
					 .Any();
			if (!isFound) //Can add
			{
				_context.AcademicPeriods.Add(newPeriod);
				await _context.SaveChangesAsync();

				TempData["MessageAddYear"] = "Thêm năm học thành công!";
				TempData["MessageAddType"] = "success";

				// Redirect to the Page to see update
				return RedirectToAction("SchoolYear");
			}
			else
			{
				TempData["MessageAddYear"] = "Thông tin năm học đã tồn tại!";
				TempData["MessageAddType"] = "danger";
				return RedirectToAction("SchoolYear");
			}
		}
//SemesterPlan actions
		public IActionResult SemesterPlan()
		{
			ViewData["page"] = "SemesterPlan";
			return View();
		}
	}
}


