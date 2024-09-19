using Microsoft.AspNetCore.Mvc;

namespace CAPSTONE_TEAM01_2024.Controllers
{
    public class ClassManagerController : Controller
    {
        public IActionResult ClassList()
        {
            return View();
        }
    }
}
