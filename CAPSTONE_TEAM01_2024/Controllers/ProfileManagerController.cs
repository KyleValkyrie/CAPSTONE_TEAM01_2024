using Microsoft.AspNetCore.Mvc;

namespace CAPSTONE_TEAM01_2024.Controllers
{
    public class ProfileManagerController: Controller
    {
// Database Context
        private readonly ApplicationDbContext _context;
        public ProfileManagerController(ApplicationDbContext context)
        {
            _context = context;
        }
// StudentProfiles actions
    //Render view
    public IActionResult StudentProfiles()
        {
            ViewData["page"] = "StudentProfiles";
            return View();
        }
// AdvisorProfiles actions
    //Render view
        public IActionResult AdvisorProfiles()
        {
            ViewData["page"] = "AdvisorProfiles";
            return View();
        }
    }
}
