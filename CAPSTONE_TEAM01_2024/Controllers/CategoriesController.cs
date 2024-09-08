using Microsoft.AspNetCore.Mvc;

namespace CAPSTONE_TEAM01_2024.Controllers
{
    public class CategoriesController:Controller
    {
        public IActionResult EndSemesterReport()
        {
            ViewData["page"] = "EndSemesterReport";
            return View();
        }

        public IActionResult SchoolYear()
        {
            ViewData["page"] = "SchoolYear";
            return View();
        }

        public IActionResult SemesterPlan()
        {
            ViewData["page"] = "SemesterPlan";
            return View();
        }

        public IActionResult Statistics()
        {
            ViewData["page"] = "Statistics";
            return View();
        }
    }
}
