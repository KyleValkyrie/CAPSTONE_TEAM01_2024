using Microsoft.AspNetCore.Mvc;

namespace CAPSTONE_TEAM01_2024.Controllers
{
    public class ProfileManagerController: Controller
    {
        public IActionResult ProfileManager()
        {
            ViewData["page"] = "ProfileManager";
            return View();
        }
    }
}
