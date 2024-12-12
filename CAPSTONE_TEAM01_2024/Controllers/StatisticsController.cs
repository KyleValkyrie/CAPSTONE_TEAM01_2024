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
        public IActionResult StatisticsMajor()
        {
            ViewData["page"] = "StatisticsMajor";
            return View();
        }
        public IActionResult StatisticsRole()
        {
            ViewData["page"] = "StatisticsRole";
            return View();
        }
        public IActionResult StatisticsStatusStudent()
        {
            ViewData["page"] = "StatisticsStatusStudent";
            return View();
        }
    }
}
