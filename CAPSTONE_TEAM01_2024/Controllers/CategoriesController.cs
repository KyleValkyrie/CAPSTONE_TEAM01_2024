using CAPSTONE_TEAM01_2024.Models;
using CAPSTONE_TEAM01_2024.Utilities;
using CAPSTONE_TEAM01_2024.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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
        }//EndSemesterReport actions
        public IActionResult EndSemesterReport()
        {
            ViewData["page"] = "EndSemesterReport";
            return View();
        }
//ClassList actions
        //Render ClassList view
        public async Task<IActionResult> ClassList(int pageNumber = 1, int pageSize = 10)
        {
			ViewData["page"] = "ClassList";
			var classes = _context.Classes.AsQueryable();

            var paginatedClasses = await PaginatedList<Class>.CreateAsync(classes.OrderBy(c => c.ClassName), pageNumber, pageSize);

            var advisors = await _context.ProfileManagers
                .Where(pm => pm.VaiTro == "Cố vấn học tập")
                .Select(pm => new SelectListItem
                {
                    Value = pm.Id.ToString(),
                    Text = pm.TenDayDu
                }).ToListAsync();

            var years = await _context.AcademicPeriods
                .Select(ap => new SelectListItem
                {
                    Value = ap.PeriodId.ToString(),
                    Text = ap.PeriodName
				}).ToListAsync();

            var viewModel = new ClassListViewModel
            {
                Class = new Class(),
                Advisors = advisors,
                PaginatedClasses = paginatedClasses
            };

            return View(viewModel);
        }
		//Add new Class
		[HttpPost]
		public async Task<IActionResult> AddClass(string className, string advisor, int advisorId, string yearName, string department, int studentCount)
		{
			// Create a new AcademicPeriod object
			var newClass = new Class
			{
				ClassName = className,
				AdvisorName = advisor,
				AdvisorId=advisorId,
				YearName = yearName,
				Department = department,
				StudentCount = studentCount
			};
			// Add the new object to the database
			bool isFound = _context.Classes
					 .Where(r => r.ClassName == newClass.ClassName &&
								 r.AdvisorName == newClass.AdvisorName &&
								 r.AdvisorId == newClass.AdvisorId &&
								 r.YearName == newClass.YearName &&
								 r.Department == newClass.Department)
					 .Any();
			if (!isFound) //Can add
			{
				_context.Classes.Add(newClass);
				await _context.SaveChangesAsync();

				TempData["MessageAddClass"] = "Thêm lớp thành công!";
				TempData["MessageAddClassType"] = "success";

				// Redirect to the Page to see update
				return RedirectToAction("ClassList");
			}
			else
			{
				TempData["MessageAddClass"] = "Thông tin lớp đã tồn tại!";
				TempData["MessageAddClassType"] = "danger";
				return RedirectToAction("ClassList");
			}
		}
		//Edit Class
		[HttpPost]
		public async Task<IActionResult> EditClass(int classId, string className, string advisor, int advisorId, string yearName, string department, int studentCount)
		{
			var modifiedClass = await _context.Classes.FindAsync(classId);
			// Check if another record with same info exist
			var existingClass = await _context.Classes
				.FirstOrDefaultAsync(cl => cl.ClassName == className &&
										   cl.AdvisorName == advisor &&
										   cl.AdvisorId == advisorId &&
										   cl.YearName == yearName &&
										   cl.Department == department &&
										   cl.StudentCount == studentCount);
			if (modifiedClass != null && existingClass == null)
			{
				TempData["MessageEditClass"] = "Cập nhật lớp thành công!";
				TempData["MessageEditClassType"] = "success";
				modifiedClass.ClassName = className;
				modifiedClass.AdvisorName = advisor;
				modifiedClass.AdvisorId = advisorId;
				modifiedClass.YearName = yearName;
				modifiedClass.Department = department;
				modifiedClass.StudentCount = studentCount;
				await _context.SaveChangesAsync();
				return RedirectToAction("ClassList");
			}
			else
			{
				TempData["MessageEditClass"] = "Trùng thời gian, cập nhật đã bị hủy!";
				TempData["MessageEditClassType"] = "danger";
				return RedirectToAction("ClassList");
			}
		}
		//Delete Class
		[HttpPost]
		public async Task<IActionResult> DeleteClass(int classId)
		{
			var classToDelete = await _context.Classes.FindAsync(classId);
			if (classToDelete != null)
			{
				_context.Classes.Remove(classToDelete);
				await _context.SaveChangesAsync();
				// Redirect to the Page to see update
				TempData["MessageDeleteClass"] = "Xóa lớp thành công!";
				TempData["MessageDeleteClassType"] = "success";
				return RedirectToAction("ClassList");
			}
			else
			{
				TempData["MessageDeleteClass"] = "Xảy ra lỗi khi xóa!";
				TempData["MessageDeleteClassType"] = "danger";
				return RedirectToAction("ClassList");
			}
		}
		//ClassInfo actions
		//Render ClassInfo view
		public IActionResult ClassInfo()
        {
            ViewData["page"] = "ClassInfo";
            return View();
        }
//SchoolYear actions
		//Render SchoolYear view
		public async Task<IActionResult> SchoolYear(int pageNumber = 1, int pageSize = 10)
		{
			ViewData["page"] = "SchoolYear";
			var periods = _context.AcademicPeriods.AsQueryable();

			var paginatedPeriods = await PaginatedList<AcademicPeriod>.CreateAsync(periods.OrderBy(p => p.PeriodStart), pageNumber, pageSize);

			var model = new SchoolYearViewModel
			{
				PaginatedPeriods = paginatedPeriods
			};

			return View(model);
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


