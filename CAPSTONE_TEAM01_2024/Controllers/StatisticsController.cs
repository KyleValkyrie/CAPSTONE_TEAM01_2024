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
    }
}
