using Microsoft.AspNetCore.Mvc;

namespace CAPSTONE_TEAM01_2024.Controllers
{
    public class AnnouncementController :Controller
    {
// Database Context
        private readonly ApplicationDbContext _context;
        public AnnouncementController(ApplicationDbContext context)
        {
            _context = context;
        }
// AnnouncementList Actions
    //Render view
    public IActionResult AnnouncementList()
        {
            return View();
        }
    }
}
