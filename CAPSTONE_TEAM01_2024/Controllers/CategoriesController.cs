using CAPSTONE_TEAM01_2024.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CAPSTONE_TEAM01_2024.Controllers
{
    public class CategoriesController:Controller
    {
// Database
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
//SchoolYear actions
        //Add new Academic Year
        [HttpPost]
        public async Task<IActionResult> AddAcademicYear(int year, int semester)
        {
            // Create a new AcademicPeriod object
            var academicPeriod = new AcademicPeriod
            {
                Year = year,
                Semester = semester
            };
            // Add the new object to the database
            bool isFound = _context.AcademicPeriods
                     .Where(r => r.Year == academicPeriod.Year && r.Semester == academicPeriod.Semester)
                     .Any();
            if (!isFound) //Can add
            {
                _context.AcademicPeriods.Add(academicPeriod);
                await _context.SaveChangesAsync();

                TempData["MessageAddYear"] = "Thêm năm học thành công!";
                TempData["MessageAddType"] = "success";

                // Redirect to the Page to see update
                return RedirectToAction("SchoolYear");
            }
            else
            {
                TempData["MessageAddYear"] = "Năm học và học kì đã tồn tại!";
                TempData["MessageAddType"] = "danger";
                return RedirectToAction("SchoolYear");
            }
        }
        //Delete Academic Year
        [HttpPost]
        public async Task<IActionResult> DeleteAcademicYear(int id)
        {
            var academicPeriod = await _context.AcademicPeriods.FindAsync(id);
            if (academicPeriod != null)
            {
                _context.AcademicPeriods.Remove(academicPeriod);
                await _context.SaveChangesAsync();
            }
            // Redirect to the Page to see update
            return RedirectToAction("SchoolYear");
        }
        //Edit Academic Year
        [HttpPost]
        public async Task<IActionResult> EditAcademicYear(int id, int year, int semester)
        {
            var academicPeriod = await _context.AcademicPeriods.FindAsync(id);
            // Check if another record with the same year and semester exists
            var existingPeriod = await _context.AcademicPeriods
                .FirstOrDefaultAsync(ap => ap.Year == year && ap.Semester == semester && ap.Id != id);
            if (academicPeriod != null && existingPeriod==null) 
            {
                TempData["MessageEditYear"] = "Cập nhật thành công!";
                TempData["MessageEditType"] = "success";
                academicPeriod.Year = year;
                academicPeriod.Semester = semester;
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
        //Render SchoolYear
        public async Task<IActionResult> SchoolYear()
        {
            ViewData["page"] = "SchoolYear";
            var academicPeriods = await _context.AcademicPeriods.ToListAsync();
            return View(academicPeriods);
        }
//SemesterPlan actions
        public IActionResult SemesterPlan()
        {
            ViewData["page"] = "SemesterPlan";
            return View();
        }
    }
}
