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
    //Render ClassList view
        public IActionResult ClassList()
        {
			ViewData["page"] = "ClassList";
            return View();
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


