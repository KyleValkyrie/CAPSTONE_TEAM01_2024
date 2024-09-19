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

                TempData["Message"] = "Thêm năm học thành công!";
                TempData["MessageType"] = "success";

                // Redirect to the Page to see update
                return RedirectToAction("SchoolYear");
            }
            else
            {
                TempData["Message"] = "Năm học và học kì đã tồn tại!";
                TempData["MessageType"] = "danger";
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
            //var results = HttpContext.Session.GetString("QueryResults");
            //if (results != null)
            //{
            //    academicPeriods = JsonConvert.DeserializeObject<List<AcademicPeriod>>(results);
            //    ModelState.Clear();
            //}

            return View(academicPeriods);
        }

        [HttpPost]
        public JsonResult SetViewData(string value)
        {
            //physics 1
            int parsedValue;
            bool successfulParse = int.TryParse(value, out parsedValue);
            string academicPeriod = string.Empty;
            if (successfulParse)
            {
                var results = from record in _context.AcademicPeriods where EF.Functions.Like((string)(object)record.Year, $"%{parsedValue}%") select record;
                //HttpContext.Session.SetString("QueryResults", JsonConvert.SerializeObject(academicPeriod));
                academicPeriod = JsonConvert.SerializeObject(results);
            }
            return Json(academicPeriod);
            //return RedirectToAction("SchoolYear");
        }
        //SemesterPlan actions
        public IActionResult SemesterPlan()
        {
            ViewData["page"] = "SemesterPlan";
            return View();
        }
    }
}
