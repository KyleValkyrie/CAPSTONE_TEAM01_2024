using CAPSTONE_TEAM01_2024.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
            _context.AcademicPeriods.Add(academicPeriod);
            await _context.SaveChangesAsync();

            // Redirect to the Page to see update
            return RedirectToAction("SchoolYear");
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
            if (academicPeriod != null)
            {
                academicPeriod.Year = year;
                academicPeriod.Semester = semester;
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("SchoolYear");
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
