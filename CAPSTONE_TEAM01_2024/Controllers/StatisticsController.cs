using Microsoft.AspNetCore.Mvc;
namespace CAPSTONE_TEAM01_2024.Controllers
{
    public class StatisticsController: Controller
    {
        public IActionResult ShowStatistics()
        {
            ViewData["page"] = "ShowStatistics";
            return View();
        }
        public IActionResult StatisticsClassByYear()
        {
            ViewData["page"] = "StatisticsClassByYear";
            return View();
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
